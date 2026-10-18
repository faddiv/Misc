using Superpower;
using SqlParsers.Ast;

namespace SqlParsers.Superpower;

internal sealed partial class SuperpowerGrammar
{
    private readonly TokenListParser<SqlToken, QueryExpression> queryExpression = Rule.Create<QueryExpression>(nameof(queryExpression));
    private readonly TokenListParser<SqlToken, (SetOperator Operator, bool All)> unionOperator = Rule.Create<(SetOperator Operator, bool All)>(nameof(unionOperator));
    private readonly TokenListParser<SqlToken, QueryExpression> queryTerm = Rule.Create<QueryExpression>(nameof(queryTerm));
    private readonly TokenListParser<SqlToken, QueryExpression> queryPrimary = Rule.Create<QueryExpression>(nameof(queryPrimary));
    private readonly TokenListParser<SqlToken, QueryExpression> subquery = Rule.Create<QueryExpression>(nameof(subquery));
    private readonly TokenListParser<SqlToken, QueryExpression> subqueryExpression = Rule.Create<QueryExpression>(nameof(subqueryExpression));
    private readonly TokenListParser<SqlToken, QueryExpression> subqueryTerm = Rule.Create<QueryExpression>(nameof(subqueryTerm));
    private readonly TokenListParser<SqlToken, QueryExpression> querySpecification = Rule.Create<QueryExpression>(nameof(querySpecification));
    private readonly TokenListParser<SqlToken, SetQuantifier> setQuantifier = Rule.Create<SetQuantifier>(nameof(setQuantifier));
    private readonly TokenListParser<SqlToken, IReadOnlyList<SelectItem>> selectList = Rule.Create<IReadOnlyList<SelectItem>>(nameof(selectList));
    private readonly TokenListParser<SqlToken, SelectItem> selectItem = Rule.Create<SelectItem>(nameof(selectItem));
    private readonly TokenListParser<SqlToken, string> alias = Rule.Create<string>(nameof(alias));

    private void DefineQueries()
    {
        var all = K.All.Value(true).OptionalOrDefault(false);     // ALL?

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
            from op in K.Union.Value(SetOperator.Union).Or(K.Except.Value(SetOperator.Except))
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
        var optionalQuantifier = setQuantifier.OptionalOrDefault(SetQuantifier.None);
        var tableReferences = tableReference.AtLeastOnceDelimitedBy(Lex.Comma);
        var optionalWhere = K.Where.IgnoreThen(expression).OptionalOrDefault();
        var optionalGroupBy = K.Group.IgnoreThen(K.By).IgnoreThen(expression.AtLeastOnceDelimitedBy(Lex.Comma)).OptionalOrDefault();
        var optionalHaving = K.Having.IgnoreThen(expression).OptionalOrDefault();
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
            K.Distinct.Value(SetQuantifier.Distinct)
                .Or(K.All.Value(SetQuantifier.All)));

        // selectList ::= '*' | selectItem (',' selectItem)*
        selectList.Define(
            Lex.Star.Value(Build.SelectAll)
                .Or(selectItem.AtLeastOnceDelimitedBy(Lex.Comma).Select(Build.List)));

        // selectItem ::= qualifiedName '.' '*'
        //              | expression alias?
        //   "qualifiedName '.'" is written as (identifier '.')+ so the parser stops in front of '*'.
        //   Try(): "t.col" also starts with "identifier '.'", so the first alternative must be able to back out.
        var qualifierPart =
            from id in identifier
            from _ in Lex.Dot
            select id;
        var optionalAlias = alias.OptionalOrDefault();
        selectItem.Define(
            (from qualifier in qualifierPart.AtLeastOnce()
             from _ in Lex.Star
             select Build.QualifiedStar(qualifier)).Try()
            .Or(from e in expression
                from a in optionalAlias
                select Build.ExpressionItem(e, a)));

        // alias ::= AS? identifier
        alias.Define(K.As.Optional().IgnoreThen(identifier));
    }
}
