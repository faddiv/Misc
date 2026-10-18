using System.Globalization;
using Irony.Parsing;
using SqlParsers.Ast;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    private readonly Rule<Expression> expression = new(nameof(expression));
    private readonly Rule<Expression[]> expressionList = new(nameof(expressionList));
    private readonly Rule<Expression> andExpression = new(nameof(andExpression));
    private readonly Rule<Expression> notExpression = new(nameof(notExpression));
    private readonly Rule<Expression> predicate = new(nameof(predicate));
    private readonly NonTerminal predicateSuffixOpt = new(nameof(predicateSuffixOpt));
    private readonly NonTerminal comparisonSuffix = new(nameof(comparisonSuffix));
    private readonly NonTerminal quantifiedSuffix = new(nameof(quantifiedSuffix));
    private readonly NonTerminal isNullSuffix = new(nameof(isNullSuffix));
    private readonly NonTerminal betweenSuffix = new(nameof(betweenSuffix));
    private readonly NonTerminal inSubquerySuffix = new(nameof(inSubquerySuffix));
    private readonly NonTerminal inListSuffix = new(nameof(inListSuffix));
    private readonly NonTerminal likeSuffix = new(nameof(likeSuffix));
    private readonly Rule<Expression?> escapeOpt = new(nameof(escapeOpt));
    private readonly Rule<bool> notOpt = new(nameof(notOpt));
    private readonly Rule<BinaryOperator> comparisonOperator = new(nameof(comparisonOperator));
    private readonly Rule<Quantifier> quantifier = new(nameof(quantifier));
    private readonly Rule<Expression> additiveExpression = new(nameof(additiveExpression));
    private readonly Rule<BinaryOperator> additiveOperator = new(nameof(additiveOperator));
    private readonly Rule<Expression> multiplicativeExpression = new(nameof(multiplicativeExpression));
    private readonly Rule<BinaryOperator> multiplicativeOperator = new(nameof(multiplicativeOperator));
    private readonly Rule<Expression> unaryExpression = new(nameof(unaryExpression));
    private readonly Rule<UnaryOperator> unaryOperator = new(nameof(unaryOperator));
    private readonly Rule<Expression> primary = new(nameof(primary));
    private readonly Rule<Expression> columnReference = new(nameof(columnReference));
    private readonly Rule<Expression> functionCall = new(nameof(functionCall));
    private readonly Rule<FunctionArguments> functionArgumentsOpt = new(nameof(functionArgumentsOpt));
    private readonly Rule<Expression> caseExpression = new(nameof(caseExpression));
    private readonly Rule<Expression?> caseOperandOpt = new(nameof(caseOperandOpt));
    private readonly Rule<WhenClause[]> whenClauseList = new(nameof(whenClauseList));
    private readonly Rule<WhenClause> whenClause = new(nameof(whenClause));
    private readonly Rule<Expression?> elseOpt = new(nameof(elseOpt));
    private readonly Rule<Expression> castExpression = new(nameof(castExpression));
    private readonly Rule<DataType> dataType = new(nameof(dataType));
    private readonly Rule<string[]> typeWordList = new(nameof(typeWordList));
    private readonly Rule<int[]> typeParametersOpt = new(nameof(typeParametersOpt));
    private readonly NonTerminal integerList = new(nameof(integerList));
    private readonly Rule<Expression> existsExpression = new(nameof(existsExpression));
    private readonly Rule<string[]> qualifiedName = new(nameof(qualifiedName));
    private readonly Rule<string> identifier = new(nameof(identifier));

    private void DefineExpressions()
    {
        // expression ::= andExpression (OR andExpression)*
        expression.Rule = andExpression | expression + "OR" + andExpression;
        expression.ToAst = n => n.ChildNodes.Count == 1
            ? n.Get<Expression>(0)                                                                   // [andExpression]
            : Build.Binary(BinaryOperator.Or, n.Get<Expression>(0), n.Get<Expression>(1));           // [expression, andExpression]

        expressionList.Rule = MakePlusRule(expressionList, ToTerm(","), expression);
        expressionList.ToAst = n => n.Items<Expression>();

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.Rule = notExpression | andExpression + "AND" + notExpression;
        andExpression.ToAst = n => n.ChildNodes.Count == 1
            ? n.Get<Expression>(0)                                                                   // [notExpression]
            : Build.Binary(BinaryOperator.And, n.Get<Expression>(0), n.Get<Expression>(1));          // [andExpression, notExpression]

        // notExpression ::= NOT notExpression | predicate
        notExpression.Rule = ToTerm("NOT") + notExpression | predicate;
        notExpression.ToAst = n => n.ChildNodes.Count == 2
            ? Build.Not(n.Get<Expression>(1))                                                        // [NOT, notExpression]
            : n.Get<Expression>(0);                                                                  // [predicate]

        // predicate ::= additiveExpression predicateSuffix?
        predicate.Rule = additiveExpression + predicateSuffixOpt;
        predicate.ToAst = n => PredicateToAst(n.Get<Expression>(0), n.ChildNodes[1]);              // [additiveExpression, predicateSuffixOpt]

        // predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery
        //                                        | additiveExpression )
        //                   | IS NOT? NULL
        //                   | NOT? ( BETWEEN additiveExpression AND additiveExpression
        //                          | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
        //                          | LIKE additiveExpression (ESCAPE additiveExpression)? )
        //   One nonterminal per alternative; PredicateToAst below turns each into an AST node.
        predicateSuffixOpt.Rule = Empty | comparisonSuffix | quantifiedSuffix | isNullSuffix | betweenSuffix
                                  | inSubquerySuffix | inListSuffix | likeSuffix;
        comparisonSuffix.Rule = comparisonOperator + additiveExpression;
        quantifiedSuffix.Rule = comparisonOperator + quantifier + subquery;
        isNullSuffix.Rule = ToTerm("IS") + notOpt + "NULL";
        betweenSuffix.Rule = notOpt + "BETWEEN" + additiveExpression + "AND" + additiveExpression;
        inSubquerySuffix.Rule = notOpt + "IN" + subquery;
        inListSuffix.Rule = notOpt + "IN" + "(" + expressionList + ")";
        likeSuffix.Rule = notOpt + "LIKE" + additiveExpression + escapeOpt;

        escapeOpt.Rule = Empty | ToTerm("ESCAPE") + additiveExpression;
        escapeOpt.ToAst = n => n.IsEmpty() ? null : n.Get<Expression>(0);

        notOpt.Rule = Empty | "NOT";
        notOpt.ToAst = n => !n.IsEmpty();

        // comparisonOperator ::= '=' | '<>' | '<' | '>' | '<=' | '>='
        comparisonOperator.Rule = ToTerm("=") | "<>" | "<" | ">" | "<=" | ">=";
        comparisonOperator.ToAst = OperatorToAst;

        quantifier.Rule = ToTerm("ANY") | "SOME" | "ALL";
        quantifier.ToAst = n =>
            n.Is(0, "ANY") ? Quantifier.Any
            : n.Is(0, "SOME") ? Quantifier.Some
            : Quantifier.All;

        // additiveExpression ::= multiplicativeExpression (('+' | '-' | '||') multiplicativeExpression)*
        additiveExpression.Rule = multiplicativeExpression | additiveExpression + additiveOperator + multiplicativeExpression;
        additiveExpression.ToAst = BinaryChain;

        additiveOperator.Rule = ToTerm("+") | "-" | "||";
        additiveOperator.ToAst = OperatorToAst;

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        multiplicativeExpression.Rule = unaryExpression | multiplicativeExpression + multiplicativeOperator + unaryExpression;
        multiplicativeExpression.ToAst = BinaryChain;

        multiplicativeOperator.Rule = ToTerm("*") | "/";
        multiplicativeOperator.ToAst = OperatorToAst;

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        unaryExpression.Rule = primary | unaryOperator + unaryExpression;
        unaryExpression.ToAst = n => n.ChildNodes.Count == 1
            ? n.Get<Expression>(0)                                                                   // [primary]
            : Build.Unary(n.Get<UnaryOperator>(0), n.Get<Expression>(1));                            // [unaryOperator, unaryExpression]

        unaryOperator.Rule = ToTerm("-") | "+";
        unaryOperator.ToAst = n => n.Is(0, "-") ? UnaryOperator.Minus : UnaryOperator.Plus;

        // primary ::= NUMBER
        //           | STRING
        //           | NULL
        //           | caseExpression
        //           | castExpression
        //           | EXISTS subquery
        //           | qualifiedName ('(' functionArguments? ')')?
        //           | '(' (subqueryExpression | expression) ')'
        //   The last two alternatives are spelled out separately: columnReference / functionCall and
        //   subquery / '(' expression ')'.
        primary.Rule = number | stringLiteral | "NULL" | caseExpression | castExpression | existsExpression
                       | functionCall | columnReference | subquery | ToTerm("(") + expression + ")";
        primary.ToAst = n =>
            n.Is(0, number) ? Build.Number(n.ChildNodes[0].Token.Text)
            : n.Is(0, stringLiteral) ? Build.String((string)n.ChildNodes[0].Token.Value)
            : n.Is(0, subquery) ? Build.ScalarSubquery(n.Get<QueryExpression>(0))
            : n.Is(0, "NULL") ? Build.Null
            : n.Get<Expression>(0);                    // caseExpression, castExpression, existsExpression, functionCall, columnReference, expression

        columnReference.Rule = qualifiedName;
        columnReference.ToAst = n => Build.ColumnReference(n.Get<string[]>(0));

        functionCall.Rule = qualifiedName + "(" + functionArgumentsOpt + ")";
        functionCall.ToAst = n => Build.FunctionCall(n.Get<string[]>(0), n.Get<FunctionArguments>(1));    // [qualifiedName, functionArgumentsOpt]

        // functionArguments ::= '*' | setQuantifier? expression (',' expression)*
        functionArgumentsOpt.Rule = Empty | "*" | setQuantifierOpt + expressionList;
        functionArgumentsOpt.ToAst = n =>
            n.IsEmpty() ? Build.NoArguments                                                          // []
            : n.Is(0, "*") ? Build.StarArgument                                                      // ["*"]
            : Build.Arguments(n.Get<SetQuantifier>(0), n.Get<Expression[]>(1));                      // [setQuantifierOpt, expressionList]

        // caseExpression ::= CASE expression? (WHEN expression THEN expression)+ (ELSE expression)? END
        caseExpression.Rule = ToTerm("CASE") + caseOperandOpt + whenClauseList + elseOpt + "END";
        caseExpression.ToAst = n => Build.Case(n.Get<Expression?>(0), n.Get<WhenClause[]>(1), n.Get<Expression?>(2));   // [caseOperandOpt, whenClauseList, elseOpt]

        caseOperandOpt.Rule = Empty | expression;
        caseOperandOpt.ToAst = n => n.IsEmpty() ? null : n.Get<Expression>(0);

        whenClauseList.Rule = MakePlusRule(whenClauseList, whenClause);
        whenClauseList.ToAst = n => n.Items<WhenClause>();

        whenClause.Rule = ToTerm("WHEN") + expression + "THEN" + expression;
        whenClause.ToAst = n => Build.When(n.Get<Expression>(0), n.Get<Expression>(1));             // [expression, expression]

        elseOpt.Rule = Empty | ToTerm("ELSE") + expression;
        elseOpt.ToAst = n => n.IsEmpty() ? null : n.Get<Expression>(0);

        // castExpression ::= CAST '(' expression AS dataType ')'
        castExpression.Rule = ToTerm("CAST") + "(" + expression + "AS" + dataType + ")";
        castExpression.ToAst = n => Build.Cast(n.Get<Expression>(0), n.Get<DataType>(1));            // [expression, dataType]

        // dataType ::= IDENTIFIER+ ('(' INTEGER (',' INTEGER)* ')')?
        dataType.Rule = typeWordList + typeParametersOpt;
        dataType.ToAst = n => Build.DataType(n.Get<string[]>(0), n.Get<int[]>(1));                   // [typeWordList, typeParametersOpt]

        typeWordList.Rule = MakePlusRule(typeWordList, identifierToken);
        typeWordList.ToAst = n => n.ChildNodes.Select(word => word.Token.Text).ToArray();

        typeParametersOpt.Rule = Empty | ToTerm("(") + integerList + ")";
        typeParametersOpt.ToAst = n => n.IsEmpty() ? [] : n.ChildNodes[0].ChildNodes.Select(IntegerToAst).ToArray();

        integerList.Rule = MakePlusRule(integerList, ToTerm(","), number);

        existsExpression.Rule = ToTerm("EXISTS") + subquery;
        existsExpression.ToAst = n => Build.Exists(n.Get<QueryExpression>(0));

        // qualifiedName ::= identifier ('.' identifier)*
        qualifiedName.Rule = MakePlusRule(qualifiedName, ToTerm("."), identifier);
        qualifiedName.ToAst = n => n.Items<string>();

        // identifier ::= IDENTIFIER | QUOTED_IDENTIFIER
        identifier.Rule = identifierToken | quotedIdentifier;
        identifier.ToAst = n => n.Is(0, quotedIdentifier)
            ? (string)n.ChildNodes[0].Token.Value     // unescaped by Irony
            : n.ChildNodes[0].Token.Text;
    }

    /// <summary>Applies a <c>predicateSuffixOpt</c> node to the predicate's left operand.</summary>
    private Expression PredicateToAst(Expression left, ParseTreeNode suffixOpt)
    {
        if (suffixOpt.IsEmpty())
            return left;

        var s = suffixOpt.ChildNodes[0];
        var term = s.Term;
        if (term == comparisonSuffix)      // [comparisonOperator, additiveExpression]
            return Build.Binary(s.Get<BinaryOperator>(0), left, s.Get<Expression>(1));
        if (term == quantifiedSuffix)      // [comparisonOperator, quantifier, subquery]
            return Build.QuantifiedComparison(left, s.Get<BinaryOperator>(0), s.Get<Quantifier>(1), s.Get<QueryExpression>(2));
        if (term == isNullSuffix)          // [notOpt, NULL]
            return Build.IsNull(left, s.Get<bool>(0));
        if (term == betweenSuffix)         // [notOpt, additiveExpression, additiveExpression]
            return Build.Between(left, s.Get<bool>(0), s.Get<Expression>(1), s.Get<Expression>(2));
        if (term == inSubquerySuffix)      // [notOpt, subquery]
            return Build.InSubquery(left, s.Get<bool>(0), s.Get<QueryExpression>(1));
        if (term == inListSuffix)          // [notOpt, expressionList]
            return Build.InList(left, s.Get<bool>(0), s.Get<Expression[]>(1));
        if (term == likeSuffix)            // [notOpt, additiveExpression, escapeOpt]
            return Build.Like(left, s.Get<bool>(0), s.Get<Expression>(1), s.Get<Expression?>(2));
        throw new InvalidOperationException($"Unexpected predicate suffix '{term.Name}'.");
    }

    // [operand] | [left, operator, right]
    private static Expression BinaryChain(ParseTreeNode n) =>
        n.ChildNodes.Count == 1
            ? n.Get<Expression>(0)
            : Build.Binary(n.Get<BinaryOperator>(1), n.Get<Expression>(0), n.Get<Expression>(2));

    // [symbol]
    private static BinaryOperator OperatorToAst(ParseTreeNode n) => n.ChildNodes[0].Term.Name switch
    {
        "=" => BinaryOperator.Equal,
        "<>" => BinaryOperator.NotEqual,
        "<" => BinaryOperator.LessThan,
        ">" => BinaryOperator.GreaterThan,
        "<=" => BinaryOperator.LessThanOrEqual,
        ">=" => BinaryOperator.GreaterThanOrEqual,
        "+" => BinaryOperator.Add,
        "-" => BinaryOperator.Subtract,
        "||" => BinaryOperator.Concat,
        "*" => BinaryOperator.Multiply,
        "/" => BinaryOperator.Divide,
        var other => throw new InvalidOperationException($"Unexpected operator '{other}'."),
    };

    private static int IntegerToAst(ParseTreeNode token) =>
        int.TryParse(token.Token.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new SqlParseException($"Invalid type parameter '{token.Token.Text}'");
}
