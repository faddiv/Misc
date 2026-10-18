using SqlParsers.Ast;
using static SqlParsers.Antlr.Generated.Sql92SelectParser;

namespace SqlParsers.Antlr;

internal static partial class AntlrAstBuilder
{
    // statement : queryExpression orderByClause? SEMICOLON? EOF ;
    // orderByClause : ORDER BY orderItem (COMMA orderItem)* ;
    public static SelectStatement ToAst(StatementContext c) =>
        new(
            ToAst(c.queryExpression()),
            c.orderByClause() is { } orderBy ? orderBy.orderItem().Select(ToAst).ToArray() : Array.Empty<OrderByItem>());

    // orderItem : expression (ASC | DESC)? ;
    private static OrderByItem ToAst(OrderItemContext c) =>
        new(ToAst(c.expression()), c.DESC() is not null ? SortDirection.Descending : SortDirection.Ascending);
}
