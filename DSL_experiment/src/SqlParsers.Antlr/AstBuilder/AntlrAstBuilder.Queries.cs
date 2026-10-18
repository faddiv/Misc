using SqlParsers.Ast;
using static SqlParsers.Antlr.Generated.Sql92SelectParser;

namespace SqlParsers.Antlr;

internal static partial class AntlrAstBuilder
{
    // queryExpression : queryTerm (unionOperator queryTerm)* ;
    private static QueryExpression ToAst(QueryExpressionContext c)
    {
        var result = ToAst(c.queryTerm(0));
        var operators = c.unionOperator();
        for (var i = 0; i < operators.Length; i++)
            result = SetOperation(result, ToAst(operators[i]), ToAst(c.queryTerm(i + 1)));
        return result;
    }

    // unionOperator : (UNION | EXCEPT) ALL? ;
    private static (SetOperator Operator, bool All) ToAst(UnionOperatorContext c) =>
        (c.UNION() is not null ? SetOperator.Union : SetOperator.Except, c.ALL() is not null);

    // queryTerm : queryPrimary (intersectOperator queryPrimary)* ;
    // intersectOperator : INTERSECT ALL? ;
    private static QueryExpression ToAst(QueryTermContext c)
    {
        var result = ToAst(c.queryPrimary(0));
        var operators = c.intersectOperator();
        for (var i = 0; i < operators.Length; i++)
            result = SetOperation(result, (SetOperator.Intersect, operators[i].ALL() is not null), ToAst(c.queryPrimary(i + 1)));
        return result;
    }

    // queryPrimary : querySpecification | LPAREN queryExpression RPAREN ;
    private static QueryExpression ToAst(QueryPrimaryContext c) =>
        c.querySpecification() is { } specification ? ToAst(specification) : ToAst(c.queryExpression());

    // subquery : LPAREN subqueryExpression RPAREN ;
    private static QueryExpression ToAst(SubqueryContext c) => ToAst(c.subqueryExpression());

    // subqueryExpression : subqueryTerm (unionOperator queryTerm)* ;
    private static QueryExpression ToAst(SubqueryExpressionContext c)
    {
        var result = ToAst(c.subqueryTerm());
        var operators = c.unionOperator();
        for (var i = 0; i < operators.Length; i++)
            result = SetOperation(result, ToAst(operators[i]), ToAst(c.queryTerm(i)));
        return result;
    }

    // subqueryTerm : querySpecification (intersectOperator queryPrimary)* ;
    private static QueryExpression ToAst(SubqueryTermContext c)
    {
        var result = ToAst(c.querySpecification());
        var operators = c.intersectOperator();
        for (var i = 0; i < operators.Length; i++)
            result = SetOperation(result, (SetOperator.Intersect, operators[i].ALL() is not null), ToAst(c.queryPrimary(i)));
        return result;
    }

    private static QueryExpression SetOperation(QueryExpression left, (SetOperator Operator, bool All) op, QueryExpression right) =>
        new Ast.SetOperation(left, op.Operator, op.All, right);

    // querySpecification
    //     : SELECT setQuantifier? selectList
    //       FROM tableReference (COMMA tableReference)*
    //       whereClause? groupByClause? havingClause? ;
    // whereClause : WHERE expression ;
    // groupByClause : GROUP BY expression (COMMA expression)* ;
    // havingClause : HAVING expression ;
    private static QueryExpression ToAst(QuerySpecificationContext c) =>
        new QuerySpecification(
            ToAst(c.setQuantifier()),
            ToAst(c.selectList()),
            c.tableReference().Select(ToAst).ToArray(),
            c.whereClause() is { } where ? ToAst(where.expression()) : null,
            c.groupByClause() is { } groupBy ? groupBy.expression().Select(ToAst).ToArray() : NoExpressions,
            c.havingClause() is { } having ? ToAst(having.expression()) : null);

    // setQuantifier : DISTINCT | ALL ;   (absent: SetQuantifier.None)
    private static SetQuantifier ToAst(SetQuantifierContext? c) =>
        c is null ? SetQuantifier.None
        : c.DISTINCT() is not null ? SetQuantifier.Distinct
        : SetQuantifier.All;

    // selectList : STAR | selectItem (COMMA selectItem)* ;
    private static IReadOnlyList<SelectItem> ToAst(SelectListContext c) =>
        c.STAR() is not null
            ? new SelectItem[] { new AllColumns(Array.Empty<string>()) }
            : c.selectItem().Select(ToAst).ToArray();

    // selectItem
    //     : qualifiedName DOT STAR            # qualifiedStar
    //     | expression (AS? identifier)?      # expressionItem
    //     ;
    private static SelectItem ToAst(SelectItemContext c) => c switch
    {
        QualifiedStarContext q => new AllColumns(ToAst(q.qualifiedName())),
        ExpressionItemContext e => new ExpressionItem(ToAst(e.expression()), e.identifier() is { } alias ? ToAst(alias) : null),
        _ => throw new InvalidOperationException(c.GetType().Name),
    };
}
