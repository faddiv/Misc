namespace SqlParsers.Ast;

// Shared syntax tree produced by every parser implementation.
// Lists are exposed as IReadOnlyList, so compare trees with SqlFormatter.Format, not record equality.

public sealed record SelectStatement(QueryExpression Query, IReadOnlyList<OrderByItem> OrderBy)
{
    public override string ToString() => SqlFormatter.Format(this);
}

public enum SortDirection { Ascending, Descending }

public sealed record OrderByItem(Expression Expression, SortDirection Direction);

// ---------------------------------------------------------------- queries

public abstract record QueryExpression
{
    public sealed override string ToString() => SqlFormatter.Format(this);
}

public enum SetQuantifier { None, Distinct, All }

public sealed record QuerySpecification(
    SetQuantifier Quantifier,
    IReadOnlyList<SelectItem> SelectList,
    IReadOnlyList<TableReference> From,
    Expression? Where,
    IReadOnlyList<Expression> GroupBy,
    Expression? Having) : QueryExpression;

public enum SetOperator { Union, Except, Intersect }

public sealed record SetOperation(QueryExpression Left, SetOperator Operator, bool All, QueryExpression Right) : QueryExpression;

// ---------------------------------------------------------------- select list

public abstract record SelectItem;

/// <summary><c>*</c> when <see cref="Qualifier"/> is empty, otherwise <c>qualifier.*</c>.</summary>
public sealed record AllColumns(IReadOnlyList<string> Qualifier) : SelectItem;

public sealed record ExpressionItem(Expression Expression, string? Alias) : SelectItem;

// ---------------------------------------------------------------- FROM clause

public abstract record TableReference
{
    public sealed override string ToString() => SqlFormatter.Format(this);
}

public sealed record NamedTable(IReadOnlyList<string> Name, string? Alias) : TableReference;

public sealed record DerivedTable(QueryExpression Query, string Alias) : TableReference;

public enum JoinType { Inner, Left, Right, Full, Cross }

public sealed record JoinedTable(TableReference Left, JoinType Type, bool Natural, TableReference Right, JoinCondition? Condition) : TableReference;

public abstract record JoinCondition;

public sealed record OnCondition(Expression Condition) : JoinCondition;

public sealed record UsingCondition(IReadOnlyList<string> Columns) : JoinCondition;

// ---------------------------------------------------------------- expressions

public abstract record Expression
{
    public sealed override string ToString() => SqlFormatter.Format(this);
}

public sealed record ColumnReference(IReadOnlyList<string> Parts) : Expression;

/// <summary>Numeric literal, kept as written (e.g. <c>1</c>, <c>2.50</c>, <c>.5</c>, <c>1E3</c>).</summary>
public sealed record NumericLiteral(string Text) : Expression;

/// <summary>Character string literal; <see cref="Value"/> is unescaped.</summary>
public sealed record StringLiteral(string Value) : Expression;

public sealed record NullLiteral : Expression
{
    public static readonly NullLiteral Instance = new();
}

public enum UnaryOperator { Plus, Minus, Not }

public sealed record UnaryExpression(UnaryOperator Operator, Expression Operand) : Expression;

public enum BinaryOperator
{
    Or, And,
    Equal, NotEqual, LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual,
    Add, Subtract, Concat, Multiply, Divide,
}

public sealed record BinaryExpression(BinaryOperator Operator, Expression Left, Expression Right) : Expression;

public enum Quantifier { Any, Some, All }

public sealed record QuantifiedComparison(Expression Left, BinaryOperator Operator, Quantifier Quantifier, QueryExpression Query) : Expression;

public sealed record IsNullExpression(Expression Operand, bool Negated) : Expression;

public sealed record BetweenExpression(Expression Operand, bool Negated, Expression Low, Expression High) : Expression;

public sealed record InListExpression(Expression Operand, bool Negated, IReadOnlyList<Expression> Values) : Expression;

public sealed record InSubqueryExpression(Expression Operand, bool Negated, QueryExpression Query) : Expression;

public sealed record LikeExpression(Expression Operand, bool Negated, Expression Pattern, Expression? Escape) : Expression;

public sealed record ExistsExpression(QueryExpression Query) : Expression;

public sealed record ScalarSubquery(QueryExpression Query) : Expression;

public sealed record FunctionCall(string Name, SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar) : Expression;

public sealed record WhenClause(Expression Condition, Expression Result);

/// <summary>Simple CASE when <see cref="Operand"/> is set, searched CASE otherwise.</summary>
public sealed record CaseExpression(Expression? Operand, IReadOnlyList<WhenClause> WhenClauses, Expression? Else) : Expression;

/// <summary>Type name words upper-cased and joined by a single space, e.g. <c>DOUBLE PRECISION</c>.</summary>
public sealed record DataType(string Name, IReadOnlyList<int> Parameters);

public sealed record CastExpression(Expression Operand, DataType Type) : Expression;
