using SqlParsers.Ast;

namespace SqlParsers.Superpower;

/// <summary>Completes a predicate once its left operand is known (the result of <c>predicateSuffix</c>).</summary>
internal delegate Expression PredicateSuffix(Expression left);

/// <summary>A comparison suffix that still needs its operator.</summary>
internal delegate PredicateSuffix ComparisonRest(BinaryOperator op);

/// <summary>A BETWEEN, IN or LIKE suffix that still needs to know whether it was preceded by NOT.</summary>
internal delegate PredicateSuffix NegatableRest(bool negated);

internal readonly record struct FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

internal readonly record struct JoinPart(JoinType Type, bool Natural, TableReference Right, JoinCondition? Condition);

/// <summary>
/// Tree construction for <see cref="SuperpowerGrammar"/>. Each method takes the values of one grammar rule's
/// elements, in order, and returns the AST node. Optional elements are null (or a default) when absent.
/// </summary>
internal static class Build
{
    public static readonly IReadOnlyList<Expression> NoExpressions = Array.Empty<Expression>();
    public static readonly IReadOnlyList<SelectItem> SelectAll = [new AllColumns([])];
    public static readonly FunctionArguments NoArguments = new(SetQuantifier.None, NoExpressions, false);
    public static readonly FunctionArguments StarArgument = new(SetQuantifier.None, NoExpressions, true);
    public static readonly Expression Null = NullLiteral.Instance;

    /// <summary>Superpower returns repetitions as arrays, which already are read-only lists.</summary>
    public static IReadOnlyList<T> List<T>(T[] items) => items;

    // ------------------------------------------------------------ statement

    public static SelectStatement Statement(QueryExpression query, IReadOnlyList<OrderByItem>? orderBy) =>
        new(query, orderBy ?? Array.Empty<OrderByItem>());

    public static OrderByItem OrderItem(Expression expression, SortDirection direction) => new(expression, direction);

    // ------------------------------------------------------------ query expressions

    public static QueryExpression UnionChain(
        QueryExpression first, ((SetOperator Operator, bool All) Operator, QueryExpression Right)[] tail)
    {
        var result = first;
        foreach (var (op, right) in tail)
            result = new SetOperation(result, op.Operator, op.All, right);
        return result;
    }

    public static QueryExpression IntersectChain(QueryExpression first, (bool All, QueryExpression Right)[] tail)
    {
        var result = first;
        foreach (var (all, right) in tail)
            result = new SetOperation(result, SetOperator.Intersect, all, right);
        return result;
    }

    public static QueryExpression QuerySpecification(
        SetQuantifier quantifier,
        IReadOnlyList<SelectItem> selectList,
        TableReference[] from,
        Expression? where,
        Expression[]? groupBy,
        Expression? having) =>
        new Ast.QuerySpecification(quantifier, selectList, from, where, groupBy ?? NoExpressions, having);

    public static SelectItem QualifiedStar(string[] qualifier) => new AllColumns(qualifier);

    public static SelectItem ExpressionItem(Expression expression, string? alias) => new Ast.ExpressionItem(expression, alias);

    // ------------------------------------------------------------ FROM clause

    public static TableReference Joins(TableReference first, JoinPart[] joins)
    {
        var result = first;
        foreach (var join in joins)
            result = new JoinedTable(result, join.Type, join.Natural, join.Right, join.Condition);
        return result;
    }

    public static JoinPart CrossJoin(TableReference right) => new(JoinType.Cross, false, right, null);

    public static JoinPart NaturalJoin(JoinType type, TableReference right) => new(type, true, right, null);

    public static JoinPart QualifiedJoin(JoinType type, TableReference right, JoinCondition condition) => new(type, false, right, condition);

    public static JoinCondition On(Expression condition) => new OnCondition(condition);

    public static JoinCondition Using(string[] columns) => new UsingCondition(columns);

    public static TableReference NamedTable(IReadOnlyList<string> name, string? alias) => new Ast.NamedTable(name, alias);

    public static TableReference DerivedTable(QueryExpression query, string alias) => new Ast.DerivedTable(query, alias);

    // ------------------------------------------------------------ expressions

    public static Expression Binary(BinaryOperator op, Expression left, Expression right) => new BinaryExpression(op, left, right);

    public static Expression Not(Expression operand) => new UnaryExpression(UnaryOperator.Not, operand);

    public static Expression Plus(Expression operand) => new UnaryExpression(UnaryOperator.Plus, operand);

    public static Expression Minus(Expression operand) => new UnaryExpression(UnaryOperator.Minus, operand);

    public static Expression Predicate(Expression left, PredicateSuffix? suffix) => suffix is null ? left : suffix(left);

    // comparisonOperator ( ... )
    public static PredicateSuffix ComparisonSuffix(BinaryOperator op, ComparisonRest completion) => completion(op);

    public static ComparisonRest QuantifiedComparison(Quantifier quantifier, QueryExpression query) =>
        op => left => new Ast.QuantifiedComparison(left, op, quantifier, query);

    public static ComparisonRest Comparison(Expression right) =>
        op => left => new BinaryExpression(op, left, right);

    // IS NOT? NULL
    public static PredicateSuffix IsNull(bool negated) => left => new IsNullExpression(left, negated);

    // NOT? ( ... )
    public static PredicateSuffix Negatable(bool negated, NegatableRest completion) => completion(negated);

    public static NegatableRest Between(Expression low, Expression high) =>
        negated => left => new BetweenExpression(left, negated, low, high);

    public static NegatableRest InSubquery(QueryExpression query) =>
        negated => left => new InSubqueryExpression(left, negated, query);

    public static NegatableRest InList(Expression[] values) =>
        negated => left => new InListExpression(left, negated, values);

    public static NegatableRest Like(Expression pattern, Expression? escape) =>
        negated => left => new LikeExpression(left, negated, pattern, escape);

    // primary
    public static Expression Number(string text) => new NumericLiteral(text);

    public static Expression String(string value) => new StringLiteral(value);

    public static Expression Exists(QueryExpression query) => new ExistsExpression(query);

    public static Expression ScalarSubquery(QueryExpression query) => new Ast.ScalarSubquery(query);

    public static FunctionArguments OrNoArguments(FunctionArguments? arguments) => arguments ?? NoArguments;

    public static FunctionArguments Arguments(SetQuantifier quantifier, Expression[] arguments) => new(quantifier, arguments, false);

    public static Expression ColumnOrFunctionCall(IReadOnlyList<string> name, FunctionArguments? arguments) =>
        arguments is { } a
            ? new FunctionCall(string.Join(".", name), a.Quantifier, a.Arguments, a.IsStar)
            : new ColumnReference(name);

    public static WhenClause When(Expression condition, Expression result) => new(condition, result);

    public static Expression Case(Expression? operand, WhenClause[] whenClauses, Expression? @else) =>
        new CaseExpression(operand, whenClauses, @else);

    public static Expression Cast(Expression operand, DataType type) => new CastExpression(operand, type);

    public static DataType DataType(string[] words, int[] parameters) =>
        new(string.Join(" ", words.Select(w => w.ToUpperInvariant())), parameters);
}
