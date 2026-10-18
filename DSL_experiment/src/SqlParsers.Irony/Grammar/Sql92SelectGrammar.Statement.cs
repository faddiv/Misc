using Irony.Parsing;
using SqlParsers.Ast;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    private readonly Rule<SelectStatement> statement = new(nameof(statement));
    private readonly NonTerminal semicolonOpt = new(nameof(semicolonOpt));
    private readonly Rule<IReadOnlyList<OrderByItem>?> orderByClauseOpt = new(nameof(orderByClauseOpt));
    private readonly Rule<IReadOnlyList<OrderByItem>> orderItemList = new(nameof(orderItemList));
    private readonly Rule<OrderByItem> orderItem = new(nameof(orderItem));
    private readonly Rule<SortDirection> directionOpt = new(nameof(directionOpt));

    private void DefineStatement()
    {
        // statement ::= queryExpression orderByClause? ';'?
        statement.Rule = queryExpression + orderByClauseOpt + semicolonOpt;
        statement.ToAst = n => Build.Statement(       // [queryExpression, orderByClauseOpt, semicolonOpt]
            n.Get<QueryExpression>(0),
            n.Get<IReadOnlyList<OrderByItem>?>(1));

        semicolonOpt.Rule = Empty | ";";

        // orderByClause ::= ORDER BY orderItem (',' orderItem)*
        orderByClauseOpt.Rule = Empty | ToTerm("ORDER") + "BY" + orderItemList;
        orderByClauseOpt.ToAst = n => n.IsEmpty() ? null : n.Get<IReadOnlyList<OrderByItem>>(0);   // [] | [orderItemList]

        orderItemList.Rule = MakePlusRule(orderItemList, ToTerm(","), orderItem);
        orderItemList.ToAst = n => n.Items<OrderByItem>();

        // orderItem ::= expression (ASC | DESC)?
        orderItem.Rule = expression + directionOpt;
        orderItem.ToAst = n => Build.OrderItem(n.Get<Expression>(0), n.Get<SortDirection>(1));   // [expression, directionOpt]

        directionOpt.Rule = Empty | "ASC" | "DESC";
        directionOpt.ToAst = n => !n.IsEmpty() && n.Is(0, "DESC") ? SortDirection.Descending : SortDirection.Ascending;
    }
}
