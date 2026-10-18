using Pidgin;
using SqlParsers.Ast;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace SqlParsers.Pidgin;

internal sealed partial class PidginGrammar
{
    private readonly Rule<SelectStatement> statement = new(nameof(statement));
    private readonly Rule<IReadOnlyList<OrderByItem>> orderByClause = new(nameof(orderByClause));
    private readonly Rule<OrderByItem> orderItem = new(nameof(orderItem));

    private void DefineStatement()
    {
        // statement ::= queryExpression orderByClause? ';'?
        //   (plus leading whitespace and the end of the input, which the EBNF leaves implicit)
        statement.Definition = Lex.Skip
            .Then(Map(Build.Statement, queryExpression, orderByClause.Optional()))
            .Before(Lex.Semicolon.Optional())
            .Before(End);

        // orderByClause ::= ORDER BY orderItem (',' orderItem)*
        orderByClause.Definition = K.Order.Then(K.By)
            .Then(orderItem.SeparatedAtLeastOnce(Lex.Comma))
            .Select(Build.List);

        // orderItem ::= expression (ASC | DESC)?
        orderItem.Definition = Map(
            Build.OrderItem,
            expression,
            OneOf(
                K.Asc.ThenReturn(SortDirection.Ascending),
                K.Desc.ThenReturn(SortDirection.Descending)).Optional());
    }
}
