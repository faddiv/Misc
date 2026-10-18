using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

internal sealed partial class SpracheGrammar
{
    private readonly Parser<Expression> expression = Rule.Create<Expression>(nameof(expression));
    private readonly Parser<Expression> andExpression = Rule.Create<Expression>(nameof(andExpression));
    private readonly Parser<Expression> notExpression = Rule.Create<Expression>(nameof(notExpression));
    private readonly Parser<Expression> predicate = Rule.Create<Expression>(nameof(predicate));
    private readonly Parser<PredicateSuffix> predicateSuffix = Rule.Create<PredicateSuffix>(nameof(predicateSuffix));
    private readonly Parser<BinaryOperator> comparisonOperator = Rule.Create<BinaryOperator>(nameof(comparisonOperator));
    private readonly Parser<Expression> additiveExpression = Rule.Create<Expression>(nameof(additiveExpression));
    private readonly Parser<Expression> multiplicativeExpression = Rule.Create<Expression>(nameof(multiplicativeExpression));
    private readonly Parser<Expression> unaryExpression = Rule.Create<Expression>(nameof(unaryExpression));
    private readonly Parser<Expression> primary = Rule.Create<Expression>(nameof(primary));
    private readonly Parser<Expression> caseExpression = Rule.Create<Expression>(nameof(caseExpression));
    private readonly Parser<Expression> castExpression = Rule.Create<Expression>(nameof(castExpression));
    private readonly Parser<FunctionArguments> functionArguments = Rule.Create<FunctionArguments>(nameof(functionArguments));
    private readonly Parser<DataType> dataType = Rule.Create<DataType>(nameof(dataType));
    private readonly Parser<IReadOnlyList<string>> qualifiedName = Rule.Create<IReadOnlyList<string>>(nameof(qualifiedName));
    private readonly Parser<string> identifier = Rule.Create<string>(nameof(identifier));

    private void DefineExpressions()
    {
        // expression ::= andExpression (OR andExpression)*
        expression.Define(LeftAssociative(andExpression, K.Or.Return(BinaryOperator.Or)));

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.Define(LeftAssociative(notExpression, K.And.Return(BinaryOperator.And)));

        // notExpression ::= NOT notExpression | predicate
        notExpression.Define(
            (from _ in K.Not
             from operand in notExpression
             select Build.Not(operand))
            .Or(predicate));

        // predicate ::= additiveExpression predicateSuffix?
        var optionalSuffix = predicateSuffix.Optional();
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
        var quantifier = K.Any.Return(Quantifier.Any)
            .Or(K.Some.Return(Quantifier.Some))
            .Or(K.All.Return(Quantifier.All));
        var not = K.Not.Return(true).Or(Parse.Return(false));     // NOT?

        //   ( (ANY | SOME | ALL) subquery | additiveExpression )
        var comparisonCompletion =
            (from q in quantifier
             from query in subquery
             select Build.QuantifiedComparison(q, query))
            .Or(additiveExpression.Select(Build.Comparison));

        //   ( BETWEEN ... | IN ... | LIKE ... )
        var inValues = Lex.Parens(
            subqueryExpression.Select(Build.InSubquery)
                .Or(expression.DelimitedBy(Lex.Comma).Select(Build.InList)));
        var optionalEscape = (from _ in K.Escape from escape in additiveExpression select escape).Optional();
        var negatableCompletion =
            (from _ in K.Between
             from low in additiveExpression
             from __ in K.And
             from high in additiveExpression
             select Build.Between(low, high))
            .Or(from _ in K.In
                from completion in inValues
                select completion)
            .Or(from _ in K.Like
                from pattern in additiveExpression
                from escape in optionalEscape
                select Build.Like(pattern, escape));

        predicateSuffix.Define(
            (from op in comparisonOperator
             from completion in comparisonCompletion
             select Build.ComparisonSuffix(op, completion))
            .Or(from _ in K.Is
                from negated in not
                from __ in K.Null
                select Build.IsNull(negated))
            .Or(from negated in not
                from completion in negatableCompletion
                select Build.Negatable(negated, completion)));

        // comparisonOperator ::= '=' | '<>' | '<' | '>' | '<=' | '>='
        //   (two-character operators are tried first)
        comparisonOperator.Define(
            Lex.Symbol("<>").Return(BinaryOperator.NotEqual)
                .Or(Lex.Symbol("<=").Return(BinaryOperator.LessThanOrEqual))
                .Or(Lex.Symbol(">=").Return(BinaryOperator.GreaterThanOrEqual))
                .Or(Lex.Symbol("=").Return(BinaryOperator.Equal))
                .Or(Lex.Symbol("<").Return(BinaryOperator.LessThan))
                .Or(Lex.Symbol(">").Return(BinaryOperator.GreaterThan)));

        // additiveExpression ::= multiplicativeExpression (('+' | '-' | '||') multiplicativeExpression)*
        additiveExpression.Define(LeftAssociative(
            multiplicativeExpression,
            Lex.Symbol("||").Return(BinaryOperator.Concat)
                .Or(Lex.Symbol("+").Return(BinaryOperator.Add))
                .Or(Lex.Symbol("-").Return(BinaryOperator.Subtract))));

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        multiplicativeExpression.Define(LeftAssociative(
            unaryExpression,
            Lex.Star.Return(BinaryOperator.Multiply)
                .Or(Lex.Symbol("/").Return(BinaryOperator.Divide))));

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        //   Written as two alternatives, the cheapest form for the rule every primary passes through.
        var plus = Lex.Symbol("+");
        var minus = Lex.Symbol("-");
        unaryExpression.Define(
            (from _ in plus from operand in unaryExpression select Build.Plus(operand))
            .Or(from _ in minus from operand in unaryExpression select Build.Minus(operand))
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
        primary.Define(
            Lex.Number.Select(Build.Number)
                .Or(Lex.String.Select(Build.String))
                .Or(K.Null.Return(Build.Null))
                .Or(caseExpression)
                .Or(castExpression)
                .Or(from _ in K.Exists
                    from query in subquery
                    select Build.Exists(query))
                .Or(from name in qualifiedName
                    from arguments in optionalCallArguments
                    select Build.ColumnOrFunctionCall(name, arguments))
                .Or(Lex.Parens(
                    subqueryExpression.Select(Build.ScalarSubquery)
                        .Or(expression))));

        // caseExpression ::= CASE expression? (WHEN expression THEN expression)+ (ELSE expression)? END
        var whenClauses = (
            from _ in K.When
            from condition in expression
            from __ in K.Then
            from result in expression
            select Build.When(condition, result)).AtLeastOnce();
        var optionalOperand = expression.Optional();
        var optionalElse = (from _ in K.Else from e in expression select e).Optional();
        caseExpression.Define(
            from _ in K.Case
            from operand in optionalOperand
            from whens in whenClauses
            from @else in optionalElse
            from __ in K.End
            select Build.Case(operand, whens, @else));

        // castExpression ::= CAST '(' expression AS dataType ')'
        var castBody = Lex.Parens(
            from operand in expression
            from _ in K.As
            from type in dataType
            select Build.Cast(operand, type));
        castExpression.Define(
            from _ in K.Cast
            from cast in castBody
            select cast);

        // functionArguments ::= '*' | setQuantifier? expression (',' expression)*
        var expressionList = expression.DelimitedBy(Lex.Comma);
        functionArguments.Define(
            Lex.Star.Return(Build.StarArgument)
                .Or(from q in setQuantifier.Optional()
                    from arguments in expressionList
                    select Build.Arguments(q, arguments)));

        // dataType ::= IDENTIFIER+ ('(' INTEGER (',' INTEGER)* ')')?
        var optionalTypeParameters = Lex.Parens(Lex.Integer.DelimitedBy(Lex.Comma)).Optional();
        dataType.Define(
            from words in Lex.Identifier.AtLeastOnce()
            from parameters in optionalTypeParameters
            select Build.DataType(words, parameters));

        // qualifiedName ::= identifier ('.' identifier)*
        qualifiedName.Define(identifier.DelimitedBy(Lex.Dot).Select(Build.List));

        // identifier ::= IDENTIFIER | QUOTED_IDENTIFIER
        identifier.Define(Lex.Identifier.Or(Lex.QuotedIdentifier));
    }

    /// <summary><c>operand (op operand)*</c>, folded to the left.</summary>
    private static Parser<Expression> LeftAssociative(Parser<Expression> operand, Parser<BinaryOperator> op) =>
        Combinators.ChainOperator(op, operand, Build.Binary);
}
