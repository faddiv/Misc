using Parlot.Fluent;
using SqlParsers.Ast;

namespace SqlParsers.Parlot;

internal sealed partial class ParlotGrammar
{
    private readonly Rule<QueryExpression> queryExpression = new() { Name = "queryExpression" };
    private readonly Rule<(SetOperator Operator, bool All)> unionOperator = new() { Name = "unionOperator" };
    private readonly Rule<QueryExpression> queryTerm = new() { Name = "queryTerm" };
    private readonly Rule<QueryExpression> queryPrimary = new() { Name = "queryPrimary" };
    private readonly Rule<QueryExpression> subquery = new() { Name = "subquery" };
    private readonly Rule<QueryExpression> subqueryExpression = new() { Name = "subqueryExpression" };
    private readonly Rule<QueryExpression> subqueryTerm = new() { Name = "subqueryTerm" };
    private readonly Rule<QueryExpression> querySpecification = new() { Name = "querySpecification" };
    private readonly Rule<SetQuantifier> setQuantifier = new() { Name = "setQuantifier" };
    private readonly Rule<IReadOnlyList<SelectItem>> selectList = new() { Name = "selectList" };
    private readonly Rule<SelectItem> selectItem = new() { Name = "selectItem" };
    private readonly Rule<string> alias = new() { Name = "alias" };

    private void DefineQueries()
    {
        // ALL?  (ZeroOrOne yields false when absent)
        var all = ZeroOrOne(K.All.Then(static _ => true));

        // queryExpression ::= queryTerm (unionOperator queryTerm)*
        queryExpression.Definition = queryTerm
            .And(ZeroOrMany(unionOperator.And(queryTerm)))
            .Then(Build.UnionChain);

        // unionOperator ::= (UNION | EXCEPT) ALL?
        unionOperator.Definition = OneOf(
                K.Union.Then(static _ => SetOperator.Union),
                K.Except.Then(static _ => SetOperator.Except))
            .And(all);

        // queryTerm ::= queryPrimary (INTERSECT ALL? queryPrimary)*
        queryTerm.Definition = queryPrimary
            .And(ZeroOrMany(K.Intersect.SkipAnd(all).And(queryPrimary)))
            .Then(Build.IntersectChain);

        // queryPrimary ::= querySpecification | '(' queryExpression ')'
        queryPrimary.Definition = OneOf(
            querySpecification,
            Lex.Parens(queryExpression));

        // subquery ::= '(' subqueryExpression ')'
        subquery.Definition = Lex.Parens(subqueryExpression);

        // subqueryExpression ::= subqueryTerm (unionOperator queryTerm)*
        subqueryExpression.Definition = subqueryTerm
            .And(ZeroOrMany(unionOperator.And(queryTerm)))
            .Then(Build.UnionChain);

        // subqueryTerm ::= querySpecification (INTERSECT ALL? queryPrimary)*
        subqueryTerm.Definition = querySpecification
            .And(ZeroOrMany(K.Intersect.SkipAnd(all).And(queryPrimary)))
            .Then(Build.IntersectChain);

        // querySpecification ::= SELECT setQuantifier? selectList
        //                        FROM tableReference (',' tableReference)*
        //                        (WHERE expression)?
        //                        (GROUP BY expression (',' expression)*)?
        //                        (HAVING expression)?
        querySpecification.Definition = K.Select.SkipAnd(ZeroOrOne(setQuantifier)).And(selectList)
            .AndSkip(K.From).And(Separated(Lex.Comma, tableReference))
            .And(ZeroOrOne(K.Where.SkipAnd(expression)))
            .And(ZeroOrOne(K.Group.SkipAnd(K.By).SkipAnd(Separated(Lex.Comma, expression))))
            .And(ZeroOrOne(K.Having.SkipAnd(expression)))
            .Then(Build.QuerySpecification);

        // setQuantifier ::= DISTINCT | ALL   (ZeroOrOne(setQuantifier) yields SetQuantifier.None when absent)
        setQuantifier.Definition = OneOf(
            K.Distinct.Then(static _ => SetQuantifier.Distinct),
            K.All.Then(static _ => SetQuantifier.All));

        // selectList ::= '*' | selectItem (',' selectItem)*
        selectList.Definition = OneOf(
            Lex.Star.Then(static _ => Build.SelectAll),
            Separated(Lex.Comma, selectItem));

        // selectItem ::= qualifiedName '.' '*'
        //              | expression alias?
        //   "qualifiedName '.'" is written as (identifier '.')+ so the parser stops in front of '*'.
        selectItem.Definition = OneOf(
            OneOrMany(identifier.AndSkip(Lex.Dot)).AndSkip(Lex.Star).Then(Build.QualifiedStar),
            expression.And(ZeroOrOne(alias)).Then(Build.ExpressionItem));

        // alias ::= AS? identifier
        alias.Definition = ZeroOrOne(K.As).SkipAnd(identifier);
    }
}
