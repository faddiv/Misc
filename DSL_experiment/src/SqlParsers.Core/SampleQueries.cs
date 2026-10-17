using System.Text;

namespace SqlParsers;

/// <summary>Benchmark inputs. Every parser must produce the same tree for each of them (checked by the tests).</summary>
public static class SampleQueries
{
    public const string Simple = "SELECT id, name FROM customers WHERE id = 42";

    public const string Medium = """
        SELECT c.id, c.name, o.order_date, SUM(oi.quantity * oi.unit_price) AS total
        FROM customers AS c
        INNER JOIN orders o ON o.customer_id = c.id
        LEFT OUTER JOIN order_items oi ON oi.order_id = o.id
        WHERE c.country IN ('HU', 'AT', 'DE')
          AND o.order_date BETWEEN '2024-01-01' AND '2024-12-31'
          AND c.name LIKE 'A%'
        GROUP BY c.id, c.name, o.order_date
        HAVING SUM(oi.quantity * oi.unit_price) > 1000
        ORDER BY total DESC, c.name
        """;

    public const string Complex = """
        SELECT d.region,
               CASE WHEN d.revenue >= 1000000 THEN 'large'
                    WHEN d.revenue >= 100000 THEN 'medium'
                    ELSE 'small' END AS segment,
               CAST(d.revenue AS DECIMAL(18, 2)) AS revenue,
               (SELECT COUNT(*) FROM employees e WHERE e.region = d.region) AS headcount
        FROM (SELECT r.name AS region, SUM(s.amount) AS revenue
              FROM regions r JOIN sales s ON s.region_id = r.id
              WHERE s.status <> 'cancelled' AND s.amount IS NOT NULL
              GROUP BY r.name) AS d
        WHERE EXISTS (SELECT 1 FROM targets t WHERE t.region = d.region AND t.amount < d.revenue)
           OR d.region NOT IN (SELECT region FROM excluded_regions)
        UNION ALL
        SELECT 'total', 'n/a', CAST(SUM(amount) AS DECIMAL(18, 2)), COUNT(DISTINCT employee_id)
        FROM sales
        WHERE amount > ALL (SELECT threshold FROM limits)
        ORDER BY 1, 3 DESC
        """;

    /// <summary>A wide select list and a long IN list: stresses lists and repetition.</summary>
    public static readonly string Large = BuildLarge(columns: 200, inValues: 1000);

    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        ["Simple"] = Simple,
        ["Medium"] = Medium,
        ["Complex"] = Complex,
        ["Large"] = Large,
    };

    private static string BuildLarge(int columns, int inValues)
    {
        var sb = new StringBuilder("SELECT ");
        for (var i = 0; i < columns; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append($"t.col{i} * {i} + {i}.5 AS c{i}");
        }
        sb.Append(" FROM big_table t WHERE t.id IN (");
        for (var i = 0; i < inValues; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(i);
        }
        sb.Append(") AND t.name <> 'x'");
        return sb.ToString();
    }
}
