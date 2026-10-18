using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

internal sealed partial class SpracheGrammar
{
    private readonly Parser<QueryExpression> queryExpression = Rule.Create<QueryExpression>(nameof(queryExpression));
    private readonly Parser<(SetOperator Operator, bool All)> unionOperator = Rule.Create<(SetOperator Operator, bool All)>(nameof(unionOperator));
    private readonly Parser<QueryExpression> queryTerm = Rule.Create<QueryExpression>(nameof(queryTerm));
    private readonly Parser<QueryExpression> queryPrimary = Rule.Create<QueryExpression>(nameof(queryPrimary));
    private readonly Parser<QueryExpression> subquery = Rule.Create<QueryExpression>(nameof(subquery));
    private readonly Parser<QueryExpression> subqueryExpression = Rule.Create<QueryExpression>(nameof(subqueryExpression));
    private readonly Parser<QueryExpression> subqueryTerm = Rule.Create<QueryExpression>(nameof(subqueryTerm));
    private readonly Parser<QueryExpression> querySpecification = Rule.Create<QueryExpression>(nameof(querySpecification));
    private readonly Parser<SetQuantifier> setQuantifier = Rule.Create<SetQuantifier>(nameof(setQuantifier));
    private readonly Parser<IReadOnlyList<SelectItem>> selectList = Rule.Create<IReadOnlyList<SelectItem>>(nameof(selectList));
    private readonly Parser<SelectItem> selectItem = Rule.Create<SelectItem>(nameof(selectItem));
    private readonly Parser<string> alias = Rule.Create<string>(nameof(alias));

    private void DefineQueries()
    {
        var all = K.All.Return(true).Or(Parse.Return(false));     // ALL?

        // (unionOperator queryTerm)*
        var unionTail = (
            from op in unionOperator
            from right in queryTerm
            select (op, right)).Many();

        // (INTERSECT ALL? queryPrimary)*
        var intersectTail = (
            from _ in K.Intersect
            from a in all
            from right in queryPrimary
            select (a, right)).Many();

        // queryExpression ::= queryTerm (unionOperator queryTerm)*
        queryExpression.Define(
            from first in queryTerm
            from tail in unionTail
            select Build.UnionChain(first, tail));

        // unionOperator ::= (UNION | EXCEPT) ALL?
        unionOperator.Define(
            from op in K.Union.Return(SetOperator.Union).Or(K.Except.Return(SetOperator.Except))
            from a in all
            select (op, a));

        // queryTerm ::= queryPrimary (INTERSECT ALL? queryPrimary)*
        queryTerm.Define(
            from first in queryPrimary
            from tail in intersectTail
            select Build.IntersectChain(first, tail));

        // queryPrimary ::= querySpecification | '(' queryExpression ')'
        queryPrimary.Define(
            querySpecification
                .Or(Lex.Parens(queryExpression)));

        // subquery ::= '(' subqueryExpression ')'
        subquery.Define(Lex.Parens(subqueryExpression));

        // subqueryExpression ::= subqueryTerm (unionOperator queryTerm)*
        subqueryExpression.Define(
            from first in subqueryTerm
            from tail in unionTail
            select Build.UnionChain(first, tail));

        // subqueryTerm ::= querySpecification (INTERSECT ALL? queryPrimary)*
        subqueryTerm.Define(
            from first in querySpecification
            from tail in intersectTail
            select Build.IntersectChain(first, tail));

        // querySpecification ::= SELECT setQuantifier? selectList
        //                        FROM tableReference (',' tableReference)*
        //                        (WHERE expression)?
        //                        (GROUP BY expression (',' expression)*)?
        //                        (HAVING expression)?
        var optionalQuantifier = setQuantifier.Optional();
        var tableReferences = tableReference.DelimitedBy(Lex.Comma);
        var optionalWhere = (from _ in K.Where from condition in expression select condition).Optional();
        var optionalGroupBy = (from _ in K.Group from __ in K.By from list in expression.DelimitedBy(Lex.Comma) select list).Optional();
        var optionalHaving = (from _ in K.Having from condition in expression select condition).Optional();
        querySpecification.Define(
            from _ in K.Select
            from quantifier in optionalQuantifier
            from items in selectList
            from __ in K.From
            from tables in tableReferences
            from condition in optionalWhere
            from groupBy in optionalGroupBy
            from having in optionalHaving
            select Build.QuerySpecification(quantifier, items, tables, condition, groupBy, having));

        // setQuantifier ::= DISTINCT | ALL
        setQuantifier.Define(
            K.Distinct.Return(SetQuantifier.Distinct)
                .Or(K.All.Return(SetQuantifier.All)));

        // selectList ::= '*' | selectItem (',' selectItem)*
        var selectItems = selectItem.DelimitedBy(Lex.Comma);
        selectList.Define(
            Lex.Star.Return(Build.SelectAll)
                .Or(selectItems.Select(Build.List)));

        // selectItem ::= qualifiedName '.' '*'
        //              | expression alias?
        //   "qualifiedName '.'" is written as (identifier '.')+ so the parser stops in front of '*'.
        var qualifierPart =
            from id in identifier
            from _ in Lex.Dot
            select id;
        var optionalAlias = alias.Optional();
        selectItem.Define(
            (from qualifier in qualifierPart.AtLeastOnce()
             from _ in Lex.Star
             select Build.QualifiedStar(qualifier))
            .Or(from e in expression
                from a in optionalAlias
                select Build.ExpressionItem(e, a)));

        // alias ::= AS? identifier
        var optionalAs = K.As.Optional();
        alias.Define(
            from _ in optionalAs
            from name in identifier
            select name);
    }
}
