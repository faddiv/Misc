using SqlParsers.Ast;

namespace SqlParsers.Irony;

internal readonly record struct FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

/// <summary>
/// Tree construction for <see cref="Sql92SelectGrammar"/>. The rules' <c>ToAst</c> functions pick the children out
/// of the parse tree; these methods build the AST nodes from them. Optional elements are null when absent.
/// </summary>
internal static class Build
{
    public static readonly IReadOnlyList<Expression> NoExpressions = Array.Empty<Expression>();
    public static readonly IReadOnlyList<SelectItem> SelectAll = [new AllColumns([])];
    public static readonly FunctionArguments NoArguments = new(SetQuantifier.None, NoExpressions, false);
    public static readonly FunctionArguments StarArgument = new(SetQuantifier.None, NoExpressions, true);
    public static readonly Expression Null = NullLiteral.Instance;

    // ------------------------------------------------------------ statement

    public static SelectStatement Statement(QueryExpression query, IReadOnlyList<OrderByItem>? orderBy) =>
        new(query, orderBy ?? Array.Empty<OrderByItem>());

    public static OrderByItem OrderItem(Expression expression, SortDirection direction) => new(expression, direction);

    // ------------------------------------------------------------ query expressions

    public static QueryExpression SetOperation(QueryExpression left, (SetOperator Operator, bool All) op, QueryExpression right) =>
        new Ast.SetOperation(left, op.Operator, op.All, right);

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

    public static TableReference Join(TableReference left, JoinType type, bool natural, TableReference right, JoinCondition? condition) =>
        new JoinedTable(left, type, natural, right, condition);

    public static JoinCondition On(Expression condition) => new OnCondition(condition);

    public static JoinCondition Using(string[] columns) => new UsingCondition(columns);

    public static TableReference NamedTable(string[] name, string? alias) => new Ast.NamedTable(name, alias);

    public static TableReference DerivedTable(QueryExpression query, string alias) => new Ast.DerivedTable(query, alias);

    // ------------------------------------------------------------ expressions

    public static Expression Binary(BinaryOperator op, Expression left, Expression right) => new BinaryExpression(op, left, right);

    public static Expression Not(Expression operand) => new UnaryExpression(UnaryOperator.Not, operand);

    public static Expression Unary(UnaryOperator op, Expression operand) => new UnaryExpression(op, operand);

    public static Expression QuantifiedComparison(Expression left, BinaryOperator op, Quantifier quantifier, QueryExpression query) =>
        new Ast.QuantifiedComparison(left, op, quantifier, query);

    public static Expression IsNull(Expression operand, bool negated) => new IsNullExpression(operand, negated);

    public static Expression Between(Expression operand, bool negated, Expression low, Expression high) =>
        new BetweenExpression(operand, negated, low, high);

    public static Expression InSubquery(Expression operand, bool negated, QueryExpression query) =>
        new InSubqueryExpression(operand, negated, query);

    public static Expression InList(Expression operand, bool negated, Expression[] values) =>
        new InListExpression(operand, negated, values);

    public static Expression Like(Expression operand, bool negated, Expression pattern, Expression? escape) =>
        new LikeExpression(operand, negated, pattern, escape);

    public static Expression Number(string text) => new NumericLiteral(text);

    public static Expression String(string value) => new Ast.StringLiteral(value);

    public static Expression Exists(QueryExpression query) => new ExistsExpression(query);

    public static Expression ScalarSubquery(QueryExpression query) => new Ast.ScalarSubquery(query);

    public static Expression ColumnReference(string[] name) => new Ast.ColumnReference(name);

    public static FunctionArguments Arguments(SetQuantifier quantifier, Expression[] arguments) => new(quantifier, arguments, false);

    public static Expression FunctionCall(string[] name, FunctionArguments arguments) =>
        new Ast.FunctionCall(string.Join(".", name), arguments.Quantifier, arguments.Arguments, arguments.IsStar);

    public static WhenClause When(Expression condition, Expression result) => new(condition, result);

    public static Expression Case(Expression? operand, WhenClause[] whenClauses, Expression? @else) =>
        new CaseExpression(operand, whenClauses, @else);

    public static Expression Cast(Expression operand, DataType type) => new CastExpression(operand, type);

    public static DataType DataType(string[] words, int[] parameters) =>
        new(string.Join(" ", words.Select(w => w.ToUpperInvariant())), parameters);
}
