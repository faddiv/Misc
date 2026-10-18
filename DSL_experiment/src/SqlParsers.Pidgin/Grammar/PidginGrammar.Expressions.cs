using Pidgin;
using SqlParsers.Ast;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace SqlParsers.Pidgin;

internal sealed partial class PidginGrammar
{
    private readonly Rule<Expression> expression = new(nameof(expression));
    private readonly Rule<Expression> andExpression = new(nameof(andExpression));
    private readonly Rule<Expression> notExpression = new(nameof(notExpression));
    private readonly Rule<Expression> predicate = new(nameof(predicate));
    private readonly Rule<PredicateSuffix> predicateSuffix = new(nameof(predicateSuffix));
    private readonly Rule<BinaryOperator> comparisonOperator = new(nameof(comparisonOperator));
    private readonly Rule<Expression> additiveExpression = new(nameof(additiveExpression));
    private readonly Rule<Expression> multiplicativeExpression = new(nameof(multiplicativeExpression));
    private readonly Rule<Expression> unaryExpression = new(nameof(unaryExpression));
    private readonly Rule<Expression> primary = new(nameof(primary));
    private readonly Rule<Expression> caseExpression = new(nameof(caseExpression));
    private readonly Rule<Expression> castExpression = new(nameof(castExpression));
    private readonly Rule<FunctionArguments> functionArguments = new(nameof(functionArguments));
    private readonly Rule<DataType> dataType = new(nameof(dataType));
    private readonly Rule<IReadOnlyList<string>> qualifiedName = new(nameof(qualifiedName));
    private readonly Rule<string> identifier = new(nameof(identifier));

    private void DefineExpressions()
    {
        // expression ::= andExpression (OR andExpression)*
        expression.Definition = Map(
            Build.LeftAssociative,
            andExpression,
            K.Or.ThenReturn(BinaryOperator.Or).Then(andExpression, (op, right) => (op, right)).Many());

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.Definition = Map(
            Build.LeftAssociative,
            notExpression,
            K.And.ThenReturn(BinaryOperator.And).Then(notExpression, (op, right) => (op, right)).Many());

        // notExpression ::= NOT notExpression | predicate
        notExpression.Definition = OneOf(
            K.Not.Then(notExpression).Select(Build.Not),
            predicate);

        // predicate ::= additiveExpression predicateSuffix?
        predicate.Definition = Map(Build.Predicate, additiveExpression, predicateSuffix.Optional());

        // predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery
        //                                        | additiveExpression )
        //                   | IS NOT? NULL
        //                   | NOT? ( BETWEEN additiveExpression AND additiveExpression
        //                          | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
        //                          | LIKE additiveExpression (ESCAPE additiveExpression)? )
        //   A suffix is a function that completes the predicate once its left operand is known.
        var quantifier = OneOf(
            K.Any.ThenReturn(Quantifier.Any),
            K.Some.ThenReturn(Quantifier.Some),
            K.All.ThenReturn(Quantifier.All));
        var not = K.Not.ThenReturn(true).Or(Return(false));   // NOT?

        predicateSuffix.Definition = OneOf(
            Map(Build.ComparisonSuffix,
                comparisonOperator,
                OneOf(
                    Map(Build.QuantifiedComparison, quantifier, subquery),
                    additiveExpression.Select(Build.Comparison))),
            K.Is.Then(not).Before(K.Null)
                .Select(Build.IsNull),
            Map(Build.Negatable,
                not,
                OneOf(
                    Map(Build.Between, K.Between.Then(additiveExpression), K.And.Then(additiveExpression)),
                    K.In.Then(Lex.Parens(OneOf(
                        subqueryExpression.Select(Build.InSubquery),
                        expression.SeparatedAtLeastOnce(Lex.Comma).Select(Build.InList)))),
                    Map(Build.Like, K.Like.Then(additiveExpression), K.Escape.Then(additiveExpression).Optional()))));

        // comparisonOperator ::= '=' | '<>' | '<' | '>' | '<=' | '>='
        //   (two-character operators are tried first)
        comparisonOperator.Definition = OneOf(
            Lex.Operator("<>").ThenReturn(BinaryOperator.NotEqual),
            Lex.Operator("<=").ThenReturn(BinaryOperator.LessThanOrEqual),
            Lex.Operator(">=").ThenReturn(BinaryOperator.GreaterThanOrEqual),
            Lex.Operator("=").ThenReturn(BinaryOperator.Equal),
            Lex.Operator("<").ThenReturn(BinaryOperator.LessThan),
            Lex.Operator(">").ThenReturn(BinaryOperator.GreaterThan));

        // additiveExpression ::= multiplicativeExpression (('+' | '-' | '||') multiplicativeExpression)*
        var additiveOperator = OneOf(
            Lex.Operator("||").ThenReturn(BinaryOperator.Concat),
            Lex.Symbol('+').ThenReturn(BinaryOperator.Add),
            Lex.Symbol('-').ThenReturn(BinaryOperator.Subtract));
        additiveExpression.Definition = Map(
            Build.LeftAssociative,
            multiplicativeExpression,
            additiveOperator.Then(multiplicativeExpression, (op, right) => (op, right)).Many());

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        var multiplicativeOperator = OneOf(
            Lex.Star.ThenReturn(BinaryOperator.Multiply),
            Lex.Symbol('/').ThenReturn(BinaryOperator.Divide));
        multiplicativeExpression.Definition = Map(
            Build.LeftAssociative,
            unaryExpression,
            multiplicativeOperator.Then(unaryExpression, (op, right) => (op, right)).Many());

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        //   Written as two alternatives rather than Map(sign, unaryExpression): this is the hottest rule (every
        //   primary passes through it), and the extra Map/OneOf layers made the whole parser about 3% slower.
        unaryExpression.Definition = OneOf(
            Lex.Symbol('+').Then(unaryExpression).Select(Build.Plus),
            Lex.Symbol('-').Then(unaryExpression).Select(Build.Minus),
            primary);

        // primary ::= NUMBER
        //           | STRING
        //           | NULL
        //           | caseExpression
        //           | castExpression
        //           | EXISTS subquery
        //           | qualifiedName ('(' functionArguments? ')')?
        //           | '(' (subqueryExpression | expression) ')'
        var callArguments = Lex.Parens(functionArguments.Optional()).Select(Build.OrNoArguments);
        primary.Definition = OneOf(
            Lex.Number.Select(Build.Number),
            Lex.String.Select(Build.String),
            K.Null.ThenReturn(Build.Null),
            caseExpression,
            castExpression,
            K.Exists.Then(subquery).Select(Build.Exists),
            Map(Build.ColumnOrFunctionCall, qualifiedName, callArguments.Optional()),
            Lex.Parens(OneOf(
                subqueryExpression.Select(Build.ScalarSubquery),
                expression)));

        // caseExpression ::= CASE expression? (WHEN expression THEN expression)+ (ELSE expression)? END
        var whenClause = Map(Build.When, K.When.Then(expression), K.Then.Then(expression));
        caseExpression.Definition = K.Case
            .Then(Map(Build.Case, expression.Optional(), whenClause.AtLeastOnce(), K.Else.Then(expression).Optional()))
            .Before(K.End);

        // castExpression ::= CAST '(' expression AS dataType ')'
        castExpression.Definition = K.Cast.Then(Lex.Parens(Map(Build.Cast, expression.Before(K.As), dataType)));

        // functionArguments ::= '*' | setQuantifier? expression (',' expression)*
        functionArguments.Definition = OneOf(
            Lex.Star.ThenReturn(Build.StarArgument),
            Map(Build.Arguments, setQuantifier.Optional(), expression.SeparatedAtLeastOnce(Lex.Comma)));

        // dataType ::= IDENTIFIER+ ('(' INTEGER (',' INTEGER)* ')')?
        dataType.Definition = Map(
            Build.DataType,
            Lex.Identifier.AtLeastOnce(),
            Lex.Parens(Lex.Integer.SeparatedAtLeastOnce(Lex.Comma)).Optional());

        // qualifiedName ::= identifier ('.' identifier)*
        qualifiedName.Definition = identifier.SeparatedAtLeastOnce(Lex.Dot).Select(Build.List);

        // identifier ::= IDENTIFIER | QUOTED_IDENTIFIER
        identifier.Definition = OneOf(Lex.Identifier, Lex.QuotedIdentifier);
    }
}
