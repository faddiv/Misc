using System.Globalization;
using Antlr4.Runtime.Tree;
using SqlParsers.Antlr.Generated;
using SqlParsers.Ast;
using static SqlParsers.Antlr.Generated.Sql92SelectParser;

namespace SqlParsers.Antlr;

/// <summary>Converts the ANTLR parse tree into the shared AST.</summary>
internal static class AntlrAstBuilder
{
    private static readonly Expression[] NoExpressions = [];

    public static SelectStatement Statement(StatementContext c) =>
        new(
            Query(c.queryExpression()),
            c.orderByClause() is { } orderBy ? orderBy.orderItem().Select(OrderItem).ToArray() : Array.Empty<OrderByItem>());

    private static OrderByItem OrderItem(OrderItemContext c) =>
        new(Expr(c.expression()), c.DESC() is not null ? SortDirection.Descending : SortDirection.Ascending);

    // ------------------------------------------------------------ queries

    private static QueryExpression Query(QueryExpressionContext c)
    {
        var result = Term(c.queryTerm(0));
        var ops = c.unionOperator();
        for (var i = 0; i < ops.Length; i++)
            result = Union(result, ops[i], Term(c.queryTerm(i + 1)));
        return result;
    }

    private static QueryExpression Query(SubqueryExpressionContext c)
    {
        var result = Term(c.subqueryTerm());
        var ops = c.unionOperator();
        for (var i = 0; i < ops.Length; i++)
            result = Union(result, ops[i], Term(c.queryTerm(i)));
        return result;
    }

    private static QueryExpression Union(QueryExpression left, UnionOperatorContext op, QueryExpression right) =>
        new SetOperation(left, op.UNION() is not null ? SetOperator.Union : SetOperator.Except, op.ALL() is not null, right);

    private static QueryExpression Term(QueryTermContext c)
    {
        var result = Primary(c.queryPrimary(0));
        var ops = c.intersectOperator();
        for (var i = 0; i < ops.Length; i++)
            result = new SetOperation(result, SetOperator.Intersect, ops[i].ALL() is not null, Primary(c.queryPrimary(i + 1)));
        return result;
    }

    private static QueryExpression Term(SubqueryTermContext c)
    {
        var result = Specification(c.querySpecification());
        var ops = c.intersectOperator();
        for (var i = 0; i < ops.Length; i++)
            result = new SetOperation(result, SetOperator.Intersect, ops[i].ALL() is not null, Primary(c.queryPrimary(i)));
        return result;
    }

    private static QueryExpression Primary(QueryPrimaryContext c) =>
        c.querySpecification() is { } spec ? Specification(spec) : Query(c.queryExpression());

    private static QueryExpression SubqueryOf(SubqueryContext c) => Query(c.subqueryExpression());

    private static QueryExpression Specification(QuerySpecificationContext c) =>
        new QuerySpecification(
            QuantifierOf(c.setQuantifier()),
            SelectList(c.selectList()),
            c.tableReference().Select(TableRef).ToArray(),
            c.whereClause() is { } where ? Expr(where.expression()) : null,
            c.groupByClause() is { } groupBy ? groupBy.expression().Select(Expr).ToArray() : NoExpressions,
            c.havingClause() is { } having ? Expr(having.expression()) : null);

    private static SetQuantifier QuantifierOf(SetQuantifierContext? c) =>
        c is null ? SetQuantifier.None
        : c.DISTINCT() is not null ? SetQuantifier.Distinct
        : SetQuantifier.All;

    private static IReadOnlyList<SelectItem> SelectList(SelectListContext c) =>
        c.STAR() is not null
            ? new SelectItem[] { new AllColumns(Array.Empty<string>()) }
            : c.selectItem().Select(SelectItemOf).ToArray();

    private static SelectItem SelectItemOf(SelectItemContext c) => c switch
    {
        QualifiedStarContext q => new AllColumns(QualifiedName(q.qualifiedName())),
        ExpressionItemContext e => new ExpressionItem(Expr(e.expression()), e.identifier() is { } a ? Identifier(a) : null),
        _ => throw new InvalidOperationException(c.GetType().Name),
    };

    // ------------------------------------------------------------ FROM clause

    private static TableReference TableRef(TableReferenceContext c)
    {
        var result = TablePrimary(c.tablePrimary());
        foreach (var join in c.joinPart())
        {
            result = join switch
            {
                CrossJoinContext cross => new JoinedTable(result, JoinType.Cross, false, TablePrimary(cross.tablePrimary()), null),
                NaturalJoinContext natural => new JoinedTable(result, JoinTypeOf(natural.joinType()), true, TablePrimary(natural.tablePrimary()), null),
                QualifiedJoinContext q => new JoinedTable(result, JoinTypeOf(q.joinType()), false, TablePrimary(q.tablePrimary()), JoinConditionOf(q.joinSpecification())),
                _ => throw new InvalidOperationException(join.GetType().Name),
            };
        }
        return result;
    }

    private static JoinType JoinTypeOf(JoinTypeContext? c) =>
        c is null || c.INNER() is not null ? JoinType.Inner
        : c.LEFT() is not null ? JoinType.Left
        : c.RIGHT() is not null ? JoinType.Right
        : JoinType.Full;

    private static JoinCondition JoinConditionOf(JoinSpecificationContext c) =>
        c.ON() is not null
            ? new OnCondition(Expr(c.expression()))
            : new UsingCondition(c.identifier().Select(Identifier).ToArray());

    private static TableReference TablePrimary(TablePrimaryContext c) => c switch
    {
        NamedTableContext n => new NamedTable(QualifiedName(n.qualifiedName()), n.identifier() is { } a ? Identifier(a) : null),
        DerivedTableContext d => new DerivedTable(Query(d.queryExpression()), Identifier(d.identifier())),
        _ => throw new InvalidOperationException(c.GetType().Name),
    };

    // ------------------------------------------------------------ expressions

    private static Expression Expr(ExpressionContext c) => Or(c.orExpression());

    private static Expression Or(OrExpressionContext c)
    {
        var operands = c.andExpression();
        var result = And(operands[0]);
        for (var i = 1; i < operands.Length; i++)
            result = new BinaryExpression(BinaryOperator.Or, result, And(operands[i]));
        return result;
    }

    private static Expression And(AndExpressionContext c)
    {
        var operands = c.notExpression();
        var result = Not(operands[0]);
        for (var i = 1; i < operands.Length; i++)
            result = new BinaryExpression(BinaryOperator.And, result, Not(operands[i]));
        return result;
    }

    private static Expression Not(NotExpressionContext c) =>
        c.NOT() is not null
            ? new UnaryExpression(UnaryOperator.Not, Not(c.notExpression()))
            : Predicate(c.predicate());

    private static Expression Predicate(PredicateContext c)
    {
        var left = Additive(c.additiveExpression());
        return c.predicateSuffix() switch
        {
            null => left,
            QuantifiedComparisonContext q => new QuantifiedComparison(
                left,
                ComparisonOperator(q.comparisonOperator()),
                q.ANY() is not null ? Quantifier.Any : q.SOME() is not null ? Quantifier.Some : Quantifier.All,
                SubqueryOf(q.subquery())),
            ComparisonContext cmp => new BinaryExpression(ComparisonOperator(cmp.comparisonOperator()), left, Additive(cmp.additiveExpression())),
            IsNullContext isNull => new IsNullExpression(left, isNull.NOT() is not null),
            BetweenContext between => new BetweenExpression(
                left, between.NOT() is not null, Additive(between.additiveExpression(0)), Additive(between.additiveExpression(1))),
            InSubqueryContext inSubquery => new InSubqueryExpression(left, inSubquery.NOT() is not null, SubqueryOf(inSubquery.subquery())),
            InListContext inList => new InListExpression(left, inList.NOT() is not null, inList.expression().Select(Expr).ToArray()),
            LikeContext like => new LikeExpression(
                left,
                like.NOT() is not null,
                Additive(like.additiveExpression(0)),
                like.ESCAPE() is not null ? Additive(like.additiveExpression(1)) : null),
            var other => throw new InvalidOperationException(other.GetType().Name),
        };
    }

    private static BinaryOperator ComparisonOperator(ComparisonOperatorContext c) => c.Start.Type switch
    {
        EQ => BinaryOperator.Equal,
        NEQ => BinaryOperator.NotEqual,
        LT => BinaryOperator.LessThan,
        GT => BinaryOperator.GreaterThan,
        LTE => BinaryOperator.LessThanOrEqual,
        _ => BinaryOperator.GreaterThanOrEqual,
    };

    private static Expression Additive(AdditiveExpressionContext c)
    {
        var operands = c.multiplicativeExpression();
        var ops = c.additiveOperator();
        var result = Multiplicative(operands[0]);
        for (var i = 0; i < ops.Length; i++)
        {
            var op = ops[i].Start.Type switch
            {
                PLUS => BinaryOperator.Add,
                MINUS => BinaryOperator.Subtract,
                _ => BinaryOperator.Concat,
            };
            result = new BinaryExpression(op, result, Multiplicative(operands[i + 1]));
        }
        return result;
    }

    private static Expression Multiplicative(MultiplicativeExpressionContext c)
    {
        var operands = c.unaryExpression();
        var ops = c.multiplicativeOperator();
        var result = Unary(operands[0]);
        for (var i = 0; i < ops.Length; i++)
        {
            var op = ops[i].Start.Type == STAR ? BinaryOperator.Multiply : BinaryOperator.Divide;
            result = new BinaryExpression(op, result, Unary(operands[i + 1]));
        }
        return result;
    }

    private static Expression Unary(UnaryExpressionContext c) =>
        c.primary() is { } primary ? Primary(primary)
        : new UnaryExpression(c.MINUS() is not null ? UnaryOperator.Minus : UnaryOperator.Plus, Unary(c.unaryExpression()));

    private static Expression Primary(PrimaryContext c) => c switch
    {
        NumberLiteralContext n => new NumericLiteral(n.NUMBER().GetText()),
        StringLiteralContext s => new StringLiteral(Unquote(s.STRING(), '\'')),
        NullLiteralContext => NullLiteral.Instance,
        CaseExpressionContext ce => new CaseExpression(
            ce.caseOperand() is { } operand ? Expr(operand.expression()) : null,
            ce.whenClause().Select(w => new WhenClause(Expr(w.expression(0)), Expr(w.expression(1)))).ToArray(),
            ce.elseClause() is { } @else ? Expr(@else.expression()) : null),
        CastExpressionContext cast => new CastExpression(Expr(cast.expression()), DataTypeOf(cast.dataType())),
        ExistsExpressionContext exists => new ExistsExpression(SubqueryOf(exists.subquery())),
        FunctionCallContext f => FunctionCallOf(f),
        ColumnReferenceContext col => new ColumnReference(QualifiedName(col.qualifiedName())),
        ScalarSubqueryContext sq => new ScalarSubquery(SubqueryOf(sq.subquery())),
        ParenthesizedContext p => Expr(p.expression()),
        _ => throw new InvalidOperationException(c.GetType().Name),
    };

    private static Expression FunctionCallOf(FunctionCallContext c)
    {
        var name = string.Join(".", QualifiedName(c.qualifiedName()));
        var args = c.functionArguments();
        if (args is null) return new FunctionCall(name, SetQuantifier.None, NoExpressions, false);
        if (args.STAR() is not null) return new FunctionCall(name, SetQuantifier.None, NoExpressions, true);
        return new FunctionCall(name, QuantifierOf(args.setQuantifier()), args.expression().Select(Expr).ToArray(), false);
    }

    private static DataType DataTypeOf(DataTypeContext c) =>
        new(
            string.Join(" ", c.IDENTIFIER().Select(i => i.GetText().ToUpperInvariant())),
            c.NUMBER().Select(n => int.TryParse(n.GetText(), NumberStyles.None, CultureInfo.InvariantCulture, out var v)
                ? v
                : throw new SqlParseException($"Invalid type parameter '{n.GetText()}'")).ToArray());

    // ------------------------------------------------------------ names

    private static string[] QualifiedName(QualifiedNameContext c) => c.identifier().Select(Identifier).ToArray();

    private static string Identifier(IdentifierContext c) =>
        c.QUOTED_IDENTIFIER() is { } quoted ? Unquote(quoted, '"') : c.IDENTIFIER().GetText();

    private static string Unquote(ITerminalNode node, char quote)
    {
        var text = node.GetText();
        return text.Substring(1, text.Length - 2).Replace(new string(quote, 2), quote.ToString());
    }
}
