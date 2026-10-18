using Superpower;
using Superpower.Parsers;
using SqlParsers.Ast;

namespace SqlParsers.Superpower;

internal sealed partial class SuperpowerGrammar
{
    private readonly TokenListParser<SqlToken, Expression> expression = Rule.Create<Expression>(nameof(expression));
    private readonly TokenListParser<SqlToken, Expression> andExpression = Rule.Create<Expression>(nameof(andExpression));
    private readonly TokenListParser<SqlToken, Expression> notExpression = Rule.Create<Expression>(nameof(notExpression));
    private readonly TokenListParser<SqlToken, Expression> predicate = Rule.Create<Expression>(nameof(predicate));
    private readonly TokenListParser<SqlToken, PredicateSuffix> predicateSuffix = Rule.Create<PredicateSuffix>(nameof(predicateSuffix));
    private readonly TokenListParser<SqlToken, BinaryOperator> comparisonOperator = Rule.Create<BinaryOperator>(nameof(comparisonOperator));
    private readonly TokenListParser<SqlToken, Expression> additiveExpression = Rule.Create<Expression>(nameof(additiveExpression));
    private readonly TokenListParser<SqlToken, Expression> multiplicativeExpression = Rule.Create<Expression>(nameof(multiplicativeExpression));
    private readonly TokenListParser<SqlToken, Expression> unaryExpression = Rule.Create<Expression>(nameof(unaryExpression));
    private readonly TokenListParser<SqlToken, Expression> primary = Rule.Create<Expression>(nameof(primary));
    private readonly TokenListParser<SqlToken, Expression> caseExpression = Rule.Create<Expression>(nameof(caseExpression));
    private readonly TokenListParser<SqlToken, Expression> castExpression = Rule.Create<Expression>(nameof(castExpression));
    private readonly TokenListParser<SqlToken, FunctionArguments> functionArguments = Rule.Create<FunctionArguments>(nameof(functionArguments));
    private readonly TokenListParser<SqlToken, DataType> dataType = Rule.Create<DataType>(nameof(dataType));
    private readonly TokenListParser<SqlToken, IReadOnlyList<string>> qualifiedName = Rule.Create<IReadOnlyList<string>>(nameof(qualifiedName));
    private readonly TokenListParser<SqlToken, string> identifier = Rule.Create<string>(nameof(identifier));

    private void DefineExpressions()
    {
        // expression ::= andExpression (OR andExpression)*
        expression.Define(LeftAssociative(andExpression, K.Or.Value(BinaryOperator.Or)));

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.Define(LeftAssociative(notExpression, K.And.Value(BinaryOperator.And)));

        // notExpression ::= NOT notExpression | predicate
        notExpression.Define(
            K.Not.IgnoreThen(notExpression).Select(Build.Not)
                .Or(predicate));

        // predicate ::= additiveExpression predicateSuffix?
        var optionalSuffix = predicateSuffix.OptionalOrDefault();
        predicate.Define(
            from left in additiveExpression
            from suffix in optionalSuffix
            select Build.Predicate(left, suffix));

        // predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery
        //                                        | additiveExpression )
        //                   | IS NOT? NULL
        //                   | NOT? ( BETWEEN additiveExpression AND additiveExpression
        //                          | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
        //                          | LIKE additiveExpression (ESCAPE additiveExpression)? )
        //   A suffix is a function that completes the predicate once its left operand is known.
        var quantifier = K.Any.Value(Quantifier.Any)
            .Or(K.Some.Value(Quantifier.Some))
            .Or(K.All.Value(Quantifier.All));
        var not = K.Not.Value(true).OptionalOrDefault(false);     // NOT?

        //   ( (ANY | SOME | ALL) subquery | additiveExpression )
        var comparisonCompletion =
            (from q in quantifier
             from query in subquery
             select Build.QuantifiedComparison(q, query))
            .Or(additiveExpression.Select(Build.Comparison));

        //   ( BETWEEN ... | IN ... | LIKE ... )
        var optionalEscape = K.Escape.IgnoreThen(additiveExpression).OptionalOrDefault();
        var negatableCompletion = Parse.OneOf(
            from _ in K.Between
            from low in additiveExpression
            from __ in K.And
            from high in additiveExpression
            select Build.Between(low, high),

            K.In.IgnoreThen(Lex.Parens(
                subqueryExpression.Select(Build.InSubquery)
                    .Or(expression.AtLeastOnceDelimitedBy(Lex.Comma).Select(Build.InList)))),

            from _ in K.Like
            from pattern in additiveExpression
            from escape in optionalEscape
            select Build.Like(pattern, escape));

        predicateSuffix.Define(Parse.OneOf(
            from op in comparisonOperator
            from completion in comparisonCompletion
            select Build.ComparisonSuffix(op, completion),

            from _ in K.Is
            from negated in not
            from __ in K.Null
            select Build.IsNull(negated),

            from negated in not
            from completion in negatableCompletion
            select Build.Negatable(negated, completion)));

        // comparisonOperator ::= '=' | '<>' | '<' | '>' | '<=' | '>='   (one token each)
        comparisonOperator.Define(Parse.OneOf(
            Token.EqualTo(SqlToken.Equal).Value(BinaryOperator.Equal),
            Token.EqualTo(SqlToken.NotEqual).Value(BinaryOperator.NotEqual),
            Token.EqualTo(SqlToken.LessThan).Value(BinaryOperator.LessThan),
            Token.EqualTo(SqlToken.GreaterThan).Value(BinaryOperator.GreaterThan),
            Token.EqualTo(SqlToken.LessThanOrEqual).Value(BinaryOperator.LessThanOrEqual),
            Token.EqualTo(SqlToken.GreaterThanOrEqual).Value(BinaryOperator.GreaterThanOrEqual)));

        // additiveExpression ::= multiplicativeExpression (('+' | '-' | '||') multiplicativeExpression)*
        additiveExpression.Define(LeftAssociative(
            multiplicativeExpression,
            Lex.Concat.Value(BinaryOperator.Concat)
                .Or(Lex.Plus.Value(BinaryOperator.Add))
                .Or(Lex.Minus.Value(BinaryOperator.Subtract))));

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        multiplicativeExpression.Define(LeftAssociative(
            unaryExpression,
            Lex.Star.Value(BinaryOperator.Multiply)
                .Or(Lex.Slash.Value(BinaryOperator.Divide))));

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        //   Written as two alternatives, the cheapest form for the rule every primary passes through.
        unaryExpression.Define(
            Lex.Plus.IgnoreThen(unaryExpression).Select(Build.Plus)
                .Or(Lex.Minus.IgnoreThen(unaryExpression).Select(Build.Minus))
                .Or(primary));

        // primary ::= NUMBER
        //           | STRING
        //           | NULL
        //           | caseExpression
        //           | castExpression
        //           | EXISTS subquery
        //           | qualifiedName ('(' functionArguments? ')')?
        //           | '(' (subqueryExpression | expression) ')'
        var optionalCallArguments = Lex.Parens(functionArguments.Optional()).Select(Build.OrNoArguments).Optional();
        primary.Define(Parse.OneOf(
            Lex.Number.Select(Build.Number),
            Lex.String.Select(Build.String),
            K.Null.Value(Build.Null),
            caseExpression,
            castExpression,
            K.Exists.IgnoreThen(subquery).Select(Build.Exists),
            from name in qualifiedName
            from arguments in optionalCallArguments
            select Build.ColumnOrFunctionCall(name, arguments),
            Lex.Parens(
                subqueryExpression.Select(Build.ScalarSubquery)
                    .Or(expression))));

        // caseExpression ::= CASE expression? (WHEN expression THEN expression)+ (ELSE expression)? END
        var whenClause =
            from _ in K.When
            from condition in expression
            from __ in K.Then
            from result in expression
            select Build.When(condition, result);
        var optionalOperand = expression.OptionalOrDefault();
        var whenClauses = whenClause.AtLeastOnce();
        var optionalElse = K.Else.IgnoreThen(expression).OptionalOrDefault();
        caseExpression.Define(
            from _ in K.Case
            from operand in optionalOperand
            from whens in whenClauses
            from @else in optionalElse
            from __ in K.End
            select Build.Case(operand, whens, @else));

        // castExpression ::= CAST '(' expression AS dataType ')'
        castExpression.Define(K.Cast.IgnoreThen(Lex.Parens(
            from operand in expression
            from _ in K.As
            from type in dataType
            select Build.Cast(operand, type))));

        // functionArguments ::= '*' | setQuantifier? expression (',' expression)*
        var expressionList = expression.AtLeastOnceDelimitedBy(Lex.Comma);
        functionArguments.Define(
            Lex.Star.Value(Build.StarArgument)
                .Or(from q in setQuantifier.OptionalOrDefault(SetQuantifier.None)
                    from arguments in expressionList
                    select Build.Arguments(q, arguments)));

        // dataType ::= IDENTIFIER+ ('(' INTEGER (',' INTEGER)* ')')?
        var optionalTypeParameters = Lex.Parens(Lex.Integer.AtLeastOnceDelimitedBy(Lex.Comma)).OptionalOrDefault(Array.Empty<int>());
        dataType.Define(
            from words in Lex.Identifier.AtLeastOnce()
            from parameters in optionalTypeParameters
            select Build.DataType(words, parameters));

        // qualifiedName ::= identifier ('.' identifier)*
        qualifiedName.Define(identifier.AtLeastOnceDelimitedBy(Lex.Dot).Select(Build.List));

        // identifier ::= IDENTIFIER | QUOTED_IDENTIFIER
        identifier.Define(Lex.Identifier.Or(Lex.QuotedIdentifier));
    }

    /// <summary><c>operand (op operand)*</c>, folded to the left.</summary>
    private static TokenListParser<SqlToken, Expression> LeftAssociative(
        TokenListParser<SqlToken, Expression> operand,
        TokenListParser<SqlToken, BinaryOperator> op) =>
        Parse.Chain(op, operand, Build.Binary);
}
