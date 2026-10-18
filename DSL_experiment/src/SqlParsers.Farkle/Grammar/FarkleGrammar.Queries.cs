using Farkle.Builder;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

internal sealed partial class FarkleGrammar
{
    private readonly Nonterminal<QueryExpression> queryExpression = Nonterminal.Create<QueryExpression>(nameof(queryExpression));
    private readonly Nonterminal<(SetOperator Operator, bool All)> unionOperator = Nonterminal.Create<(SetOperator Operator, bool All)>(nameof(unionOperator));
    private readonly Nonterminal<bool> allOpt = Nonterminal.Create<bool>(nameof(allOpt));
    private readonly Nonterminal<QueryExpression> queryTerm = Nonterminal.Create<QueryExpression>(nameof(queryTerm));
    private readonly Nonterminal<QueryExpression> queryPrimary = Nonterminal.Create<QueryExpression>(nameof(queryPrimary));
    private readonly Nonterminal<QueryExpression> subquery = Nonterminal.Create<QueryExpression>(nameof(subquery));
    private readonly Nonterminal<QueryExpression> subqueryExpression = Nonterminal.Create<QueryExpression>(nameof(subqueryExpression));
    private readonly Nonterminal<QueryExpression> subqueryTerm = Nonterminal.Create<QueryExpression>(nameof(subqueryTerm));
    private readonly Nonterminal<QueryExpression> querySpecification = Nonterminal.Create<QueryExpression>(nameof(querySpecification));
    private readonly Nonterminal<SetQuantifier> setQuantifierOpt = Nonterminal.Create<SetQuantifier>(nameof(setQuantifierOpt));
    private readonly Nonterminal<IReadOnlyList<SelectItem>> selectList = Nonterminal.Create<IReadOnlyList<SelectItem>>(nameof(selectList));
    private readonly Nonterminal<List<SelectItem>> selectItemList = Nonterminal.Create<List<SelectItem>>(nameof(selectItemList));
    private readonly Nonterminal<SelectItem> selectItem = Nonterminal.Create<SelectItem>(nameof(selectItem));
    private readonly Nonterminal<string?> aliasOpt = Nonterminal.Create<string?>(nameof(aliasOpt));
    private readonly Nonterminal<Expression?> whereClauseOpt = Nonterminal.Create<Expression?>(nameof(whereClauseOpt));
    private readonly Nonterminal<IReadOnlyList<Expression>> groupByClauseOpt = Nonterminal.Create<IReadOnlyList<Expression>>(nameof(groupByClauseOpt));
    private readonly Nonterminal<Expression?> havingClauseOpt = Nonterminal.Create<Expression?>(nameof(havingClauseOpt));

    private void DefineQueries()
    {
        // queryExpression ::= queryTerm (unionOperator queryTerm)*
        queryExpression.SetProductions(
            queryTerm.AsProduction(),
            queryExpression.Extended().Extend(unionOperator).Extend(queryTerm).Finish(Build.SetOperation));

        // unionOperator ::= (UNION | EXCEPT) ALL?
        unionOperator.SetProductions(
            "UNION".Appended().Extend(allOpt).Finish(Build.Union),
            "EXCEPT".Appended().Extend(allOpt).Finish(Build.Except));

        allOpt.SetProductions(
            Empty.FinishConstant(false),
            "ALL".Appended().FinishConstant(true));

        // queryTerm ::= queryPrimary (INTERSECT ALL? queryPrimary)*
        queryTerm.SetProductions(
            queryPrimary.AsProduction(),
            queryTerm.Extended().Append("INTERSECT").Extend(allOpt).Extend(queryPrimary).Finish(Build.Intersect));

        // queryPrimary ::= querySpecification | '(' queryExpression ')'
        queryPrimary.SetProductions(
            querySpecification.AsProduction(),
            "(".Appended().Extend(queryExpression).Append(")").AsProduction());

        // subquery ::= '(' subqueryExpression ')'
        //   A subquery inside an expression must start with SELECT; this keeps '(' unambiguous for LALR(1).
        subquery.SetProductions(
            "(".Appended().Extend(subqueryExpression).Append(")").AsProduction());

        // subqueryExpression ::= subqueryTerm (unionOperator queryTerm)*
        subqueryExpression.SetProductions(
            subqueryTerm.AsProduction(),
            subqueryExpression.Extended().Extend(unionOperator).Extend(queryTerm).Finish(Build.SetOperation));

        // subqueryTerm ::= querySpecification (INTERSECT ALL? queryPrimary)*
        subqueryTerm.SetProductions(
            querySpecification.AsProduction(),
            subqueryTerm.Extended().Append("INTERSECT").Extend(allOpt).Extend(queryPrimary).Finish(Build.Intersect));

        // querySpecification ::= SELECT setQuantifier? selectList
        //                        FROM tableReference (',' tableReference)*
        //                        (WHERE expression)?
        //                        (GROUP BY expression (',' expression)*)?
        //                        (HAVING expression)?
        querySpecification.SetProductions(
            "SELECT".Appended().Extend(setQuantifierOpt).Extend(selectList)
                .Append("FROM").Extend(tableReferenceList)
                .Extend(whereClauseOpt).Extend(groupByClauseOpt).Extend(havingClauseOpt)
                .Finish(Build.QuerySpecification));

        // setQuantifier ::= DISTINCT | ALL
        setQuantifierOpt.SetProductions(
            Empty.FinishConstant(SetQuantifier.None),
            "DISTINCT".Appended().FinishConstant(SetQuantifier.Distinct),
            "ALL".Appended().FinishConstant(SetQuantifier.All));

        // selectList ::= '*' | selectItem (',' selectItem)*
        selectList.SetProductions(
            "*".Appended().FinishConstant(Build.SelectAll),
            selectItemList.Extended().Finish(Build.SelectItems));
        DefineList(selectItemList, selectItem, ",");

        // selectItem ::= qualifiedName '.' '*'
        //              | expression alias?
        selectItem.SetProductions(
            qualifiedName.Extended().Append(".").Append("*").Finish(Build.QualifiedStar),
            expression.Extended().Extend(aliasOpt).Finish(Build.ExpressionItem));

        // alias ::= AS? identifier
        aliasOpt.SetProductions(
            Empty.FinishConstant<string?>(null),
            identifier.Extended().Finish(Build.Some),
            "AS".Appended().Extend(identifier).Finish(Build.Some));

        // (WHERE expression)?
        whereClauseOpt.SetProductions(
            Empty.FinishConstant<Expression?>(null),
            "WHERE".Appended().Extend(expression).Finish(Build.Some));

        // (GROUP BY expression (',' expression)*)?
        groupByClauseOpt.SetProductions(
            Empty.FinishConstant(Build.NoExpressions),
            "GROUP".Appended().Append("BY").Extend(expressionList).Finish(Build.Expressions));

        // (HAVING expression)?
        havingClauseOpt.SetProductions(
            Empty.FinishConstant<Expression?>(null),
            "HAVING".Appended().Extend(expression).Finish(Build.Some));
    }
}
