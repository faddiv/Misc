using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

internal sealed partial class SpracheGrammar
{
    private readonly Parser<SelectStatement> statement = Rule.Create<SelectStatement>(nameof(statement));
    private readonly Parser<IReadOnlyList<OrderByItem>> orderByClause = Rule.Create<IReadOnlyList<OrderByItem>>(nameof(orderByClause));
    private readonly Parser<OrderByItem> orderItem = Rule.Create<OrderByItem>(nameof(orderItem));

    private void DefineStatement()
    {
        // statement ::= queryExpression orderByClause? ';'?
        //   (plus leading whitespace and the end of the input, which the EBNF leaves implicit)
        var optionalOrderBy = orderByClause.Optional();
        var optionalSemicolon = Lex.Semicolon.Optional();
        statement.Define((
                from _ in Lex.Skip
                from query in queryExpression
                from orderBy in optionalOrderBy
                from __ in optionalSemicolon
                select Build.Statement(query, orderBy))
            .End());

        // orderByClause ::= ORDER BY orderItem (',' orderItem)*
        var orderItems = orderItem.DelimitedBy(Lex.Comma);
        orderByClause.Define(
            from _ in K.Order
            from __ in K.By
            from items in orderItems
            select Build.List(items));

        // orderItem ::= expression (ASC | DESC)?
        var direction = K.Asc.Return(SortDirection.Ascending)
            .Or(K.Desc.Return(SortDirection.Descending))
            .Or(Parse.Return(SortDirection.Ascending));
        orderItem.Define(
            from e in expression
            from d in direction
            select Build.OrderItem(e, d));
    }
}
