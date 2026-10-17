namespace SqlParsers.Tests;

/// <summary>Input SQL and the expected canonical form produced by <see cref="SqlFormatter"/>.</summary>
public sealed record SqlCase(string Name, string Sql, string Expected);

public static class TestQueries
{
    public static readonly IReadOnlyList<SqlCase> Valid =
    [
        new("star",
            "SELECT * FROM t",
            "SELECT * FROM t"),

        new("lowercase keywords",
            "select a, b from t",
            "SELECT a, b FROM t"),

        new("mixed case keywords",
            "SeLeCt a FrOm t WhErE a = 1",
            "SELECT a FROM t WHERE (a = 1)"),

        new("distinct and aliases",
            "SELECT DISTINCT t.a AS x, b y FROM schema1.t AS t",
            "SELECT DISTINCT t.a AS x, b AS y FROM schema1.t AS t"),

        new("qualified star",
            "SELECT t.*, u.* FROM t, u",
            "SELECT t.*, u.* FROM t, u"),

        new("arithmetic precedence",
            "SELECT a + b * c - d FROM t",
            "SELECT ((a + (b * c)) - d) FROM t"),

        new("unary minus",
            "SELECT -a * b, - (a + b) FROM t",
            "SELECT ((-a) * b), (-(a + b)) FROM t"),

        new("concatenation and escaped quote",
            "SELECT a || 'x''y' FROM t",
            "SELECT (a || 'x''y') FROM t"),

        new("numeric literals",
            "SELECT 1, 2.5, .5, 1.5E3, 3. FROM t",
            "SELECT 1, 2.5, .5, 1.5E3, 3. FROM t"),

        new("boolean precedence",
            "SELECT a FROM t WHERE a = 1 AND b <> 2 OR NOT c >= 3",
            "SELECT a FROM t WHERE (((a = 1) AND (b <> 2)) OR (NOT (c >= 3)))"),

        new("between",
            "SELECT a FROM t WHERE a BETWEEN 1 AND 10 AND b NOT BETWEEN x AND y",
            "SELECT a FROM t WHERE ((a BETWEEN 1 AND 10) AND (b NOT BETWEEN x AND y))"),

        new("in list",
            "SELECT a FROM t WHERE a IN (1, 2, 3) OR b NOT IN ('x')",
            "SELECT a FROM t WHERE ((a IN (1, 2, 3)) OR (b NOT IN ('x')))"),

        new("like with escape",
            "SELECT a FROM t WHERE name LIKE 'A%' AND code NOT LIKE 'x!_%' ESCAPE '!'",
            "SELECT a FROM t WHERE ((name LIKE 'A%') AND (code NOT LIKE 'x!_%' ESCAPE '!'))"),

        new("is null",
            "SELECT a FROM t WHERE b IS NULL AND c IS NOT NULL",
            "SELECT a FROM t WHERE ((b IS NULL) AND (c IS NOT NULL))"),

        new("subquery predicates",
            "SELECT a FROM t WHERE EXISTS (SELECT 1 FROM u WHERE u.id = t.id) AND a IN (SELECT b FROM v) AND c > ALL (SELECT d FROM w)",
            "SELECT a FROM t WHERE ((EXISTS (SELECT 1 FROM u WHERE (u.id = t.id)) AND (a IN (SELECT b FROM v))) AND (c > ALL (SELECT d FROM w)))"),

        new("scalar subquery",
            "SELECT (SELECT MAX(b) FROM u) AS m FROM t",
            "SELECT (SELECT MAX(b) FROM u) AS m FROM t"),

        new("aggregates, group by, having",
            "SELECT COUNT(*), COUNT(DISTINCT a), SUM(b), f() FROM t GROUP BY c, d HAVING COUNT(*) > 1",
            "SELECT COUNT(*), COUNT(DISTINCT a), SUM(b), f() FROM t GROUP BY c, d HAVING (COUNT(*) > 1)"),

        new("case expressions",
            "SELECT CASE WHEN a > 0 THEN 'pos' WHEN a < 0 THEN 'neg' ELSE 'zero' END, CASE b WHEN 1 THEN 'one' END FROM t",
            "SELECT CASE WHEN (a > 0) THEN 'pos' WHEN (a < 0) THEN 'neg' ELSE 'zero' END, CASE b WHEN 1 THEN 'one' END FROM t"),

        new("cast",
            "SELECT CAST(a AS VARCHAR(20)), CAST(b AS decimal(10, 2)), CAST(c AS double precision) FROM t",
            "SELECT CAST(a AS VARCHAR(20)), CAST(b AS DECIMAL(10,2)), CAST(c AS DOUBLE PRECISION) FROM t"),

        new("joins",
            "SELECT * FROM a JOIN b ON a.id = b.id LEFT OUTER JOIN c USING (id) CROSS JOIN d NATURAL FULL JOIN e",
            "SELECT * FROM ((((a INNER JOIN b ON (a.id = b.id)) LEFT OUTER JOIN c USING (id)) CROSS JOIN d) NATURAL FULL OUTER JOIN e)"),

        new("derived table",
            "SELECT x.a FROM (SELECT a FROM t) AS x INNER JOIN u y ON x.a = y.a",
            "SELECT x.a FROM ((SELECT a FROM t) AS x INNER JOIN u AS y ON (x.a = y.a))"),

        new("set operation precedence",
            "SELECT a FROM t UNION SELECT a FROM u INTERSECT SELECT a FROM v EXCEPT ALL SELECT a FROM w",
            "((SELECT a FROM t UNION (SELECT a FROM u INTERSECT SELECT a FROM v)) EXCEPT ALL SELECT a FROM w)"),

        new("parenthesized query with order by",
            "(SELECT a FROM t UNION ALL SELECT a FROM u) ORDER BY a DESC, 2",
            "(SELECT a FROM t UNION ALL SELECT a FROM u) ORDER BY a DESC, 2 ASC"),

        new("quoted identifiers",
            "SELECT \"Order\".\"Total Amount\" FROM \"Order\"",
            "SELECT \"Order\".\"Total Amount\" FROM \"Order\""),

        new("comment and semicolon",
            "SELECT a -- trailing comment\n FROM t ;",
            "SELECT a FROM t"),
    ];

    public static readonly IReadOnlyList<string> Invalid =
    [
        "SELECT FROM t",
        "SELECT a",
        "SELECT a FROM",
        "SELECT a FROM t WHERE",
        "SELECT a FROM t WHERE a = = 1",
        "SELECT a, FROM t",
        "SELECT a FROM t ORDER a",
        "SELECT select FROM t",
        "SELECT 'unterminated FROM t",
        "SELECT a FROM t extra tokens",
        "SELECT a FROM t JOIN u",
        "INSERT INTO t VALUES (1)",
    ];
}
