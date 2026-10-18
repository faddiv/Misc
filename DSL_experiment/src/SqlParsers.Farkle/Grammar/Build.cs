using System.Globalization;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

internal readonly record struct FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

/// <summary>
/// Semantic actions for <see cref="FarkleGrammar"/>. Each method takes the values of one production's elements, in
/// order, and returns the AST node. Optional elements are null (or a default) when absent.
/// </summary>
internal static class Build
{
    public static readonly IReadOnlyList<Expression> NoExpressions = Array.Empty<Expression>();
    public static readonly IReadOnlyList<int> NoTypeParameters = Array.Empty<int>();
    public static readonly IReadOnlyList<SelectItem> SelectAll = [new AllColumns([])];
    public static readonly FunctionArguments NoArguments = new(SetQuantifier.None, NoExpressions, false);
    public static readonly FunctionArguments StarArgument = new(SetQuantifier.None, NoExpressions, true);
    public static readonly Expression Null = NullLiteral.Instance;

    // ------------------------------------------------------------ helpers

    public static List<T> StartList<T>(T item) => new(4) { item };

    public static List<T> AddToList<T>(List<T> list, T item)
    {
        list.Add(item);
        return list;
    }

    /// <summary>The value of an optional element that is present.</summary>
    public static T? Some<T>(T value) where T : class => value;

    // ------------------------------------------------------------ statement

    public static SelectStatement Statement(QueryExpression query, List<OrderByItem>? orderBy) =>
        new(query, orderBy ?? (IReadOnlyList<OrderByItem>)Array.Empty<OrderByItem>());

    public static OrderByItem Ascending(Expression expression) => new(expression, SortDirection.Ascending);

    public static OrderByItem Descending(Expression expression) => new(expression, SortDirection.Descending);

    // ------------------------------------------------------------ query expressions

    public static QueryExpression SetOperation(QueryExpression left, (SetOperator Operator, bool All) op, QueryExpression right) =>
        new Ast.SetOperation(left, op.Operator, op.All, right);

    public static QueryExpression Intersect(QueryExpression left, bool all, QueryExpression right) =>
        new Ast.SetOperation(left, SetOperator.Intersect, all, right);

    public static (SetOperator Operator, bool All) Union(bool all) => (SetOperator.Union, all);

    public static (SetOperator Operator, bool All) Except(bool all) => (SetOperator.Except, all);

    public static QueryExpression QuerySpecification(
        SetQuantifier quantifier,
        IReadOnlyList<SelectItem> selectList,
        List<TableReference> from,
        Expression? where,
        IReadOnlyList<Expression> groupBy,
        Expression? having) =>
        new Ast.QuerySpecification(quantifier, selectList, from, where, groupBy, having);

    public static IReadOnlyList<SelectItem> SelectItems(List<SelectItem> items) => items;

    public static IReadOnlyList<Expression> Expressions(List<Expression> items) => items;

    public static SelectItem QualifiedStar(List<string> qualifier) => new AllColumns(qualifier);

    public static SelectItem ExpressionItem(Expression expression, string? alias) => new Ast.ExpressionItem(expression, alias);

    // ------------------------------------------------------------ FROM clause

    public static TableReference CrossJoin(TableReference left, TableReference right) =>
        new JoinedTable(left, JoinType.Cross, false, right, null);

    public static TableReference NaturalJoin(TableReference left, JoinType type, TableReference right) =>
        new JoinedTable(left, type, true, right, null);

    public static TableReference QualifiedJoin(TableReference left, JoinType type, TableReference right, JoinCondition condition) =>
        new JoinedTable(left, type, false, right, condition);

    public static JoinCondition On(Expression condition) => new OnCondition(condition);

    public static JoinCondition Using(List<string> columns) => new UsingCondition(columns);

    public static TableReference NamedTable(List<string> name, string? alias) => new Ast.NamedTable(name, alias);

    public static TableReference DerivedTable(QueryExpression query, string alias) => new Ast.DerivedTable(query, alias);

    // ------------------------------------------------------------ expressions

    public static Expression Or(Expression left, Expression right) => new BinaryExpression(BinaryOperator.Or, left, right);

    public static Expression And(Expression left, Expression right) => new BinaryExpression(BinaryOperator.And, left, right);

    public static Expression Not(Expression operand) => new UnaryExpression(UnaryOperator.Not, operand);

    public static Expression Binary(Expression left, BinaryOperator op, Expression right) => new BinaryExpression(op, left, right);

    public static Expression Comparison(Expression left, BinaryOperator op, Expression right) => new BinaryExpression(op, left, right);

    public static Expression QuantifiedComparison(Expression left, BinaryOperator op, Quantifier quantifier, QueryExpression query) =>
        new Ast.QuantifiedComparison(left, op, quantifier, query);

    public static Expression IsNull(Expression operand, bool negated) => new IsNullExpression(operand, negated);

    public static Expression Between(Expression operand, bool negated, Expression low, Expression high) =>
        new BetweenExpression(operand, negated, low, high);

    public static Expression InSubquery(Expression operand, bool negated, QueryExpression query) =>
        new InSubqueryExpression(operand, negated, query);

    public static Expression InList(Expression operand, bool negated, List<Expression> values) =>
        new InListExpression(operand, negated, values);

    public static Expression Like(Expression operand, bool negated, Expression pattern, Expression? escape) =>
        new LikeExpression(operand, negated, pattern, escape);

    public static Expression Plus(Expression operand) => new UnaryExpression(UnaryOperator.Plus, operand);

    public static Expression Minus(Expression operand) => new UnaryExpression(UnaryOperator.Minus, operand);

    public static Expression Number(string text) => new NumericLiteral(text);

    public static Expression String(string value) => new StringLiteral(value);

    public static Expression Exists(QueryExpression query) => new ExistsExpression(query);

    public static Expression ScalarSubquery(QueryExpression query) => new Ast.ScalarSubquery(query);

    public static Expression ColumnReference(List<string> name) => new Ast.ColumnReference(name);

    public static Expression FunctionCall(List<string> name, FunctionArguments arguments) =>
        new Ast.FunctionCall(string.Join(".", name), arguments.Quantifier, arguments.Arguments, arguments.IsStar);

    public static FunctionArguments Arguments(SetQuantifier quantifier, List<Expression> arguments) => new(quantifier, arguments, false);

    public static Expression Case(Expression? operand, List<WhenClause> whenClauses, Expression? @else) =>
        new CaseExpression(operand, whenClauses, @else);

    public static WhenClause When(Expression condition, Expression result) => new(condition, result);

    public static Expression Cast(Expression operand, DataType type) => new CastExpression(operand, type);

    public static DataType DataType(List<string> words, IReadOnlyList<int> parameters) =>
        new(string.Join(" ", words.Select(w => w.ToUpperInvariant())), parameters);

    public static IReadOnlyList<int> TypeParameters(List<int> parameters) => parameters;

    public static int Integer(string text) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new SqlParseException($"Invalid type parameter '{text}'");
}
