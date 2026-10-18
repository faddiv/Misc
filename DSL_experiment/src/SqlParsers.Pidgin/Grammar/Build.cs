using Pidgin;
using SqlParsers.Ast;

namespace SqlParsers.Pidgin;

/// <summary>Completes a predicate once its left operand is known (the result of <c>predicateSuffix</c>).</summary>
internal delegate Expression PredicateSuffix(Expression left);

/// <summary>A comparison suffix that still needs its operator.</summary>
internal delegate PredicateSuffix ComparisonRest(BinaryOperator op);

/// <summary>A BETWEEN, IN or LIKE suffix that still needs to know whether it was preceded by NOT.</summary>
internal delegate PredicateSuffix NegatableRest(bool negated);

internal readonly record struct FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

internal readonly record struct JoinPart(JoinType Type, bool Natural, TableReference Right, JoinCondition? Condition);

/// <summary>
/// Tree construction for <see cref="PidginGrammar"/>. Each method takes the values of one grammar rule's
/// elements, in order, and returns the AST node. Optional elements arrive as <see cref="Maybe{T}"/>.
/// </summary>
internal static class Build
{
    public static readonly IReadOnlyList<Expression> NoExpressions = Array.Empty<Expression>();
    public static readonly IReadOnlyList<SelectItem> SelectAll = [new AllColumns([])];
    public static readonly FunctionArguments NoArguments = new(SetQuantifier.None, NoExpressions, false);
    public static readonly FunctionArguments StarArgument = new(SetQuantifier.None, NoExpressions, true);
    public static readonly Expression Null = NullLiteral.Instance;

    // Pidgin returns repetitions as IEnumerable<T>. LINQ (Aggregate, ToArray) takes a fast path for the
    // underlying list; a plain foreach would box the enumerator and allocate for every repetition.

    /// <summary>Materializes a parsed list.</summary>
    public static IReadOnlyList<T> List<T>(IEnumerable<T> items) => items.ToArray();

    private static T? OrNull<T>(Maybe<T> value) where T : class => value.HasValue ? value.Value : null;

    // ------------------------------------------------------------ statement

    public static SelectStatement Statement(QueryExpression query, Maybe<IReadOnlyList<OrderByItem>> orderBy) =>
        new(query, orderBy.HasValue ? orderBy.Value : Array.Empty<OrderByItem>());

    public static OrderByItem OrderItem(Expression expression, Maybe<SortDirection> direction) =>
        new(expression, direction.HasValue ? direction.Value : SortDirection.Ascending);

    // ------------------------------------------------------------ query expressions

    public static QueryExpression UnionChain(
        QueryExpression first, IEnumerable<((SetOperator Operator, bool All) Operator, QueryExpression Right)> tail)
        => tail.Aggregate(first, static (left, x) => new SetOperation(left, x.Operator.Operator, x.Operator.All, x.Right));

    public static QueryExpression IntersectChain(QueryExpression first, IEnumerable<(bool All, QueryExpression Right)> tail)
        => tail.Aggregate(first, static (left, x) => new SetOperation(left, SetOperator.Intersect, x.All, x.Right));

    public static QueryExpression QuerySpecification(
        Maybe<SetQuantifier> quantifier,
        IReadOnlyList<SelectItem> selectList,
        IEnumerable<TableReference> from,
        Maybe<Expression> where,
        Maybe<IEnumerable<Expression>> groupBy,
        Maybe<Expression> having) =>
        new Ast.QuerySpecification(
            quantifier.HasValue ? quantifier.Value : SetQuantifier.None,
            selectList,
            from.ToArray(),
            OrNull(where),
            groupBy.HasValue ? groupBy.Value.ToArray() : NoExpressions,
            OrNull(having));

    public static SelectItem QualifiedStar(IEnumerable<string> qualifier) => new AllColumns(qualifier.ToArray());

    public static SelectItem ExpressionItem(Expression expression, Maybe<string> alias) =>
        new Ast.ExpressionItem(expression, OrNull(alias));

    // ------------------------------------------------------------ FROM clause

    public static TableReference Joins(TableReference first, IEnumerable<JoinPart> joins)
        => joins.Aggregate(first, static (left, j) => new JoinedTable(left, j.Type, j.Natural, j.Right, j.Condition));

    public static JoinPart CrossJoin(TableReference right) => new(JoinType.Cross, false, right, null);

    public static JoinPart NaturalJoin(Maybe<JoinType> type, TableReference right) =>
        new(type.HasValue ? type.Value : JoinType.Inner, true, right, null);

    public static JoinPart QualifiedJoin(Maybe<JoinType> type, TableReference right, JoinCondition condition) =>
        new(type.HasValue ? type.Value : JoinType.Inner, false, right, condition);

    public static JoinCondition On(Expression condition) => new OnCondition(condition);

    public static JoinCondition Using(IEnumerable<string> columns) => new UsingCondition(columns.ToArray());

    public static TableReference NamedTable(IReadOnlyList<string> name, Maybe<string> alias) => new Ast.NamedTable(name, OrNull(alias));

    public static TableReference DerivedTable(QueryExpression query, string alias) => new Ast.DerivedTable(query, alias);

    // ------------------------------------------------------------ expressions

    public static Expression LeftAssociative(Expression first, IEnumerable<(BinaryOperator Operator, Expression Right)> tail)
        => tail.Aggregate(first, static (left, x) => new BinaryExpression(x.Operator, left, x.Right));

    public static Expression Not(Expression operand) => new UnaryExpression(UnaryOperator.Not, operand);

    public static Expression Plus(Expression operand) => new UnaryExpression(UnaryOperator.Plus, operand);

    public static Expression Minus(Expression operand) => new UnaryExpression(UnaryOperator.Minus, operand);

    public static Expression Predicate(Expression left, Maybe<PredicateSuffix> suffix) =>
        suffix.HasValue ? suffix.Value(left) : left;

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

    public static NegatableRest InList(IEnumerable<Expression> items)
    {
        var values = items.ToArray();
        return negated => left => new InListExpression(left, negated, values);
    }

    public static NegatableRest Like(Expression pattern, Maybe<Expression> escape)
    {
        var escapeValue = OrNull(escape);
        return negated => left => new LikeExpression(left, negated, pattern, escapeValue);
    }

    // primary
    public static Expression Number(string text) => new NumericLiteral(text);

    public static Expression String(string value) => new StringLiteral(value);

    public static Expression Exists(QueryExpression query) => new ExistsExpression(query);

    public static Expression ScalarSubquery(QueryExpression query) => new Ast.ScalarSubquery(query);

    public static FunctionArguments OrNoArguments(Maybe<FunctionArguments> arguments) =>
        arguments.HasValue ? arguments.Value : NoArguments;

    public static FunctionArguments Arguments(Maybe<SetQuantifier> quantifier, IEnumerable<Expression> arguments) =>
        new(quantifier.HasValue ? quantifier.Value : SetQuantifier.None, arguments.ToArray(), false);

    public static Expression ColumnOrFunctionCall(IReadOnlyList<string> name, Maybe<FunctionArguments> arguments) =>
        arguments.HasValue
            ? new FunctionCall(string.Join(".", name), arguments.Value.Quantifier, arguments.Value.Arguments, arguments.Value.IsStar)
            : new ColumnReference(name);

    public static WhenClause When(Expression condition, Expression result) => new(condition, result);

    public static Expression Case(Maybe<Expression> operand, IEnumerable<WhenClause> whenClauses, Maybe<Expression> @else) =>
        new CaseExpression(OrNull(operand), whenClauses.ToArray(), OrNull(@else));

    public static Expression Cast(Expression operand, DataType type) => new CastExpression(operand, type);

    public static DataType DataType(IEnumerable<string> words, Maybe<IEnumerable<int>> parameters) =>
        new(string.Join(" ", words.Select(w => w.ToUpperInvariant())),
            parameters.HasValue ? parameters.Value.ToArray() : Array.Empty<int>());
}
