using Farkle.Builder;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

internal sealed partial class FarkleGrammar
{
    private readonly Nonterminal<Expression> expression = Nonterminal.Create<Expression>(nameof(expression));
    private readonly Nonterminal<List<Expression>> expressionList = Nonterminal.Create<List<Expression>>(nameof(expressionList));
    private readonly Nonterminal<Expression> andExpression = Nonterminal.Create<Expression>(nameof(andExpression));
    private readonly Nonterminal<Expression> notExpression = Nonterminal.Create<Expression>(nameof(notExpression));
    private readonly Nonterminal<Expression> predicate = Nonterminal.Create<Expression>(nameof(predicate));
    private readonly Nonterminal<bool> notOpt = Nonterminal.Create<bool>(nameof(notOpt));
    private readonly Nonterminal<Expression?> escapeOpt = Nonterminal.Create<Expression?>(nameof(escapeOpt));
    private readonly Nonterminal<BinaryOperator> comparisonOperator = Nonterminal.Create<BinaryOperator>(nameof(comparisonOperator));
    private readonly Nonterminal<Quantifier> quantifier = Nonterminal.Create<Quantifier>(nameof(quantifier));
    private readonly Nonterminal<Expression> additiveExpression = Nonterminal.Create<Expression>(nameof(additiveExpression));
    private readonly Nonterminal<BinaryOperator> additiveOperator = Nonterminal.Create<BinaryOperator>(nameof(additiveOperator));
    private readonly Nonterminal<Expression> multiplicativeExpression = Nonterminal.Create<Expression>(nameof(multiplicativeExpression));
    private readonly Nonterminal<BinaryOperator> multiplicativeOperator = Nonterminal.Create<BinaryOperator>(nameof(multiplicativeOperator));
    private readonly Nonterminal<Expression> unaryExpression = Nonterminal.Create<Expression>(nameof(unaryExpression));
    private readonly Nonterminal<Expression> primary = Nonterminal.Create<Expression>(nameof(primary));
    private readonly Nonterminal<FunctionArguments> functionArgumentsOpt = Nonterminal.Create<FunctionArguments>(nameof(functionArgumentsOpt));
    private readonly Nonterminal<Expression> caseExpression = Nonterminal.Create<Expression>(nameof(caseExpression));
    private readonly Nonterminal<Expression?> caseOperandOpt = Nonterminal.Create<Expression?>(nameof(caseOperandOpt));
    private readonly Nonterminal<List<WhenClause>> whenClauseList = Nonterminal.Create<List<WhenClause>>(nameof(whenClauseList));
    private readonly Nonterminal<WhenClause> whenClause = Nonterminal.Create<WhenClause>(nameof(whenClause));
    private readonly Nonterminal<Expression?> elseOpt = Nonterminal.Create<Expression?>(nameof(elseOpt));
    private readonly Nonterminal<Expression> castExpression = Nonterminal.Create<Expression>(nameof(castExpression));
    private readonly Nonterminal<DataType> dataType = Nonterminal.Create<DataType>(nameof(dataType));
    private readonly Nonterminal<List<string>> typeWordList = Nonterminal.Create<List<string>>(nameof(typeWordList));
    private readonly Nonterminal<IReadOnlyList<int>> typeParametersOpt = Nonterminal.Create<IReadOnlyList<int>>(nameof(typeParametersOpt));
    private readonly Nonterminal<List<int>> integerList = Nonterminal.Create<List<int>>(nameof(integerList));
    private readonly Nonterminal<int> integer = Nonterminal.Create<int>(nameof(integer));
    private readonly Nonterminal<List<string>> qualifiedName = Nonterminal.Create<List<string>>(nameof(qualifiedName));
    private readonly Nonterminal<string> identifier = Nonterminal.Create<string>(nameof(identifier));

    private void DefineExpressions()
    {
        // expression ::= andExpression (OR andExpression)*
        expression.SetProductions(
            andExpression.AsProduction(),
            expression.Extended().Append("OR").Extend(andExpression).Finish(Build.Or));
        DefineList(expressionList, expression, ",");

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.SetProductions(
            notExpression.AsProduction(),
            andExpression.Extended().Append("AND").Extend(notExpression).Finish(Build.And));

        // notExpression ::= NOT notExpression | predicate
        notExpression.SetProductions(
            "NOT".Appended().Extend(notExpression).Finish(Build.Not),
            predicate.AsProduction());

        // predicate       ::= additiveExpression predicateSuffix?
        // predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery
        //                                        | additiveExpression )
        //                   | IS NOT? NULL
        //                   | NOT? ( BETWEEN additiveExpression AND additiveExpression
        //                          | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
        //                          | LIKE additiveExpression (ESCAPE additiveExpression)? )
        //   Spelled out as one production per suffix, each starting with the left operand.
        predicate.SetProductions(
            additiveExpression.AsProduction(),
            additiveExpression.Extended().Extend(comparisonOperator).Extend(additiveExpression)
                .Finish(Build.Comparison),
            additiveExpression.Extended().Extend(comparisonOperator).Extend(quantifier).Extend(subquery)
                .Finish(Build.QuantifiedComparison),
            additiveExpression.Extended().Append("IS").Extend(notOpt).Append("NULL")
                .Finish(Build.IsNull),
            additiveExpression.Extended().Extend(notOpt).Append("BETWEEN").Extend(additiveExpression).Append("AND").Extend(additiveExpression)
                .Finish(Build.Between),
            additiveExpression.Extended().Extend(notOpt).Append("IN").Extend(subquery)
                .Finish(Build.InSubquery),
            additiveExpression.Extended().Extend(notOpt).Append("IN").Append("(").Extend(expressionList).Append(")")
                .Finish(Build.InList),
            additiveExpression.Extended().Extend(notOpt).Append("LIKE").Extend(additiveExpression).Extend(escapeOpt)
                .Finish(Build.Like));

        notOpt.SetProductions(
            Empty.FinishConstant(false),
            "NOT".Appended().FinishConstant(true));

        escapeOpt.SetProductions(
            Empty.FinishConstant<Expression?>(null),
            "ESCAPE".Appended().Extend(additiveExpression).Finish(Build.Some));

        // comparisonOperator ::= '=' | '<>' | '<' | '>' | '<=' | '>='
        comparisonOperator.SetProductions(
            "=".Appended().FinishConstant(BinaryOperator.Equal),
            "<>".Appended().FinishConstant(BinaryOperator.NotEqual),
            "<".Appended().FinishConstant(BinaryOperator.LessThan),
            ">".Appended().FinishConstant(BinaryOperator.GreaterThan),
            "<=".Appended().FinishConstant(BinaryOperator.LessThanOrEqual),
            ">=".Appended().FinishConstant(BinaryOperator.GreaterThanOrEqual));

        quantifier.SetProductions(
            "ANY".Appended().FinishConstant(Quantifier.Any),
            "SOME".Appended().FinishConstant(Quantifier.Some),
            "ALL".Appended().FinishConstant(Quantifier.All));

        // additiveExpression ::= multiplicativeExpression (('+' | '-' | '||') multiplicativeExpression)*
        additiveExpression.SetProductions(
            multiplicativeExpression.AsProduction(),
            additiveExpression.Extended().Extend(additiveOperator).Extend(multiplicativeExpression).Finish(Build.Binary));

        additiveOperator.SetProductions(
            "+".Appended().FinishConstant(BinaryOperator.Add),
            "-".Appended().FinishConstant(BinaryOperator.Subtract),
            "||".Appended().FinishConstant(BinaryOperator.Concat));

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        multiplicativeExpression.SetProductions(
            unaryExpression.AsProduction(),
            multiplicativeExpression.Extended().Extend(multiplicativeOperator).Extend(unaryExpression).Finish(Build.Binary));

        multiplicativeOperator.SetProductions(
            "*".Appended().FinishConstant(BinaryOperator.Multiply),
            "/".Appended().FinishConstant(BinaryOperator.Divide));

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        unaryExpression.SetProductions(
            primary.AsProduction(),
            "-".Appended().Extend(unaryExpression).Finish(Build.Minus),
            "+".Appended().Extend(unaryExpression).Finish(Build.Plus));

        // primary ::= NUMBER
        //           | STRING
        //           | NULL
        //           | caseExpression
        //           | castExpression
        //           | EXISTS subquery
        //           | qualifiedName ('(' functionArguments? ')')?
        //           | '(' (subqueryExpression | expression) ')'
        primary.SetProductions(
            number.Extended().Finish(Build.Number),
            stringLiteral.Extended().Finish(Build.String),
            "NULL".Appended().FinishConstant(Build.Null),
            caseExpression.AsProduction(),
            castExpression.AsProduction(),
            "EXISTS".Appended().Extend(subquery).Finish(Build.Exists),
            qualifiedName.Extended().Append("(").Extend(functionArgumentsOpt).Append(")").Finish(Build.FunctionCall),
            qualifiedName.Extended().Finish(Build.ColumnReference),
            subquery.Extended().Finish(Build.ScalarSubquery),
            "(".Appended().Extend(expression).Append(")").AsProduction());

        // functionArguments ::= '*' | setQuantifier? expression (',' expression)*
        functionArgumentsOpt.SetProductions(
            Empty.FinishConstant(Build.NoArguments),
            "*".Appended().FinishConstant(Build.StarArgument),
            setQuantifierOpt.Extended().Extend(expressionList).Finish(Build.Arguments));

        // caseExpression ::= CASE expression? (WHEN expression THEN expression)+ (ELSE expression)? END
        caseExpression.SetProductions(
            "CASE".Appended().Extend(caseOperandOpt).Extend(whenClauseList).Extend(elseOpt).Append("END").Finish(Build.Case));

        caseOperandOpt.SetProductions(
            Empty.FinishConstant<Expression?>(null),
            expression.Extended().Finish(Build.Some));

        DefineList(whenClauseList, whenClause, separator: null);

        whenClause.SetProductions(
            "WHEN".Appended().Extend(expression).Append("THEN").Extend(expression).Finish(Build.When));

        elseOpt.SetProductions(
            Empty.FinishConstant<Expression?>(null),
            "ELSE".Appended().Extend(expression).Finish(Build.Some));

        // castExpression ::= CAST '(' expression AS dataType ')'
        castExpression.SetProductions(
            "CAST".Appended().Append("(").Extend(expression).Append("AS").Extend(dataType).Append(")").Finish(Build.Cast));

        // dataType ::= IDENTIFIER+ ('(' INTEGER (',' INTEGER)* ')')?
        dataType.SetProductions(
            typeWordList.Extended().Extend(typeParametersOpt).Finish(Build.DataType));
        DefineList(typeWordList, identifierToken, separator: null);

        typeParametersOpt.SetProductions(
            Empty.FinishConstant(Build.NoTypeParameters),
            "(".Appended().Extend(integerList).Append(")").Finish(Build.TypeParameters));
        DefineList(integerList, integer, ",");

        // INTEGER ::= [0-9]+   (a NUMBER without fraction or exponent)
        integer.SetProductions(number.Extended().Finish(Build.Integer));

        // qualifiedName ::= identifier ('.' identifier)*
        DefineList(qualifiedName, identifier, ".");

        // identifier ::= IDENTIFIER | QUOTED_IDENTIFIER
        identifier.SetProductions(
            identifierToken.AsProduction(),
            quotedIdentifier.AsProduction());
    }
}
