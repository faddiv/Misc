using Parlot.Fluent;
using SqlParsers.Ast;

namespace SqlParsers.Parlot;

internal sealed partial class ParlotGrammar
{
    private readonly Rule<SelectStatement> statement = new() { Name = "statement" };
    private readonly Rule<IReadOnlyList<OrderByItem>> orderByClause = new() { Name = "orderByClause" };
    private readonly Rule<OrderByItem> orderItem = new() { Name = "orderItem" };

    private void DefineStatement()
    {
        // statement ::= queryExpression orderByClause? ';'?
        //   (plus leading whitespace and the end of the input, which the EBNF leaves implicit)
        statement.Definition = Lex.Skip.SkipAnd(queryExpression)
            .And(ZeroOrOne(orderByClause))
            .AndSkip(ZeroOrOne(Lex.Semicolon))
            .Eof()
            .Then(Build.Statement);

        // orderByClause ::= ORDER BY orderItem (',' orderItem)*
        orderByClause.Definition = K.Order.SkipAnd(K.By).SkipAnd(Separated(Lex.Comma, orderItem));

        // orderItem ::= expression (ASC | DESC)?
        orderItem.Definition = expression
            .And(ZeroOrOne(OneOf(
                K.Asc.Then(static _ => SortDirection.Ascending),
                K.Desc.Then(static _ => SortDirection.Descending))))
            .Then(Build.OrderItem);
    }
}
