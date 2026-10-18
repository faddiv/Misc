using Farkle.Builder;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

internal sealed partial class FarkleGrammar
{
    private readonly Nonterminal<SelectStatement> statement = Nonterminal.Create<SelectStatement>(nameof(statement));
    private readonly Nonterminal<List<OrderByItem>?> orderByClauseOpt = Nonterminal.Create<List<OrderByItem>?>(nameof(orderByClauseOpt));
    private readonly Nonterminal<List<OrderByItem>> orderItemList = Nonterminal.Create<List<OrderByItem>>(nameof(orderItemList));
    private readonly Nonterminal<OrderByItem> orderItem = Nonterminal.Create<OrderByItem>(nameof(orderItem));

    private void DefineStatement()
    {
        // statement ::= queryExpression orderByClause? ';'?
        var semicolonOpt = Nonterminal.CreateUntyped("semicolonOpt", Empty, ";".Appended());
        statement.SetProductions(
            queryExpression.Extended().Extend(orderByClauseOpt).Append(semicolonOpt).Finish(Build.Statement));

        // orderByClause ::= ORDER BY orderItem (',' orderItem)*
        orderByClauseOpt.SetProductions(
            Empty.FinishConstant<List<OrderByItem>?>(null),
            "ORDER".Appended().Append("BY").Extend(orderItemList).Finish(Build.Some));
        DefineList(orderItemList, orderItem, ",");

        // orderItem ::= expression (ASC | DESC)?
        orderItem.SetProductions(
            expression.Extended().Finish(Build.Ascending),
            expression.Extended().Append("ASC").Finish(Build.Ascending),
            expression.Extended().Append("DESC").Finish(Build.Descending));
    }
}
