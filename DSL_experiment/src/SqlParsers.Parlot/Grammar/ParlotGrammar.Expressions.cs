using Parlot.Fluent;
using SqlParsers.Ast;

namespace SqlParsers.Parlot;

internal sealed partial class ParlotGrammar
{
    private readonly Rule<Expression> expression = new() { Name = "expression" };
    private readonly Rule<Expression> andExpression = new() { Name = "andExpression" };
    private readonly Rule<Expression> notExpression = new() { Name = "notExpression" };
    private readonly Rule<Expression> predicate = new() { Name = "predicate" };
    private readonly Rule<PredicateSuffix> predicateSuffix = new() { Name = "predicateSuffix" };
    private readonly Rule<BinaryOperator> comparisonOperator = new() { Name = "comparisonOperator" };
    private readonly Rule<Expression> additiveExpression = new() { Name = "additiveExpression" };
    private readonly Rule<Expression> multiplicativeExpression = new() { Name = "multiplicativeExpression" };
    private readonly Rule<Expression> unaryExpression = new() { Name = "unaryExpression" };
    private readonly Rule<Expression> primary = new() { Name = "primary" };
    private readonly Rule<Expression> caseExpression = new() { Name = "caseExpression" };
    private readonly Rule<Expression> castExpression = new() { Name = "castExpression" };
    private readonly Rule<FunctionArguments> functionArguments = new() { Name = "functionArguments" };
    private readonly Rule<DataType> dataType = new() { Name = "dataType" };
    private readonly Rule<IReadOnlyList<string>> qualifiedName = new() { Name = "qualifiedName" };
    private readonly Rule<string> identifier = new() { Name = "identifier" };

    private void DefineExpressions()
    {
        // expression ::= andExpression (OR andExpression)*
        expression.Definition = andExpression
            .And(ZeroOrMany(K.Or.Then(static _ => BinaryOperator.Or).And(andExpression)))
            .Then(Build.LeftAssociative);

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.Definition = notExpression
            .And(ZeroOrMany(K.And.Then(static _ => BinaryOperator.And).And(notExpression)))
            .Then(Build.LeftAssociative);

        // notExpression ::= NOT notExpression | predicate
        notExpression.Definition = OneOf(
            K.Not.SkipAnd(notExpression).Then(Build.Not),
            predicate);

        // predicate ::= additiveExpression predicateSuffix?
        predicate.Definition = additiveExpression
            .And(ZeroOrOne(predicateSuffix))
            .Then(Build.Predicate);

        // predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery
        //                                        | additiveExpression )
        //                   | IS NOT? NULL
        //                   | NOT? ( BETWEEN additiveExpression AND additiveExpression
        //                          | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
        //                          | LIKE additiveExpression (ESCAPE additiveExpression)? )
        //   A suffix is a function that completes the predicate once its left operand is known.
        var quantifier = OneOf(
            K.Any.Then(static _ => Quantifier.Any),
            K.Some.Then(static _ => Quantifier.Some),
            K.All.Then(static _ => Quantifier.All));
        var not = ZeroOrOne(K.Not.Then(static _ => true));   // NOT?  (false when absent)

        predicateSuffix.Definition = OneOf(
            comparisonOperator
                .And(OneOf(
                    quantifier.And(subquery).Then(Build.QuantifiedComparison),
                    additiveExpression.Then(Build.Comparison)))
                .Then(Build.ComparisonSuffix),
            K.Is.SkipAnd(not).AndSkip(K.Null)
                .Then(Build.IsNull),
            not
                .And(OneOf(
                    K.Between.SkipAnd(additiveExpression).AndSkip(K.And).And(additiveExpression).Then(Build.Between),
                    K.In.SkipAnd(Lex.Parens(OneOf(
                        subqueryExpression.Then(Build.InSubquery),
                        Separated(Lex.Comma, expression).Then(Build.InList)))),
                    K.Like.SkipAnd(additiveExpression).And(ZeroOrOne(K.Escape.SkipAnd(additiveExpression))).Then(Build.Like)))
                .Then(Build.Negatable));

        // comparisonOperator ::= '=' | '<>' | '<' | '>' | '<=' | '>='
        //   (two-character operators are tried first)
        comparisonOperator.Definition = OneOf(
            Lex.Operator("<>").Then(static _ => BinaryOperator.NotEqual),
            Lex.Operator("<=").Then(static _ => BinaryOperator.LessThanOrEqual),
            Lex.Operator(">=").Then(static _ => BinaryOperator.GreaterThanOrEqual),
            Lex.Operator("=").Then(static _ => BinaryOperator.Equal),
            Lex.Operator("<").Then(static _ => BinaryOperator.LessThan),
            Lex.Operator(">").Then(static _ => BinaryOperator.GreaterThan));

        // additiveExpression ::= multiplicativeExpression (('+' | '-' | '||') multiplicativeExpression)*
        var additiveOperator = OneOf(
            Lex.Operator("||").Then(static _ => BinaryOperator.Concat),
            Lex.Symbol('+').Then(static _ => BinaryOperator.Add),
            Lex.Symbol('-').Then(static _ => BinaryOperator.Subtract));
        additiveExpression.Definition = multiplicativeExpression
            .And(ZeroOrMany(additiveOperator.And(multiplicativeExpression)))
            .Then(Build.LeftAssociative);

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        var multiplicativeOperator = OneOf(
            Lex.Star.Then(static _ => BinaryOperator.Multiply),
            Lex.Symbol('/').Then(static _ => BinaryOperator.Divide));
        multiplicativeExpression.Definition = unaryExpression
            .And(ZeroOrMany(multiplicativeOperator.And(unaryExpression)))
            .Then(Build.LeftAssociative);

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        var sign = OneOf(
            Lex.Symbol('+').Then(static _ => UnaryOperator.Plus),
            Lex.Symbol('-').Then(static _ => UnaryOperator.Minus));
        unaryExpression.Definition = OneOf(
            sign.And(unaryExpression).Then(Build.Unary),
            primary);

        // primary ::= NUMBER
        //           | STRING
        //           | NULL
        //           | caseExpression
        //           | castExpression
        //           | EXISTS subquery
        //           | qualifiedName ('(' functionArguments? ')')?
        //           | '(' (subqueryExpression | expression) ')'
        var callArguments = Lex.Parens(ZeroOrOne(functionArguments)).Then(Build.OrNoArguments);
        primary.Definition = OneOf(
            Lex.Number.Then(Build.Number),
            Lex.String.Then(Build.String),
            K.Null.Then(static _ => Build.Null),
            caseExpression,
            castExpression,
            K.Exists.SkipAnd(subquery).Then(Build.Exists),
            qualifiedName.And(ZeroOrOne(callArguments)).Then(Build.ColumnOrFunctionCall),
            Lex.Parens(OneOf(
                subqueryExpression.Then(Build.ScalarSubquery),
                expression)));

        // caseExpression ::= CASE expression? (WHEN expression THEN expression)+ (ELSE expression)? END
        caseExpression.Definition = K.Case.SkipAnd(ZeroOrOne(expression))
            .And(OneOrMany(K.When.SkipAnd(expression).AndSkip(K.Then).And(expression).Then(Build.When)))
            .And(ZeroOrOne(K.Else.SkipAnd(expression)))
            .AndSkip(K.End)
            .Then(Build.Case);

        // castExpression ::= CAST '(' expression AS dataType ')'
        castExpression.Definition = K.Cast
            .SkipAnd(Lex.Parens(expression.AndSkip(K.As).And(dataType)))
            .Then(Build.Cast);

        // functionArguments ::= '*' | setQuantifier? expression (',' expression)*
        functionArguments.Definition = OneOf(
            Lex.Star.Then(static _ => Build.StarArgument),
            ZeroOrOne(setQuantifier).And(Separated(Lex.Comma, expression)).Then(Build.Arguments));

        // dataType ::= IDENTIFIER+ ('(' INTEGER (',' INTEGER)* ')')?
        dataType.Definition = OneOrMany(Lex.Identifier)
            .And(ZeroOrOne(Lex.Parens(Separated(Lex.Comma, Lex.Integer))))
            .Then(Build.DataType);

        // qualifiedName ::= identifier ('.' identifier)*
        qualifiedName.Definition = Separated(Lex.Dot, identifier);

        // identifier ::= IDENTIFIER | QUOTED_IDENTIFIER
        identifier.Definition = OneOf(Lex.Identifier, Lex.QuotedIdentifier);
    }
}
