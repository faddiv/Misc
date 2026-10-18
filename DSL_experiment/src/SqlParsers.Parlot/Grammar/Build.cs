using SqlParsers.Ast;

namespace SqlParsers.Parlot;

/// <summary>Completes a predicate once its left operand is known (the result of <c>predicateSuffix</c>).</summary>
internal delegate Expression PredicateSuffix(Expression left);

/// <summary>A comparison suffix that still needs its operator.</summary>
internal delegate PredicateSuffix ComparisonRest(BinaryOperator op);

/// <summary>A BETWEEN, IN or LIKE suffix that still needs to know whether it was preceded by NOT.</summary>
internal delegate PredicateSuffix NegatableRest(bool negated);

internal sealed record FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

internal readonly record struct JoinPart(JoinType Type, bool Natural, TableReference Right, JoinCondition? Condition);

/// <summary>
/// Tree construction for <see cref="ParlotGrammar"/>. Each method takes the value of one grammar rule, a tuple
/// in the order of the rule's elements, and returns the AST node. Optional elements are null when absent.
/// </summary>
internal static class Build
{
    public static readonly IReadOnlyList<Expression> NoExpressions = [];
    public static readonly IReadOnlyList<SelectItem> SelectAll = [new AllColumns([])];
    public static readonly FunctionArguments NoArguments = new(SetQuantifier.None, NoExpressions, false);
    public static readonly FunctionArguments StarArgument = new(SetQuantifier.None, NoExpressions, true);
    public static readonly Expression Null = NullLiteral.Instance;

    // ------------------------------------------------------------ statement

    public static SelectStatement Statement((QueryExpression Query, IReadOnlyList<OrderByItem>? OrderBy) p) =>
        new(p.Query, p.OrderBy ?? Array.Empty<OrderByItem>());

    public static OrderByItem OrderItem((Expression Expression, SortDirection Direction) p) =>
        new(p.Expression, p.Direction);

    // ------------------------------------------------------------ query expressions

    public static QueryExpression UnionChain(
        (QueryExpression First, IReadOnlyList<((SetOperator Operator, bool All) Operator, QueryExpression Right)> Tail) p)
    {
        var result = p.First;
        foreach (var (op, right) in p.Tail)
            result = new SetOperation(result, op.Operator, op.All, right);
        return result;
    }

    public static QueryExpression IntersectChain((QueryExpression First, IReadOnlyList<(bool All, QueryExpression Right)> Tail) p)
    {
        var result = p.First;
        foreach (var (all, right) in p.Tail)
            result = new SetOperation(result, SetOperator.Intersect, all, right);
        return result;
    }

    public static QueryExpression QuerySpecification(
        (SetQuantifier Quantifier,
         IReadOnlyList<SelectItem> SelectList,
         IReadOnlyList<TableReference> From,
         Expression? Where,
         IReadOnlyList<Expression>? GroupBy,
         Expression? Having) p) =>
        new Ast.QuerySpecification(p.Quantifier, p.SelectList, p.From, p.Where, p.GroupBy ?? NoExpressions, p.Having);

    public static SelectItem QualifiedStar(IReadOnlyList<string> qualifier) => new AllColumns(qualifier);

    public static SelectItem ExpressionItem((Expression Expression, string? Alias) p) =>
        new Ast.ExpressionItem(p.Expression, p.Alias);

    // ------------------------------------------------------------ FROM clause

    public static TableReference Joins((TableReference First, IReadOnlyList<JoinPart> Joins) p)
    {
        var result = p.First;
        foreach (var join in p.Joins)
            result = new JoinedTable(result, join.Type, join.Natural, join.Right, join.Condition);
        return result;
    }

    public static JoinPart CrossJoin(TableReference right) => new(JoinType.Cross, false, right, null);

    public static JoinPart NaturalJoin((JoinType Type, TableReference Right) p) => new(p.Type, true, p.Right, null);

    public static JoinPart QualifiedJoin((JoinType Type, TableReference Right, JoinCondition Condition) p) =>
        new(p.Type, false, p.Right, p.Condition);

    public static JoinCondition On(Expression condition) => new OnCondition(condition);

    public static JoinCondition Using(IReadOnlyList<string> columns) => new UsingCondition(columns);

    public static TableReference NamedTable((IReadOnlyList<string> Name, string? Alias) p) => new Ast.NamedTable(p.Name, p.Alias);

    public static TableReference DerivedTable((QueryExpression Query, string Alias) p) => new Ast.DerivedTable(p.Query, p.Alias);

    // ------------------------------------------------------------ expressions

    public static Expression LeftAssociative((Expression First, IReadOnlyList<(BinaryOperator Operator, Expression Right)> Tail) p)
    {
        var result = p.First;
        foreach (var (op, right) in p.Tail)
            result = new BinaryExpression(op, result, right);
        return result;
    }

    public static Expression Not(Expression operand) => new UnaryExpression(UnaryOperator.Not, operand);

    public static Expression Unary((UnaryOperator Operator, Expression Operand) p) => new UnaryExpression(p.Operator, p.Operand);

    public static Expression Predicate((Expression Left, PredicateSuffix? Suffix) p) =>
        p.Suffix is null ? p.Left : p.Suffix(p.Left);

    // comparisonOperator ( ... )
    public static PredicateSuffix ComparisonSuffix((BinaryOperator Operator, ComparisonRest Completion) p) => p.Completion(p.Operator);

    public static ComparisonRest QuantifiedComparison((Quantifier Quantifier, QueryExpression Query) p) =>
        op => left => new Ast.QuantifiedComparison(left, op, p.Quantifier, p.Query);

    public static ComparisonRest Comparison(Expression right) =>
        op => left => new BinaryExpression(op, left, right);

    // IS NOT? NULL
    public static PredicateSuffix IsNull(bool negated) => left => new IsNullExpression(left, negated);

    // NOT? ( ... )
    public static PredicateSuffix Negatable((bool Negated, NegatableRest Completion) p) => p.Completion(p.Negated);

    public static NegatableRest Between((Expression Low, Expression High) p) =>
        negated => left => new BetweenExpression(left, negated, p.Low, p.High);

    public static NegatableRest InSubquery(QueryExpression query) =>
        negated => left => new InSubqueryExpression(left, negated, query);

    public static NegatableRest InList(IReadOnlyList<Expression> values) =>
        negated => left => new InListExpression(left, negated, values);

    public static NegatableRest Like((Expression Pattern, Expression? Escape) p) =>
        negated => left => new LikeExpression(left, negated, p.Pattern, p.Escape);

    // primary
    public static Expression Number(string text) => new NumericLiteral(text);

    public static Expression String(string value) => new StringLiteral(value);

    public static Expression Exists(QueryExpression query) => new ExistsExpression(query);

    public static Expression ScalarSubquery(QueryExpression query) => new Ast.ScalarSubquery(query);

    public static FunctionArguments OrNoArguments(FunctionArguments? arguments) => arguments ?? NoArguments;

    public static FunctionArguments Arguments((SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments) p) =>
        new(p.Quantifier, p.Arguments, false);

    public static Expression ColumnOrFunctionCall((IReadOnlyList<string> Name, FunctionArguments? Arguments) p) =>
        p.Arguments is { } a
            ? new FunctionCall(string.Join(".", p.Name), a.Quantifier, a.Arguments, a.IsStar)
            : new ColumnReference(p.Name);

    public static WhenClause When((Expression Condition, Expression Result) p) => new(p.Condition, p.Result);

    public static Expression Case((Expression? Operand, IReadOnlyList<WhenClause> WhenClauses, Expression? Else) p) =>
        new CaseExpression(p.Operand, p.WhenClauses, p.Else);

    public static Expression Cast((Expression Operand, DataType Type) p) => new CastExpression(p.Operand, p.Type);

    public static DataType DataType((IReadOnlyList<string> Words, IReadOnlyList<int>? Parameters) p) =>
        new(string.Join(" ", p.Words.Select(w => w.ToUpperInvariant())), p.Parameters ?? Array.Empty<int>());
}
