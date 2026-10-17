using System.Text;
using SqlParsers.Ast;

namespace SqlParsers;

/// <summary>
/// Writes a canonical, fully parenthesized text form of the tree.
/// Two trees are considered equal when their canonical forms are equal.
/// </summary>
public static class SqlFormatter
{
    public static string Format(SelectStatement statement) => Run(sb => Write(sb, statement));
    public static string Format(QueryExpression query) => Run(sb => Write(sb, query));
    public static string Format(TableReference table) => Run(sb => Write(sb, table));
    public static string Format(Expression expression) => Run(sb => Write(sb, expression));

    private static string Run(Action<StringBuilder> write)
    {
        var sb = new StringBuilder();
        write(sb);
        return sb.ToString();
    }

    private static void Write(StringBuilder sb, SelectStatement s)
    {
        Write(sb, s.Query);
        if (s.OrderBy.Count == 0) return;
        sb.Append(" ORDER BY ");
        for (var i = 0; i < s.OrderBy.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            Write(sb, s.OrderBy[i].Expression);
            sb.Append(s.OrderBy[i].Direction == SortDirection.Descending ? " DESC" : " ASC");
        }
    }

    private static void Write(StringBuilder sb, QueryExpression q)
    {
        switch (q)
        {
            case QuerySpecification spec:
                sb.Append("SELECT ");
                if (spec.Quantifier == SetQuantifier.Distinct) sb.Append("DISTINCT ");
                else if (spec.Quantifier == SetQuantifier.All) sb.Append("ALL ");
                for (var i = 0; i < spec.SelectList.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    Write(sb, spec.SelectList[i]);
                }
                sb.Append(" FROM ");
                for (var i = 0; i < spec.From.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    Write(sb, spec.From[i]);
                }
                if (spec.Where is not null)
                {
                    sb.Append(" WHERE ");
                    Write(sb, spec.Where);
                }
                if (spec.GroupBy.Count > 0)
                {
                    sb.Append(" GROUP BY ");
                    WriteList(sb, spec.GroupBy);
                }
                if (spec.Having is not null)
                {
                    sb.Append(" HAVING ");
                    Write(sb, spec.Having);
                }
                break;

            case SetOperation op:
                sb.Append('(');
                Write(sb, op.Left);
                sb.Append(op.Operator switch
                {
                    SetOperator.Union => " UNION ",
                    SetOperator.Except => " EXCEPT ",
                    _ => " INTERSECT ",
                });
                if (op.All) sb.Append("ALL ");
                Write(sb, op.Right);
                sb.Append(')');
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(q), q.GetType().Name, null);
        }
    }

    private static void Write(StringBuilder sb, SelectItem item)
    {
        switch (item)
        {
            case AllColumns all:
                foreach (var part in all.Qualifier)
                {
                    WriteIdentifier(sb, part);
                    sb.Append('.');
                }
                sb.Append('*');
                break;
            case ExpressionItem e:
                Write(sb, e.Expression);
                if (e.Alias is not null)
                {
                    sb.Append(" AS ");
                    WriteIdentifier(sb, e.Alias);
                }
                break;
        }
    }

    private static void Write(StringBuilder sb, TableReference t)
    {
        switch (t)
        {
            case NamedTable n:
                WriteName(sb, n.Name);
                if (n.Alias is not null)
                {
                    sb.Append(" AS ");
                    WriteIdentifier(sb, n.Alias);
                }
                break;

            case DerivedTable d:
                sb.Append('(');
                Write(sb, d.Query);
                sb.Append(") AS ");
                WriteIdentifier(sb, d.Alias);
                break;

            case JoinedTable j:
                sb.Append('(');
                Write(sb, j.Left);
                sb.Append(' ');
                if (j.Natural) sb.Append("NATURAL ");
                sb.Append(j.Type switch
                {
                    JoinType.Cross => "CROSS",
                    JoinType.Inner => "INNER",
                    JoinType.Left => "LEFT OUTER",
                    JoinType.Right => "RIGHT OUTER",
                    _ => "FULL OUTER",
                });
                sb.Append(" JOIN ");
                Write(sb, j.Right);
                switch (j.Condition)
                {
                    case OnCondition on:
                        sb.Append(" ON ");
                        Write(sb, on.Condition);
                        break;
                    case UsingCondition u:
                        sb.Append(" USING (");
                        for (var i = 0; i < u.Columns.Count; i++)
                        {
                            if (i > 0) sb.Append(", ");
                            WriteIdentifier(sb, u.Columns[i]);
                        }
                        sb.Append(')');
                        break;
                }
                sb.Append(')');
                break;
        }
    }

    private static void Write(StringBuilder sb, Expression e)
    {
        switch (e)
        {
            case ColumnReference c:
                WriteName(sb, c.Parts);
                break;
            case NumericLiteral n:
                sb.Append(n.Text);
                break;
            case StringLiteral s:
                sb.Append('\'').Append(s.Value.Replace("'", "''")).Append('\'');
                break;
            case NullLiteral:
                sb.Append("NULL");
                break;
            case UnaryExpression u:
                sb.Append(u.Operator switch
                {
                    UnaryOperator.Minus => "(-",
                    UnaryOperator.Plus => "(+",
                    _ => "(NOT ",
                });
                Write(sb, u.Operand);
                sb.Append(')');
                break;
            case BinaryExpression b:
                sb.Append('(');
                Write(sb, b.Left);
                sb.Append(' ').Append(OperatorText(b.Operator)).Append(' ');
                Write(sb, b.Right);
                sb.Append(')');
                break;
            case QuantifiedComparison q:
                sb.Append('(');
                Write(sb, q.Left);
                sb.Append(' ').Append(OperatorText(q.Operator)).Append(' ');
                sb.Append(q.Quantifier switch
                {
                    Quantifier.Any => "ANY",
                    Quantifier.Some => "SOME",
                    _ => "ALL",
                });
                sb.Append(" (");
                Write(sb, q.Query);
                sb.Append("))");
                break;
            case IsNullExpression i:
                sb.Append('(');
                Write(sb, i.Operand);
                sb.Append(i.Negated ? " IS NOT NULL)" : " IS NULL)");
                break;
            case BetweenExpression b:
                sb.Append('(');
                Write(sb, b.Operand);
                sb.Append(b.Negated ? " NOT BETWEEN " : " BETWEEN ");
                Write(sb, b.Low);
                sb.Append(" AND ");
                Write(sb, b.High);
                sb.Append(')');
                break;
            case InListExpression i:
                sb.Append('(');
                Write(sb, i.Operand);
                sb.Append(i.Negated ? " NOT IN (" : " IN (");
                WriteList(sb, i.Values);
                sb.Append("))");
                break;
            case InSubqueryExpression i:
                sb.Append('(');
                Write(sb, i.Operand);
                sb.Append(i.Negated ? " NOT IN (" : " IN (");
                Write(sb, i.Query);
                sb.Append("))");
                break;
            case LikeExpression l:
                sb.Append('(');
                Write(sb, l.Operand);
                sb.Append(l.Negated ? " NOT LIKE " : " LIKE ");
                Write(sb, l.Pattern);
                if (l.Escape is not null)
                {
                    sb.Append(" ESCAPE ");
                    Write(sb, l.Escape);
                }
                sb.Append(')');
                break;
            case ExistsExpression x:
                sb.Append("EXISTS (");
                Write(sb, x.Query);
                sb.Append(')');
                break;
            case ScalarSubquery s:
                sb.Append('(');
                Write(sb, s.Query);
                sb.Append(')');
                break;
            case FunctionCall f:
                sb.Append(f.Name).Append('(');
                if (f.IsStar)
                {
                    sb.Append('*');
                }
                else
                {
                    if (f.Quantifier == SetQuantifier.Distinct) sb.Append("DISTINCT ");
                    else if (f.Quantifier == SetQuantifier.All) sb.Append("ALL ");
                    WriteList(sb, f.Arguments);
                }
                sb.Append(')');
                break;
            case CaseExpression c:
                sb.Append("CASE");
                if (c.Operand is not null)
                {
                    sb.Append(' ');
                    Write(sb, c.Operand);
                }
                foreach (var w in c.WhenClauses)
                {
                    sb.Append(" WHEN ");
                    Write(sb, w.Condition);
                    sb.Append(" THEN ");
                    Write(sb, w.Result);
                }
                if (c.Else is not null)
                {
                    sb.Append(" ELSE ");
                    Write(sb, c.Else);
                }
                sb.Append(" END");
                break;
            case CastExpression c:
                sb.Append("CAST(");
                Write(sb, c.Operand);
                sb.Append(" AS ").Append(c.Type.Name);
                if (c.Type.Parameters.Count > 0)
                    sb.Append('(').AppendJoin(',', c.Type.Parameters).Append(')');
                sb.Append(')');
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e.GetType().Name, null);
        }
    }

    private static void WriteList(StringBuilder sb, IReadOnlyList<Expression> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            Write(sb, items[i]);
        }
    }

    private static string OperatorText(BinaryOperator op) => op switch
    {
        BinaryOperator.Or => "OR",
        BinaryOperator.And => "AND",
        BinaryOperator.Equal => "=",
        BinaryOperator.NotEqual => "<>",
        BinaryOperator.LessThan => "<",
        BinaryOperator.GreaterThan => ">",
        BinaryOperator.LessThanOrEqual => "<=",
        BinaryOperator.GreaterThanOrEqual => ">=",
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Concat => "||",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    private static void WriteName(StringBuilder sb, IReadOnlyList<string> parts)
    {
        for (var i = 0; i < parts.Count; i++)
        {
            if (i > 0) sb.Append('.');
            WriteIdentifier(sb, parts[i]);
        }
    }

    private static void WriteIdentifier(StringBuilder sb, string identifier)
    {
        if (IsRegularIdentifier(identifier))
            sb.Append(identifier);
        else
            sb.Append('"').Append(identifier.Replace("\"", "\"\"")).Append('"');
    }

    private static bool IsRegularIdentifier(string s)
    {
        if (s.Length == 0 || !(char.IsAsciiLetter(s[0]) || s[0] == '_')) return false;
        foreach (var c in s)
            if (!(char.IsAsciiLetterOrDigit(c) || c == '_')) return false;
        return !SqlKeywords.IsReserved(s);
    }
}
