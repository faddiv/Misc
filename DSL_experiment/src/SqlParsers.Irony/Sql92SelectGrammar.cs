using Irony.Parsing;

namespace SqlParsers.Irony;

/// <summary>
/// LALR(1) grammar for the SQL-92 SELECT subset. Keywords that carry no information are marked as
/// punctuation, so they do not appear in the parse tree; see <see cref="IronyAstBuilder"/> for node shapes.
/// </summary>
internal sealed class Sql92SelectGrammar : Grammar
{
    public Sql92SelectGrammar() : base(caseSensitive: false)
    {
        // ------------------------------------------------------------ terminals

        var number = new NumberLiteral("number", NumberOptions.AllowStartEndDot);
        var stringLiteral = new StringLiteral("string", "'", StringOptions.AllowsDoubledQuote | StringOptions.NoEscapes);
        var quotedId = new StringLiteral("quotedId", "\"", StringOptions.AllowsDoubledQuote | StringOptions.NoEscapes);
        var id = new IdentifierTerminal("id");
        NonGrammarTerminals.Add(new CommentTerminal("comment", "--", "\n", "\r\n"));

        var comma = ToTerm(",");
        var dot = ToTerm(".");
        var lParen = ToTerm("(");
        var rParen = ToTerm(")");
        var star = ToTerm("*");

        // ------------------------------------------------------------ non-terminals

        var statement = new NonTerminal("statement");
        var semiOpt = new NonTerminal("semiOpt");
        var orderByOpt = new NonTerminal("orderByOpt");
        var orderList = new NonTerminal("orderList");
        var orderItem = new NonTerminal("orderItem");
        var directionOpt = new NonTerminal("directionOpt");

        var queryExpr = new NonTerminal("queryExpr");
        var unionOp = new NonTerminal("unionOp");
        var allOpt = new NonTerminal("allOpt");
        var queryTerm = new NonTerminal("queryTerm");
        var queryPrimary = new NonTerminal("queryPrimary");
        var subqueryExpr = new NonTerminal("subqueryExpr");
        var subqueryTerm = new NonTerminal("subqueryTerm");
        var subquery = new NonTerminal("subquery");
        var querySpec = new NonTerminal("querySpec");
        var quantOpt = new NonTerminal("quantOpt");
        var selectList = new NonTerminal("selectList");
        var selectItemList = new NonTerminal("selectItemList");
        var selectItem = new NonTerminal("selectItem");
        var aliasOpt = new NonTerminal("aliasOpt");
        var whereOpt = new NonTerminal("whereOpt");
        var groupByOpt = new NonTerminal("groupByOpt");
        var havingOpt = new NonTerminal("havingOpt");

        var tableRefList = new NonTerminal("tableRefList");
        var tableRef = new NonTerminal("tableRef");
        var joinedTable = new NonTerminal("joinedTable");
        var joinTypeOpt = new NonTerminal("joinTypeOpt");
        var outerOpt = new NonTerminal("outerOpt");
        var joinSpec = new NonTerminal("joinSpec");
        var nameList = new NonTerminal("nameList");
        var tablePrimary = new NonTerminal("tablePrimary");
        var derivedAlias = new NonTerminal("derivedAlias");

        var expr = new NonTerminal("expr");
        var exprList = new NonTerminal("exprList");
        var orExpr = new NonTerminal("orExpr");
        var andExpr = new NonTerminal("andExpr");
        var notExpr = new NonTerminal("notExpr");
        var predicate = new NonTerminal("predicate");
        var predicateSuffixOpt = new NonTerminal("predicateSuffixOpt");
        var comparisonSuffix = new NonTerminal("comparisonSuffix");
        var quantifiedSuffix = new NonTerminal("quantifiedSuffix");
        var isNullSuffix = new NonTerminal("isNullSuffix");
        var betweenSuffix = new NonTerminal("betweenSuffix");
        var inSubquerySuffix = new NonTerminal("inSubquerySuffix");
        var inListSuffix = new NonTerminal("inListSuffix");
        var likeSuffix = new NonTerminal("likeSuffix");
        var escapeOpt = new NonTerminal("escapeOpt");
        var notOpt = new NonTerminal("notOpt");
        var compOp = new NonTerminal("compOp");
        var quantifier = new NonTerminal("quantifier");
        var addExpr = new NonTerminal("addExpr");
        var addOp = new NonTerminal("addOp");
        var mulExpr = new NonTerminal("mulExpr");
        var mulOp = new NonTerminal("mulOp");
        var unaryExpr = new NonTerminal("unaryExpr");
        var unaryOp = new NonTerminal("unaryOp");
        var primary = new NonTerminal("primary");
        var columnRef = new NonTerminal("columnRef");
        var functionCall = new NonTerminal("functionCall");
        var funcArgs = new NonTerminal("funcArgs");
        var caseExpr = new NonTerminal("caseExpr");
        var caseOperandOpt = new NonTerminal("caseOperandOpt");
        var whenList = new NonTerminal("whenList");
        var whenClause = new NonTerminal("whenClause");
        var elseOpt = new NonTerminal("elseOpt");
        var castExpr = new NonTerminal("castExpr");
        var dataType = new NonTerminal("dataType");
        var typeWords = new NonTerminal("typeWords");
        var typeParamsOpt = new NonTerminal("typeParamsOpt");
        var intList = new NonTerminal("intList");
        var existsExpr = new NonTerminal("existsExpr");
        var qualifiedName = new NonTerminal("qualifiedName");
        var name = new NonTerminal("name");

        // ------------------------------------------------------------ statement

        statement.Rule = queryExpr + orderByOpt + semiOpt;
        semiOpt.Rule = Empty | ";";
        orderByOpt.Rule = Empty | ToTerm("ORDER") + "BY" + orderList;
        orderList.Rule = MakePlusRule(orderList, comma, orderItem);
        orderItem.Rule = expr + directionOpt;
        directionOpt.Rule = Empty | "ASC" | "DESC";

        // ------------------------------------------------------------ queries

        queryExpr.Rule = queryTerm | queryExpr + unionOp + queryTerm;
        unionOp.Rule = ToTerm("UNION") + allOpt | ToTerm("EXCEPT") + allOpt;
        allOpt.Rule = Empty | "ALL";
        queryTerm.Rule = queryPrimary | queryTerm + "INTERSECT" + allOpt + queryPrimary;
        queryPrimary.Rule = querySpec | lParen + queryExpr + rParen;

        // Subqueries inside expressions must start with SELECT; this keeps "(" unambiguous in expressions.
        subquery.Rule = lParen + subqueryExpr + rParen;
        subqueryExpr.Rule = subqueryTerm | subqueryExpr + unionOp + queryTerm;
        subqueryTerm.Rule = querySpec | subqueryTerm + "INTERSECT" + allOpt + queryPrimary;

        querySpec.Rule = ToTerm("SELECT") + quantOpt + selectList + "FROM" + tableRefList + whereOpt + groupByOpt + havingOpt;
        quantOpt.Rule = Empty | "DISTINCT" | "ALL";
        selectList.Rule = star | selectItemList;
        selectItemList.Rule = MakePlusRule(selectItemList, comma, selectItem);
        selectItem.Rule = expr + aliasOpt | qualifiedName + dot + star;
        aliasOpt.Rule = Empty | name | ToTerm("AS") + name;
        whereOpt.Rule = Empty | ToTerm("WHERE") + expr;
        groupByOpt.Rule = Empty | ToTerm("GROUP") + "BY" + exprList;
        havingOpt.Rule = Empty | ToTerm("HAVING") + expr;

        // ------------------------------------------------------------ FROM clause

        tableRefList.Rule = MakePlusRule(tableRefList, comma, tableRef);
        tableRef.Rule = tablePrimary | joinedTable;
        joinedTable.Rule =
            tableRef + "CROSS" + "JOIN" + tablePrimary
            | tableRef + "NATURAL" + joinTypeOpt + "JOIN" + tablePrimary
            | tableRef + joinTypeOpt + "JOIN" + tablePrimary + joinSpec;
        joinTypeOpt.Rule = Empty | "INNER" | ToTerm("LEFT") + outerOpt | ToTerm("RIGHT") + outerOpt | ToTerm("FULL") + outerOpt;
        outerOpt.Rule = Empty | "OUTER";
        joinSpec.Rule = ToTerm("ON") + expr | ToTerm("USING") + lParen + nameList + rParen;
        nameList.Rule = MakePlusRule(nameList, comma, name);
        tablePrimary.Rule = qualifiedName + aliasOpt | lParen + queryExpr + rParen + derivedAlias;
        derivedAlias.Rule = name | ToTerm("AS") + name;

        // ------------------------------------------------------------ expressions

        expr.Rule = orExpr;
        exprList.Rule = MakePlusRule(exprList, comma, expr);
        orExpr.Rule = andExpr | orExpr + "OR" + andExpr;
        andExpr.Rule = notExpr | andExpr + "AND" + notExpr;
        notExpr.Rule = ToTerm("NOT") + notExpr | predicate;
        predicate.Rule = addExpr + predicateSuffixOpt;
        predicateSuffixOpt.Rule = Empty | comparisonSuffix | quantifiedSuffix | isNullSuffix | betweenSuffix
                                  | inSubquerySuffix | inListSuffix | likeSuffix;
        comparisonSuffix.Rule = compOp + addExpr;
        quantifiedSuffix.Rule = compOp + quantifier + subquery;
        isNullSuffix.Rule = ToTerm("IS") + notOpt + "NULL";
        betweenSuffix.Rule = notOpt + "BETWEEN" + addExpr + "AND" + addExpr;
        inSubquerySuffix.Rule = notOpt + "IN" + subquery;
        inListSuffix.Rule = notOpt + "IN" + lParen + exprList + rParen;
        likeSuffix.Rule = notOpt + "LIKE" + addExpr + escapeOpt;
        escapeOpt.Rule = Empty | ToTerm("ESCAPE") + addExpr;
        notOpt.Rule = Empty | "NOT";
        compOp.Rule = ToTerm("=") | "<>" | "<" | ">" | "<=" | ">=";
        quantifier.Rule = ToTerm("ANY") | "SOME" | "ALL";

        addExpr.Rule = mulExpr | addExpr + addOp + mulExpr;
        addOp.Rule = ToTerm("+") | "-" | "||";
        mulExpr.Rule = unaryExpr | mulExpr + mulOp + unaryExpr;
        mulOp.Rule = star | "/";
        unaryExpr.Rule = primary | unaryOp + unaryExpr;
        unaryOp.Rule = ToTerm("-") | "+";

        primary.Rule = number | stringLiteral | "NULL" | caseExpr | castExpr | existsExpr
                       | functionCall | columnRef | subquery | lParen + expr + rParen;
        columnRef.Rule = qualifiedName;
        functionCall.Rule = qualifiedName + lParen + funcArgs + rParen;
        funcArgs.Rule = Empty | star | quantOpt + exprList;
        caseExpr.Rule = ToTerm("CASE") + caseOperandOpt + whenList + elseOpt + "END";
        caseOperandOpt.Rule = Empty | expr;
        whenList.Rule = MakePlusRule(whenList, whenClause);
        whenClause.Rule = ToTerm("WHEN") + expr + "THEN" + expr;
        elseOpt.Rule = Empty | ToTerm("ELSE") + expr;
        castExpr.Rule = ToTerm("CAST") + lParen + expr + "AS" + dataType + rParen;
        dataType.Rule = typeWords + typeParamsOpt;
        typeWords.Rule = MakePlusRule(typeWords, id);
        typeParamsOpt.Rule = Empty | lParen + intList + rParen;
        intList.Rule = MakePlusRule(intList, comma, number);
        existsExpr.Rule = ToTerm("EXISTS") + subquery;

        qualifiedName.Rule = MakePlusRule(qualifiedName, dot, name);
        name.Rule = id | quotedId;

        Root = statement;

        MarkPunctuation(
            "(", ")", ",", ".", ";",
            "SELECT", "FROM", "WHERE", "GROUP", "BY", "HAVING", "ORDER", "AS", "JOIN", "ON", "USING",
            "CASE", "WHEN", "THEN", "ELSE", "END", "CAST", "EXISTS",
            "IS", "BETWEEN", "IN", "LIKE", "ESCAPE", "AND", "OR");
        MarkReservedWords(SqlKeywords.Reserved.ToArray());
    }
}
