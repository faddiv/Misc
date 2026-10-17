using System.Globalization;
using Irony.Parsing;
using SqlParsers.Ast;
using StringLiteral = SqlParsers.Ast.StringLiteral;

namespace SqlParsers.Irony;

/// <summary>
/// Converts Irony's parse tree into the shared AST. Child indexes below follow the rules in
/// <see cref="Sql92SelectGrammar"/> with punctuation (parentheses, commas and marked keywords) removed.
/// </summary>
internal static class IronyAstBuilder
{
    private static readonly Expression[] NoExpressions = [];

    public static SelectStatement Statement(ParseTreeNode n)
    {
        // statement: [queryExpr, orderByOpt, semiOpt]
        var orderByOpt = C(n, 1);
        var orderBy = orderByOpt.ChildNodes.Count == 0
            ? Array.Empty<OrderByItem>()
            : C(orderByOpt, 0).ChildNodes.Select(OrderItem).ToArray();
        return new SelectStatement(Query(C(n, 0)), orderBy);
    }

    private static OrderByItem OrderItem(ParseTreeNode n)
    {
        // orderItem: [expr, directionOpt]
        var direction = C(n, 1);
        var descending = direction.ChildNodes.Count > 0 && Is(C(direction, 0), "DESC");
        return new OrderByItem(Expr(C(n, 0)), descending ? SortDirection.Descending : SortDirection.Ascending);
    }

    // ------------------------------------------------------------ queries

    private static QueryExpression Query(ParseTreeNode n)
    {
        switch (n.Term.Name)
        {
            case "queryExpr":
            case "subqueryExpr":
                // [term] | [queryExpr, unionOp, queryTerm]; unionOp: [UNION|EXCEPT, allOpt]
                if (n.ChildNodes.Count == 1) return Query(C(n, 0));
                var op = C(n, 1);
                return new SetOperation(
                    Query(C(n, 0)),
                    Is(C(op, 0), "UNION") ? SetOperator.Union : SetOperator.Except,
                    C(op, 1).ChildNodes.Count > 0,
                    Query(C(n, 2)));

            case "queryTerm":
            case "subqueryTerm":
                // [primary] | [term, INTERSECT, allOpt, queryPrimary]
                if (n.ChildNodes.Count == 1) return Query(C(n, 0));
                return new SetOperation(Query(C(n, 0)), SetOperator.Intersect, C(n, 2).ChildNodes.Count > 0, Query(C(n, 3)));

            case "queryPrimary":
            case "subquery":
                return Query(C(n, 0));

            case "querySpec":
                return Specification(n);

            default:
                throw Unexpected(n);
        }
    }

    private static QueryExpression Specification(ParseTreeNode n)
    {
        // querySpec: [quantOpt, selectList, tableRefList, whereOpt, groupByOpt, havingOpt]
        var whereOpt = C(n, 3);
        var groupByOpt = C(n, 4);
        var havingOpt = C(n, 5);
        return new QuerySpecification(
            Quantifier(C(n, 0)),
            SelectList(C(n, 1)),
            C(n, 2).ChildNodes.Select(TableRef).ToArray(),
            whereOpt.ChildNodes.Count > 0 ? Expr(C(whereOpt, 0)) : null,
            groupByOpt.ChildNodes.Count > 0 ? C(groupByOpt, 0).ChildNodes.Select(Expr).ToArray() : NoExpressions,
            havingOpt.ChildNodes.Count > 0 ? Expr(C(havingOpt, 0)) : null);
    }

    private static SetQuantifier Quantifier(ParseTreeNode quantOpt) =>
        quantOpt.ChildNodes.Count == 0 ? SetQuantifier.None
        : Is(C(quantOpt, 0), "DISTINCT") ? SetQuantifier.Distinct
        : SetQuantifier.All;

    private static IReadOnlyList<SelectItem> SelectList(ParseTreeNode n)
    {
        // selectList: ["*"] | [selectItemList]
        var child = C(n, 0);
        if (Is(child, "*")) return new SelectItem[] { new AllColumns(Array.Empty<string>()) };
        return child.ChildNodes.Select(SelectItemOf).ToArray();
    }

    private static SelectItem SelectItemOf(ParseTreeNode n)
    {
        // selectItem: [expr, aliasOpt] | [qualifiedName, "*"]
        if (Is(C(n, 1), "*")) return new AllColumns(QualifiedName(C(n, 0)));
        return new ExpressionItem(Expr(C(n, 0)), Alias(C(n, 1)));
    }

    private static string? Alias(ParseTreeNode aliasOpt) =>
        aliasOpt.ChildNodes.Count == 0 ? null : Name(C(aliasOpt, 0));

    // ------------------------------------------------------------ FROM clause

    private static TableReference TableRef(ParseTreeNode n)
    {
        switch (n.Term.Name)
        {
            case "tableRef":
                return TableRef(C(n, 0));

            case "tablePrimary":
                // [qualifiedName, aliasOpt] | [queryExpr, derivedAlias]
                if (Is(C(n, 0), "qualifiedName"))
                    return new NamedTable(QualifiedName(C(n, 0)), Alias(C(n, 1)));
                return new DerivedTable(Query(C(n, 0)), Name(C(C(n, 1), 0)));

            case "joinedTable":
            {
                var left = TableRef(C(n, 0));
                var second = C(n, 1);
                if (Is(second, "CROSS"))       // [tableRef, CROSS, tablePrimary]
                    return new JoinedTable(left, JoinType.Cross, false, TableRef(C(n, 2)), null);
                if (Is(second, "NATURAL"))     // [tableRef, NATURAL, joinTypeOpt, tablePrimary]
                    return new JoinedTable(left, JoinTypeOf(C(n, 2)), true, TableRef(C(n, 3)), null);
                                               // [tableRef, joinTypeOpt, tablePrimary, joinSpec]
                return new JoinedTable(left, JoinTypeOf(second), false, TableRef(C(n, 2)), JoinConditionOf(C(n, 3)));
            }

            default:
                throw Unexpected(n);
        }
    }

    private static JoinType JoinTypeOf(ParseTreeNode joinTypeOpt)
    {
        if (joinTypeOpt.ChildNodes.Count == 0) return JoinType.Inner;
        var keyword = C(joinTypeOpt, 0);
        return Is(keyword, "INNER") ? JoinType.Inner
            : Is(keyword, "LEFT") ? JoinType.Left
            : Is(keyword, "RIGHT") ? JoinType.Right
            : JoinType.Full;
    }

    private static JoinCondition JoinConditionOf(ParseTreeNode joinSpec)
    {
        // joinSpec: [expr] | [nameList]
        var child = C(joinSpec, 0);
        return Is(child, "nameList")
            ? new UsingCondition(child.ChildNodes.Select(Name).ToArray())
            : new OnCondition(Expr(child));
    }

    // ------------------------------------------------------------ expressions

    private static Expression Expr(ParseTreeNode n)
    {
        switch (n.Term.Name)
        {
            case "expr":
            case "caseOperandOpt":
                return Expr(C(n, 0));

            case "orExpr":
                return n.ChildNodes.Count == 1 ? Expr(C(n, 0)) : new BinaryExpression(BinaryOperator.Or, Expr(C(n, 0)), Expr(C(n, 1)));

            case "andExpr":
                return n.ChildNodes.Count == 1 ? Expr(C(n, 0)) : new BinaryExpression(BinaryOperator.And, Expr(C(n, 0)), Expr(C(n, 1)));

            case "notExpr":
                // [NOT, notExpr] | [predicate]
                return n.ChildNodes.Count == 2 ? new UnaryExpression(UnaryOperator.Not, Expr(C(n, 1))) : Expr(C(n, 0));

            case "predicate":
            {
                // [addExpr, predicateSuffixOpt]
                var left = Expr(C(n, 0));
                var suffixOpt = C(n, 1);
                return suffixOpt.ChildNodes.Count == 0 ? left : Suffix(left, C(suffixOpt, 0));
            }

            case "addExpr":
            case "mulExpr":
                // [operand] | [left, op, right]
                return n.ChildNodes.Count == 1
                    ? Expr(C(n, 0))
                    : new BinaryExpression(Operator(C(C(n, 1), 0)), Expr(C(n, 0)), Expr(C(n, 2)));

            case "unaryExpr":
                // [primary] | [unaryOp, unaryExpr]
                if (n.ChildNodes.Count == 1) return Expr(C(n, 0));
                return new UnaryExpression(Is(C(C(n, 0), 0), "-") ? UnaryOperator.Minus : UnaryOperator.Plus, Expr(C(n, 1)));

            case "primary":
            {
                var child = C(n, 0);
                return child.Term.Name switch
                {
                    "number" => new NumericLiteral(child.Token.Text),
                    "string" => new StringLiteral((string)child.Token.Value),
                    "subquery" => new ScalarSubquery(Query(child)),
                    _ when Is(child, "NULL") => NullLiteral.Instance,
                    _ => Expr(child),   // caseExpr, castExpr, existsExpr, functionCall, columnRef, parenthesized expr
                };
            }

            case "columnRef":
                return new ColumnReference(QualifiedName(C(n, 0)));

            case "functionCall":
            {
                // [qualifiedName, funcArgs]; funcArgs: [] | ["*"] | [quantOpt, exprList]
                var name = string.Join(".", QualifiedName(C(n, 0)));
                var args = C(n, 1);
                if (args.ChildNodes.Count == 0) return new FunctionCall(name, SetQuantifier.None, NoExpressions, false);
                if (Is(C(args, 0), "*")) return new FunctionCall(name, SetQuantifier.None, NoExpressions, true);
                return new FunctionCall(name, Quantifier(C(args, 0)), C(args, 1).ChildNodes.Select(Expr).ToArray(), false);
            }

            case "caseExpr":
            {
                // [caseOperandOpt, whenList, elseOpt]; whenClause: [expr, expr]
                var operandOpt = C(n, 0);
                var elseOpt = C(n, 2);
                return new CaseExpression(
                    operandOpt.ChildNodes.Count > 0 ? Expr(operandOpt) : null,
                    C(n, 1).ChildNodes.Select(w => new WhenClause(Expr(C(w, 0)), Expr(C(w, 1)))).ToArray(),
                    elseOpt.ChildNodes.Count > 0 ? Expr(C(elseOpt, 0)) : null);
            }

            case "castExpr":
                // [expr, dataType]
                return new CastExpression(Expr(C(n, 0)), DataTypeOf(C(n, 1)));

            case "existsExpr":
                return new ExistsExpression(Query(C(n, 0)));

            default:
                throw Unexpected(n);
        }
    }

    private static Expression Suffix(Expression left, ParseTreeNode s)
    {
        switch (s.Term.Name)
        {
            case "comparisonSuffix":   // [compOp, addExpr]
                return new BinaryExpression(Operator(C(C(s, 0), 0)), left, Expr(C(s, 1)));

            case "quantifiedSuffix":   // [compOp, quantifier, subquery]
            {
                var q = C(C(s, 1), 0);
                return new QuantifiedComparison(
                    left,
                    Operator(C(C(s, 0), 0)),
                    Is(q, "ANY") ? Ast.Quantifier.Any : Is(q, "SOME") ? Ast.Quantifier.Some : Ast.Quantifier.All,
                    Query(C(s, 2)));
            }

            case "isNullSuffix":       // [notOpt, NULL]
                return new IsNullExpression(left, Negated(C(s, 0)));

            case "betweenSuffix":      // [notOpt, addExpr, addExpr]
                return new BetweenExpression(left, Negated(C(s, 0)), Expr(C(s, 1)), Expr(C(s, 2)));

            case "inSubquerySuffix":   // [notOpt, subquery]
                return new InSubqueryExpression(left, Negated(C(s, 0)), Query(C(s, 1)));

            case "inListSuffix":       // [notOpt, exprList]
                return new InListExpression(left, Negated(C(s, 0)), C(s, 1).ChildNodes.Select(Expr).ToArray());

            case "likeSuffix":         // [notOpt, addExpr, escapeOpt]
            {
                var escapeOpt = C(s, 2);
                return new LikeExpression(
                    left, Negated(C(s, 0)), Expr(C(s, 1)),
                    escapeOpt.ChildNodes.Count > 0 ? Expr(C(escapeOpt, 0)) : null);
            }

            default:
                throw Unexpected(s);
        }
    }

    private static bool Negated(ParseTreeNode notOpt) => notOpt.ChildNodes.Count > 0;

    private static BinaryOperator Operator(ParseTreeNode token) => token.Term.Name switch
    {
        "=" => BinaryOperator.Equal,
        "<>" => BinaryOperator.NotEqual,
        "<" => BinaryOperator.LessThan,
        ">" => BinaryOperator.GreaterThan,
        "<=" => BinaryOperator.LessThanOrEqual,
        ">=" => BinaryOperator.GreaterThanOrEqual,
        "+" => BinaryOperator.Add,
        "-" => BinaryOperator.Subtract,
        "||" => BinaryOperator.Concat,
        "*" => BinaryOperator.Multiply,
        "/" => BinaryOperator.Divide,
        _ => throw Unexpected(token),
    };

    private static DataType DataTypeOf(ParseTreeNode n)
    {
        // dataType: [typeWords, typeParamsOpt]; typeParamsOpt: [] | [intList]
        var words = C(n, 0).ChildNodes.Select(w => w.Token.Text.ToUpperInvariant());
        var paramsOpt = C(n, 1);
        var parameters = paramsOpt.ChildNodes.Count == 0
            ? Array.Empty<int>()
            : C(paramsOpt, 0).ChildNodes.Select(p => int.TryParse(p.Token.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var v)
                ? v
                : throw new SqlParseException($"Invalid type parameter '{p.Token.Text}'")).ToArray();
        return new DataType(string.Join(" ", words), parameters);
    }

    // ------------------------------------------------------------ names

    private static string[] QualifiedName(ParseTreeNode n) => n.ChildNodes.Select(Name).ToArray();

    private static string Name(ParseTreeNode n)
    {
        // name: [id] | [quotedId]
        var token = C(n, 0);
        return token.Term.Name == "quotedId" ? (string)token.Token.Value : token.Token.Text;
    }

    // ------------------------------------------------------------ helpers

    private static ParseTreeNode C(ParseTreeNode n, int index) => n.ChildNodes[index];

    private static bool Is(ParseTreeNode n, string termName) =>
        string.Equals(n.Term.Name, termName, StringComparison.OrdinalIgnoreCase);

    private static InvalidOperationException Unexpected(ParseTreeNode n) =>
        new($"Unexpected parse tree node '{n.Term.Name}' with {n.ChildNodes.Count} children.");
}
