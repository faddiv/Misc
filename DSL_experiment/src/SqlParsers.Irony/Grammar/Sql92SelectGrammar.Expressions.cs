using System.Globalization;
using Irony.Parsing;
using SqlParsers.Ast;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    // Transient pass-through levels (see MarkTransient in DefineExpressions) are plain nonterminals.
    private readonly NonTerminal expression = new(nameof(expression));
    private readonly Rule<Expression> orOperation = new(nameof(orOperation));
    private readonly Rule<Expression[]> expressionList = new(nameof(expressionList));
    private readonly NonTerminal andExpression = new(nameof(andExpression));
    private readonly Rule<Expression> andOperation = new(nameof(andOperation));
    private readonly NonTerminal notExpression = new(nameof(notExpression));
    private readonly Rule<Expression> notOperation = new(nameof(notOperation));
    private readonly NonTerminal predicate = new(nameof(predicate));
    private readonly Rule<Expression> predicateWithSuffix = new(nameof(predicateWithSuffix));
    private readonly NonTerminal predicateSuffix = new(nameof(predicateSuffix));
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
    private readonly NonTerminal additiveExpression = new(nameof(additiveExpression));
    private readonly Rule<Expression> additiveOperation = new(nameof(additiveOperation));
    private readonly Rule<BinaryOperator> additiveOperator = new(nameof(additiveOperator));
    private readonly NonTerminal multiplicativeExpression = new(nameof(multiplicativeExpression));
    private readonly Rule<Expression> multiplicativeOperation = new(nameof(multiplicativeOperation));
    private readonly Rule<BinaryOperator> multiplicativeOperator = new(nameof(multiplicativeOperator));
    private readonly NonTerminal unaryExpression = new(nameof(unaryExpression));
    private readonly Rule<Expression> unaryOperation = new(nameof(unaryOperation));
    private readonly Rule<UnaryOperator> unaryOperator = new(nameof(unaryOperator));
    private readonly NonTerminal primary = new(nameof(primary));
    private readonly Rule<Expression> literal = new(nameof(literal));
    private readonly Rule<Expression> scalarSubquery = new(nameof(scalarSubquery));
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
        // The pass-through levels of the expression hierarchy are transient: when a level has a single child,
        // Irony passes the child's node up instead of creating one more node (an expression that is a plain
        // column would otherwise be a chain of ten nodes). Irony keeps only one child of a transient node, so each
        // level is a choice between the next level and a separate rule (xOperation) for its operator.
        MarkTransient(expression, andExpression, notExpression, predicate, additiveExpression, multiplicativeExpression,
            unaryExpression, primary);

        // expression ::= andExpression (OR andExpression)*
        expression.Rule = andExpression | orOperation;
        orOperation.Rule = expression + "OR" + andExpression;
        orOperation.ToAst = n => Build.Binary(BinaryOperator.Or, n.Get<Expression>(0), n.Get<Expression>(1));     // [expression, andExpression]

        expressionList.Rule = MakePlusRule(expressionList, ToTerm(","), expression);
        expressionList.ToAst = n => n.Items<Expression>();

        // andExpression ::= notExpression (AND notExpression)*
        andExpression.Rule = notExpression | andOperation;
        andOperation.Rule = andExpression + "AND" + notExpression;
        andOperation.ToAst = n => Build.Binary(BinaryOperator.And, n.Get<Expression>(0), n.Get<Expression>(1));   // [andExpression, notExpression]

        // notExpression ::= NOT notExpression | predicate
        notExpression.Rule = notOperation | predicate;
        notOperation.Rule = ToTerm("NOT") + notExpression;
        notOperation.ToAst = n => Build.Not(n.Get<Expression>(1));                                    // [NOT, notExpression]

        // predicate ::= additiveExpression predicateSuffix?
        predicate.Rule = additiveExpression | predicateWithSuffix;
        predicateWithSuffix.Rule = additiveExpression + predicateSuffix;
        predicateWithSuffix.ToAst = n => PredicateToAst(n.Get<Expression>(0), n.ChildNodes[1]);    // [additiveExpression, predicateSuffix]

        // predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery
        //                                        | additiveExpression )
        //                   | IS NOT? NULL
        //                   | NOT? ( BETWEEN additiveExpression AND additiveExpression
        //                          | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
        //                          | LIKE additiveExpression (ESCAPE additiveExpression)? )
        //   One nonterminal per alternative; PredicateToAst below turns each into an AST node.
        predicateSuffix.Rule = comparisonSuffix | quantifiedSuffix | isNullSuffix | betweenSuffix
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
        additiveExpression.Rule = multiplicativeExpression | additiveOperation;
        additiveOperation.Rule = additiveExpression + additiveOperator + multiplicativeExpression;
        additiveOperation.ToAst = BinaryOperation;

        additiveOperator.Rule = ToTerm("+") | "-" | "||";
        additiveOperator.ToAst = OperatorToAst;

        // multiplicativeExpression ::= unaryExpression (('*' | '/') unaryExpression)*
        multiplicativeExpression.Rule = unaryExpression | multiplicativeOperation;
        multiplicativeOperation.Rule = multiplicativeExpression + multiplicativeOperator + unaryExpression;
        multiplicativeOperation.ToAst = BinaryOperation;

        multiplicativeOperator.Rule = ToTerm("*") | "/";
        multiplicativeOperator.ToAst = OperatorToAst;

        // unaryExpression ::= ('+' | '-') unaryExpression | primary
        unaryExpression.Rule = primary | unaryOperation;
        unaryOperation.Rule = unaryOperator + unaryExpression;
        unaryOperation.ToAst = n => Build.Unary(n.Get<UnaryOperator>(0), n.Get<Expression>(1));     // [unaryOperator, unaryExpression]

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
        //   scalarSubquery / '(' expression ')'. The tokens are grouped into literal, so that primary itself can
        //   be transient (a token node has no AST conversion of its own).
        primary.Rule = literal | caseExpression | castExpression | existsExpression
                       | functionCall | columnReference | scalarSubquery | ToTerm("(") + expression + ")";

        literal.Rule = number | stringLiteral | "NULL";
        literal.ToAst = n =>
            n.Is(0, number) ? Build.Number(n.ChildNodes[0].Token.Text)
            : n.Is(0, stringLiteral) ? Build.String((string)n.ChildNodes[0].Token.Value)
            : Build.Null;

        scalarSubquery.Rule = subquery;
        scalarSubquery.ToAst = n => Build.ScalarSubquery(n.Get<QueryExpression>(0));

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

    /// <summary>Applies a <c>predicateSuffix</c> node to the predicate's left operand.</summary>
    private Expression PredicateToAst(Expression left, ParseTreeNode suffix)
    {
        var s = suffix.ChildNodes[0];
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

    // [left, operator, right]
    private static Expression BinaryOperation(ParseTreeNode n) =>
        Build.Binary(n.Get<BinaryOperator>(1), n.Get<Expression>(0), n.Get<Expression>(2));

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
