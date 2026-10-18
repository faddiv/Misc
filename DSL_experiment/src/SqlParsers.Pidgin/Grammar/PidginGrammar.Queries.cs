using Pidgin;
using SqlParsers.Ast;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace SqlParsers.Pidgin;

internal sealed partial class PidginGrammar
{
    private readonly Rule<QueryExpression> queryExpression = new(nameof(queryExpression));
    private readonly Rule<(SetOperator Operator, bool All)> unionOperator = new(nameof(unionOperator));
    private readonly Rule<QueryExpression> queryTerm = new(nameof(queryTerm));
    private readonly Rule<QueryExpression> queryPrimary = new(nameof(queryPrimary));
    private readonly Rule<QueryExpression> subquery = new(nameof(subquery));
    private readonly Rule<QueryExpression> subqueryExpression = new(nameof(subqueryExpression));
    private readonly Rule<QueryExpression> subqueryTerm = new(nameof(subqueryTerm));
    private readonly Rule<QueryExpression> querySpecification = new(nameof(querySpecification));
    private readonly Rule<SetQuantifier> setQuantifier = new(nameof(setQuantifier));
    private readonly Rule<IReadOnlyList<SelectItem>> selectList = new(nameof(selectList));
    private readonly Rule<SelectItem> selectItem = new(nameof(selectItem));
    private readonly Rule<string> alias = new(nameof(alias));

    private void DefineQueries()
    {
        var all = K.All.ThenReturn(true).Or(Return(false));                                     // ALL?
        var unionTail = unionOperator.Then(queryTerm, (op, right) => (op, right)).Many();      // (unionOperator queryTerm)*
        var intersectTail = K.Intersect.Then(all).Then(queryPrimary, (a, right) => (a, right)).Many();   // (INTERSECT ALL? queryPrimary)*

        // queryExpression ::= queryTerm (unionOperator queryTerm)*
        queryExpression.Definition = Map(Build.UnionChain, queryTerm, unionTail);

        // unionOperator ::= (UNION | EXCEPT) ALL?
        unionOperator.Definition = Map(
            (op, a) => (op, a),
            OneOf(
                K.Union.ThenReturn(SetOperator.Union),
                K.Except.ThenReturn(SetOperator.Except)),
            all);

        // queryTerm ::= queryPrimary (INTERSECT ALL? queryPrimary)*
        queryTerm.Definition = Map(Build.IntersectChain, queryPrimary, intersectTail);

        // queryPrimary ::= querySpecification | '(' queryExpression ')'
        queryPrimary.Definition = OneOf(
            querySpecification,
            Lex.Parens(queryExpression));

        // subquery ::= '(' subqueryExpression ')'
        subquery.Definition = Lex.Parens(subqueryExpression);

        // subqueryExpression ::= subqueryTerm (unionOperator queryTerm)*
        subqueryExpression.Definition = Map(Build.UnionChain, subqueryTerm, unionTail);

        // subqueryTerm ::= querySpecification (INTERSECT ALL? queryPrimary)*
        subqueryTerm.Definition = Map(Build.IntersectChain, querySpecification, intersectTail);

        // querySpecification ::= SELECT setQuantifier? selectList
        //                        FROM tableReference (',' tableReference)*
        //                        (WHERE expression)?
        //                        (GROUP BY expression (',' expression)*)?
        //                        (HAVING expression)?
        querySpecification.Definition = Map(
            Build.QuerySpecification,
            K.Select.Then(setQuantifier.Optional()),
            selectList,
            K.From.Then(tableReference.SeparatedAtLeastOnce(Lex.Comma)),
            K.Where.Then(expression).Optional(),
            K.Group.Then(K.By).Then(expression.SeparatedAtLeastOnce(Lex.Comma)).Optional(),
            K.Having.Then(expression).Optional());

        // setQuantifier ::= DISTINCT | ALL
        setQuantifier.Definition = OneOf(
            K.Distinct.ThenReturn(SetQuantifier.Distinct),
            K.All.ThenReturn(SetQuantifier.All));

        // selectList ::= '*' | selectItem (',' selectItem)*
        selectList.Definition = OneOf(
            Lex.Star.ThenReturn(Build.SelectAll),
            selectItem.SeparatedAtLeastOnce(Lex.Comma).Select(Build.List));

        // selectItem ::= qualifiedName '.' '*'
        //              | expression alias?
        //   "qualifiedName '.'" is written as (identifier '.')+ so the parser stops in front of '*'.
        //   Try: "t.col" also starts with "identifier '.'", so the first alternative must be able to back out.
        selectItem.Definition = OneOf(
            Try(identifier.Before(Lex.Dot).AtLeastOnce().Before(Lex.Star)).Select(Build.QualifiedStar),
            Map(Build.ExpressionItem, expression, alias.Optional()));

        // alias ::= AS? identifier
        alias.Definition = K.As.Optional().Then(identifier);
    }
}
