using Superpower;
using SqlParsers.Ast;

namespace SqlParsers.Superpower;

internal sealed partial class SuperpowerGrammar
{
    private readonly TokenListParser<SqlToken, SelectStatement> statement = Rule.Create<SelectStatement>(nameof(statement));
    private readonly TokenListParser<SqlToken, IReadOnlyList<OrderByItem>> orderByClause = Rule.Create<IReadOnlyList<OrderByItem>>(nameof(orderByClause));
    private readonly TokenListParser<SqlToken, OrderByItem> orderItem = Rule.Create<OrderByItem>(nameof(orderItem));

    private void DefineStatement()
    {
        // statement ::= queryExpression orderByClause? ';'?
        //   (plus the end of the input, which the EBNF leaves implicit)
        var optionalOrderBy = orderByClause.OptionalOrDefault();
        var optionalSemicolon = Lex.Semicolon.Optional();
        statement.Define((
                from query in queryExpression
                from orderBy in optionalOrderBy
                from _ in optionalSemicolon
                select Build.Statement(query, orderBy))
            .AtEnd());

        // orderByClause ::= ORDER BY orderItem (',' orderItem)*
        orderByClause.Define(
            K.Order.IgnoreThen(K.By)
                .IgnoreThen(orderItem.AtLeastOnceDelimitedBy(Lex.Comma))
                .Select(Build.List));

        // orderItem ::= expression (ASC | DESC)?
        var direction = K.Asc.Value(SortDirection.Ascending)
            .Or(K.Desc.Value(SortDirection.Descending))
            .OptionalOrDefault(SortDirection.Ascending);
        orderItem.Define(
            from e in expression
            from d in direction
            select Build.OrderItem(e, d));
    }
}
