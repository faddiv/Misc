using Irony.Parsing;
using SqlParsers.Ast;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    private readonly Rule<QueryExpression> queryExpression = new(nameof(queryExpression));
    private readonly Rule<(SetOperator Operator, bool All)> unionOperator = new(nameof(unionOperator));
    private readonly Rule<bool> allOpt = new(nameof(allOpt));
    private readonly Rule<QueryExpression> queryTerm = new(nameof(queryTerm));
    private readonly Rule<QueryExpression> queryPrimary = new(nameof(queryPrimary));
    private readonly Rule<QueryExpression> subquery = new(nameof(subquery));
    private readonly Rule<QueryExpression> subqueryExpression = new(nameof(subqueryExpression));
    private readonly Rule<QueryExpression> subqueryTerm = new(nameof(subqueryTerm));
    private readonly Rule<QueryExpression> querySpecification = new(nameof(querySpecification));
    private readonly Rule<SetQuantifier> setQuantifierOpt = new(nameof(setQuantifierOpt));
    private readonly Rule<IReadOnlyList<SelectItem>> selectList = new(nameof(selectList));
    private readonly Rule<SelectItem[]> selectItemList = new(nameof(selectItemList));
    private readonly Rule<SelectItem> selectItem = new(nameof(selectItem));
    private readonly Rule<string?> aliasOpt = new(nameof(aliasOpt));
    private readonly Rule<Expression?> whereClauseOpt = new(nameof(whereClauseOpt));
    private readonly Rule<Expression[]?> groupByClauseOpt = new(nameof(groupByClauseOpt));
    private readonly Rule<Expression?> havingClauseOpt = new(nameof(havingClauseOpt));

    private void DefineQueries()
    {
        // queryExpression ::= queryTerm (unionOperator queryTerm)*
        queryExpression.Rule = queryTerm | queryExpression + unionOperator + queryTerm;
        queryExpression.ToAst = SetOperationChain;

        // unionOperator ::= (UNION | EXCEPT) ALL?
        unionOperator.Rule = ToTerm("UNION") + allOpt | ToTerm("EXCEPT") + allOpt;
        unionOperator.ToAst = n => (n.Is(0, "UNION") ? SetOperator.Union : SetOperator.Except, n.Get<bool>(1));   // [UNION|EXCEPT, allOpt]

        allOpt.Rule = Empty | "ALL";
        allOpt.ToAst = n => !n.IsEmpty();

        // queryTerm ::= queryPrimary (INTERSECT ALL? queryPrimary)*
        queryTerm.Rule = queryPrimary | queryTerm + "INTERSECT" + allOpt + queryPrimary;
        queryTerm.ToAst = IntersectChain;

        // queryPrimary ::= querySpecification | '(' queryExpression ')'
        queryPrimary.Rule = querySpecification | ToTerm("(") + queryExpression + ")";
        queryPrimary.ToAst = n => n.Get<QueryExpression>(0);

        // subquery ::= '(' subqueryExpression ')'
        //   A subquery inside an expression must start with SELECT; this keeps '(' unambiguous for LALR(1).
        subquery.Rule = ToTerm("(") + subqueryExpression + ")";
        subquery.ToAst = n => n.Get<QueryExpression>(0);

        // subqueryExpression ::= subqueryTerm (unionOperator queryTerm)*
        subqueryExpression.Rule = subqueryTerm | subqueryExpression + unionOperator + queryTerm;
        subqueryExpression.ToAst = SetOperationChain;

        // subqueryTerm ::= querySpecification (INTERSECT ALL? queryPrimary)*
        subqueryTerm.Rule = querySpecification | subqueryTerm + "INTERSECT" + allOpt + queryPrimary;
        subqueryTerm.ToAst = IntersectChain;

        // querySpecification ::= SELECT setQuantifier? selectList
        //                        FROM tableReference (',' tableReference)*
        //                        (WHERE expression)?
        //                        (GROUP BY expression (',' expression)*)?
        //                        (HAVING expression)?
        querySpecification.Rule = ToTerm("SELECT") + setQuantifierOpt + selectList
                                  + "FROM" + tableReferenceList
                                  + whereClauseOpt + groupByClauseOpt + havingClauseOpt;
        querySpecification.ToAst = n => Build.QuerySpecification(
            n.Get<SetQuantifier>(0),                   // [setQuantifierOpt,
            n.Get<IReadOnlyList<SelectItem>>(1),       //  selectList,
            n.Get<TableReference[]>(2),                //  tableReferenceList,
            n.Get<Expression?>(3),                     //  whereClauseOpt,
            n.Get<Expression[]?>(4),                   //  groupByClauseOpt,
            n.Get<Expression?>(5));                    //  havingClauseOpt]

        // setQuantifier ::= DISTINCT | ALL
        setQuantifierOpt.Rule = Empty | "DISTINCT" | "ALL";
        setQuantifierOpt.ToAst = n =>
            n.IsEmpty() ? SetQuantifier.None
            : n.Is(0, "DISTINCT") ? SetQuantifier.Distinct
            : SetQuantifier.All;

        // selectList ::= '*' | selectItem (',' selectItem)*
        selectList.Rule = ToTerm("*") | selectItemList;
        selectList.ToAst = n => n.Is(0, "*") ? Build.SelectAll : n.Get<SelectItem[]>(0);    // ["*"] | [selectItemList]

        selectItemList.Rule = MakePlusRule(selectItemList, ToTerm(","), selectItem);
        selectItemList.ToAst = n => n.Items<SelectItem>();

        // selectItem ::= qualifiedName '.' '*'
        //              | expression alias?
        selectItem.Rule = expression + aliasOpt | qualifiedName + "." + "*";
        selectItem.ToAst = n => n.Is(1, "*")
            ? Build.QualifiedStar(n.Get<string[]>(0))                       // [qualifiedName, "*"]
            : Build.ExpressionItem(n.Get<Expression>(0), n.Get<string?>(1)); // [expression, aliasOpt]

        // alias ::= AS? identifier
        aliasOpt.Rule = Empty | identifier | ToTerm("AS") + identifier;
        aliasOpt.ToAst = n => n.IsEmpty() ? null : n.Get<string>(0);        // [] | [identifier]

        // (WHERE expression)?
        whereClauseOpt.Rule = Empty | ToTerm("WHERE") + expression;
        whereClauseOpt.ToAst = n => n.IsEmpty() ? null : n.Get<Expression>(0);

        // (GROUP BY expression (',' expression)*)?
        groupByClauseOpt.Rule = Empty | ToTerm("GROUP") + "BY" + expressionList;
        groupByClauseOpt.ToAst = n => n.IsEmpty() ? null : n.Get<Expression[]>(0);

        // (HAVING expression)?
        havingClauseOpt.Rule = Empty | ToTerm("HAVING") + expression;
        havingClauseOpt.ToAst = n => n.IsEmpty() ? null : n.Get<Expression>(0);
    }

    // [term] | [expression, unionOperator, term]
    private static QueryExpression SetOperationChain(ParseTreeNode n) =>
        n.ChildNodes.Count == 1
            ? n.Get<QueryExpression>(0)
            : Build.SetOperation(n.Get<QueryExpression>(0), n.Get<(SetOperator, bool)>(1), n.Get<QueryExpression>(2));

    // [primary] | [term, INTERSECT, allOpt, primary]
    private static QueryExpression IntersectChain(ParseTreeNode n) =>
        n.ChildNodes.Count == 1
            ? n.Get<QueryExpression>(0)
            : Build.SetOperation(n.Get<QueryExpression>(0), (SetOperator.Intersect, n.Get<bool>(2)), n.Get<QueryExpression>(3));
}
