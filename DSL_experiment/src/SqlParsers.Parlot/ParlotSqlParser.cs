using System.Globalization;
using Parlot;
using Parlot.Fluent;
using SqlParsers.Ast;
using StringLiteral = SqlParsers.Ast.StringLiteral;

namespace SqlParsers.Parlot;

/// <summary>Scannerless parser built with Parlot's fluent combinators.</summary>
public sealed class ParlotSqlParser : ISqlParser
{
    public string Name => "Parlot";

    public SelectStatement Parse(string sql)
    {
        if (ParlotGrammar.Statement.TryParse(sql, out var result, out var error) && result is not null)
            return result;

        throw new SqlParseException(error is null ? "Invalid SQL" : $"{error.Message} at {error.Position}");
    }
}

internal static class ParlotGrammar
{
    private static readonly IReadOnlyList<Expression> NoExpressions = [];
    private static readonly FunctionArguments NoArguments = new(SetQuantifier.None, NoExpressions, false);

    public static readonly Parser<SelectStatement> Statement = Build();

    private sealed record FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

    private readonly record struct JoinPart(JoinType Type, bool Natural, TableReference Right, JoinCondition? Condition);

    private static Parser<SelectStatement> Build()
    {
        // ------------------------------------------------------------ lexical

        // skip := whitespace? ("--" rest-of-line skip)?  -- written without ZeroOrMany to avoid a list per token
        var whitespace = Literals.WhiteSpace(includeNewLines: true);
        var skip = Deferred<bool>();
        var commentThenSkip = Literals.Text("--")
            .SkipAnd(ZeroOrOne(Literals.Pattern(static c => c != '\r' && c != '\n')))
            .SkipAnd(skip);
        skip.Parser = ZeroOrOne(whitespace).SkipAnd(ZeroOrOne(commentThenSkip)).Then(static _ => true);

        Parser<T> Tok<T>(Parser<T> p) => p.AndSkip(skip);
        Parser<char> Sym(char c) => Tok(Literals.Char(c));
        Parser<string> Op(string s) => Tok(Literals.Text(s));

        var rawWord = Literals.Identifier();

        Parser<TextSpan> Kw(string k) =>
            Tok(rawWord.When((_, w) => w.Span.Equals(k, StringComparison.OrdinalIgnoreCase)));

        var kwAll = Kw("ALL");
        var kwAnd = Kw("AND");
        var kwAny = Kw("ANY");
        var kwAs = Kw("AS");
        var kwAsc = Kw("ASC");
        var kwBetween = Kw("BETWEEN");
        var kwBy = Kw("BY");
        var kwCase = Kw("CASE");
        var kwCast = Kw("CAST");
        var kwCross = Kw("CROSS");
        var kwDesc = Kw("DESC");
        var kwDistinct = Kw("DISTINCT");
        var kwElse = Kw("ELSE");
        var kwEnd = Kw("END");
        var kwEscape = Kw("ESCAPE");
        var kwExcept = Kw("EXCEPT");
        var kwExists = Kw("EXISTS");
        var kwFrom = Kw("FROM");
        var kwFull = Kw("FULL");
        var kwGroup = Kw("GROUP");
        var kwHaving = Kw("HAVING");
        var kwIn = Kw("IN");
        var kwInner = Kw("INNER");
        var kwIntersect = Kw("INTERSECT");
        var kwIs = Kw("IS");
        var kwJoin = Kw("JOIN");
        var kwLeft = Kw("LEFT");
        var kwLike = Kw("LIKE");
        var kwNatural = Kw("NATURAL");
        var kwNot = Kw("NOT");
        var kwNull = Kw("NULL");
        var kwOn = Kw("ON");
        var kwOr = Kw("OR");
        var kwOrder = Kw("ORDER");
        var kwOuter = Kw("OUTER");
        var kwRight = Kw("RIGHT");
        var kwSelect = Kw("SELECT");
        var kwSome = Kw("SOME");
        var kwThen = Kw("THEN");
        var kwUnion = Kw("UNION");
        var kwUsing = Kw("USING");
        var kwWhen = Kw("WHEN");
        var kwWhere = Kw("WHERE");

        var regularIdentifier = Tok(rawWord.When(static (_, w) => !SqlKeywords.IsReserved(w.Span)))
            .Then(static w => w.ToString());
        var quotedIdentifier = Tok(
            Literals.Char('"')
                .SkipAnd(OneOrMany(OneOf(
                    Literals.Pattern(static c => c != '"').Then(static s => s.ToString()),
                    Literals.Text("\"\"").Then(static _ => "\""))))
                .AndSkip(Literals.Char('"')))
            .Then(static parts => string.Concat(parts));
        var identifier = OneOf(quotedIdentifier, regularIdentifier);

        var stringLiteral = Tok(
                Literals.Char('\'')
                    .SkipAnd(ZeroOrMany(OneOf(
                        Literals.Pattern(static c => c != '\'').Then(static s => s.ToString()),
                        Literals.Text("''").Then(static _ => "'"))))
                    .AndSkip(Literals.Char('\'')))
            .Then(static parts => string.Concat(parts));

        var digits = Literals.Pattern(static c => char.IsAsciiDigit(c));
        var fraction = Literals.Char('.')
            .SkipAnd(ZeroOrOne(digits.Then(static d => d.ToString())))
            .Then(static d => "." + d);
        var exponent = Literals.Pattern(static c => c is 'e' or 'E', maxSize: 1)
            .And(ZeroOrOne(Literals.Pattern(static c => c is '+' or '-', maxSize: 1).Then(static s => s.ToString())))
            .And(digits)
            .Then(static t => t.Item1 + t.Item2 + t.Item3);
        var mantissa = OneOf(
            digits.And(ZeroOrOne(fraction)).Then(static t => t.Item1.ToString() + t.Item2),
            Literals.Char('.').SkipAnd(digits).Then(static d => "." + d.ToString()));
        var number = Tok(mantissa.And(ZeroOrOne(exponent)).Then(static t => t.Item1 + t.Item2));
        var integer = Tok(digits).Then(static d => int.Parse(d.Span, NumberStyles.None, CultureInfo.InvariantCulture));

        var comma = Sym(',');
        var dot = Sym('.');
        var star = Sym('*');
        var lParen = Sym('(');
        var rParen = Sym(')');
        Parser<T> Parens<T>(Parser<T> p) => lParen.SkipAnd(p).AndSkip(rParen);

        // ------------------------------------------------------------ forward references

        var expr = Deferred<Expression>();
        var queryExpr = Deferred<QueryExpression>();
        var subqueryExpr = Deferred<QueryExpression>();
        var subquery = Parens(subqueryExpr);

        // ------------------------------------------------------------ expressions

        // ZeroOrOne yields default(SetQuantifier) == None when absent.
        var setQuantifier = ZeroOrOne(OneOf(
            kwDistinct.Then(static _ => SetQuantifier.Distinct),
            kwAll.Then(static _ => SetQuantifier.All)));

        var qualifiedName = Separated(dot, identifier);

        var functionArguments = Parens(ZeroOrOne(OneOf(
                star.Then(static _ => new FunctionArguments(SetQuantifier.None, NoExpressions, true)),
                setQuantifier.And(Separated(comma, expr)).Then(static t => new FunctionArguments(t.Item1, t.Item2, false)))))
            .Then(static a => a ?? NoArguments);

        var nameOrCall = qualifiedName.And(ZeroOrOne(functionArguments)).Then(static t => t.Item2 is { } a
            ? (Expression)new FunctionCall(string.Join(".", t.Item1), a.Quantifier, a.Arguments, a.IsStar)
            : new ColumnReference(t.Item1));

        var whenClause = kwWhen.SkipAnd(expr).AndSkip(kwThen).And(expr)
            .Then(static t => new WhenClause(t.Item1, t.Item2));
        var caseExpression = kwCase.SkipAnd(ZeroOrOne(expr))
            .And(OneOrMany(whenClause))
            .And(ZeroOrOne(kwElse.SkipAnd(expr)))
            .AndSkip(kwEnd)
            .Then(static t => (Expression)new CaseExpression(t.Item1, t.Item2, t.Item3));

        var dataType = OneOrMany(regularIdentifier)
            .And(ZeroOrOne(Parens(Separated(comma, integer))))
            .Then(static t => new DataType(
                string.Join(" ", t.Item1.Select(w => w.ToUpperInvariant())),
                t.Item2 ?? Array.Empty<int>()));
        var castExpression = kwCast.SkipAnd(Parens(expr.AndSkip(kwAs).And(dataType)))
            .Then(static t => (Expression)new CastExpression(t.Item1, t.Item2));

        var existsExpression = kwExists.SkipAnd(subquery).Then(static q => (Expression)new ExistsExpression(q));

        // "(" starts either a scalar subquery (must begin with SELECT) or a parenthesized expression.
        var parenthesized = Parens(OneOf(
            subqueryExpr.Then(static q => (Expression)new ScalarSubquery(q)),
            expr));

        var primary = OneOf(
            number.Then(static n => (Expression)new NumericLiteral(n)),
            stringLiteral.Then(static s => (Expression)new StringLiteral(s)),
            kwNull.Then(static _ => (Expression)NullLiteral.Instance),
            caseExpression,
            castExpression,
            existsExpression,
            parenthesized,
            nameOrCall);

        var unary = Deferred<Expression>();
        unary.Parser = OneOf(
            Sym('-').SkipAnd(unary).Then(static e => (Expression)new UnaryExpression(UnaryOperator.Minus, e)),
            Sym('+').SkipAnd(unary).Then(static e => (Expression)new UnaryExpression(UnaryOperator.Plus, e)),
            primary);

        var multiplicativeOperator = OneOf(
            star.Then(static _ => BinaryOperator.Multiply),
            Sym('/').Then(static _ => BinaryOperator.Divide));
        var additiveOperator = OneOf(
            Op("||").Then(static _ => BinaryOperator.Concat),
            Sym('+').Then(static _ => BinaryOperator.Add),
            Sym('-').Then(static _ => BinaryOperator.Subtract));
        var comparisonOperator = OneOf(
            Op("<>").Then(static _ => BinaryOperator.NotEqual),
            Op("<=").Then(static _ => BinaryOperator.LessThanOrEqual),
            Op(">=").Then(static _ => BinaryOperator.GreaterThanOrEqual),
            Op("=").Then(static _ => BinaryOperator.Equal),
            Op("<").Then(static _ => BinaryOperator.LessThan),
            Op(">").Then(static _ => BinaryOperator.GreaterThan));

        var multiplicative = LeftAssociative(unary, multiplicativeOperator);
        var additive = LeftAssociative(multiplicative, additiveOperator);

        var quantifier = OneOf(
            kwAny.Then(static _ => Quantifier.Any),
            kwSome.Then(static _ => Quantifier.Some),
            kwAll.Then(static _ => Quantifier.All));

        var comparisonSuffix = comparisonOperator
            .And(OneOf(
                quantifier.And(subquery).Then(static t =>
                    (Func<BinaryOperator, Func<Expression, Expression>>)(op => l => new QuantifiedComparison(l, op, t.Item1, t.Item2))),
                additive.Then(static r =>
                    (Func<BinaryOperator, Func<Expression, Expression>>)(op => l => new BinaryExpression(op, l, r)))))
            .Then(static t => t.Item2(t.Item1));

        var notOpt = ZeroOrOne(kwNot.Then(static _ => true));

        var isNullSuffix = kwIs.SkipAnd(notOpt).AndSkip(kwNull)
            .Then(static n => (Func<Expression, Expression>)(l => new IsNullExpression(l, n)));

        var betweenTail = kwBetween.SkipAnd(additive).AndSkip(kwAnd).And(additive)
            .Then(static t => (Func<bool, Func<Expression, Expression>>)(n => l => new BetweenExpression(l, n, t.Item1, t.Item2)));
        var inTail = kwIn.SkipAnd(Parens(OneOf(
            subqueryExpr.Then(static q =>
                (Func<bool, Func<Expression, Expression>>)(n => l => new InSubqueryExpression(l, n, q))),
            Separated(comma, expr).Then(static values =>
                (Func<bool, Func<Expression, Expression>>)(n => l => new InListExpression(l, n, values))))));
        var likeTail = kwLike.SkipAnd(additive).And(ZeroOrOne(kwEscape.SkipAnd(additive)))
            .Then(static t => (Func<bool, Func<Expression, Expression>>)(n => l => new LikeExpression(l, n, t.Item1, t.Item2)));
        var negatableSuffix = notOpt.And(OneOf(betweenTail, inTail, likeTail))
            .Then(static t => t.Item2(t.Item1));

        var predicate = additive.And(ZeroOrOne(OneOf(comparisonSuffix, isNullSuffix, negatableSuffix)))
            .Then(static t => t.Item2 is null ? t.Item1 : t.Item2(t.Item1));

        var notExpression = Deferred<Expression>();
        notExpression.Parser = OneOf(
            kwNot.SkipAnd(notExpression).Then(static e => (Expression)new UnaryExpression(UnaryOperator.Not, e)),
            predicate);

        var andExpression = LeftAssociative(notExpression, kwAnd.Then(static _ => BinaryOperator.And));
        expr.Parser = LeftAssociative(andExpression, kwOr.Then(static _ => BinaryOperator.Or));

        // ------------------------------------------------------------ queries

        var alias = OneOf(kwAs.SkipAnd(identifier), identifier);

        var qualifiedStar = OneOrMany(identifier.AndSkip(dot)).AndSkip(star)
            .Then(static q => (SelectItem)new AllColumns(q));
        var expressionItem = expr.And(ZeroOrOne(alias))
            .Then(static t => (SelectItem)new ExpressionItem(t.Item1, t.Item2));
        var selectAll = (IReadOnlyList<SelectItem>)[new AllColumns([])];
        var selectList = OneOf(
            star.Then(_ => selectAll),
            Separated(comma, OneOf(qualifiedStar, expressionItem)));

        var derivedTable = Parens(queryExpr).And(alias)
            .Then(static TableReference (t) => new DerivedTable(t.Item1, t.Item2));
        var namedTable = qualifiedName.And(ZeroOrOne(alias))
            .Then(static TableReference (t) => new NamedTable(t.Item1, t.Item2));
        var tablePrimary = OneOf(derivedTable, namedTable);

        // ZeroOrOne(joinType) yields default(JoinType) == Inner when absent.
        var joinType = OneOf(
            kwInner.Then(static _ => JoinType.Inner),
            kwLeft.AndSkip(ZeroOrOne(kwOuter)).Then(static _ => JoinType.Left),
            kwRight.AndSkip(ZeroOrOne(kwOuter)).Then(static _ => JoinType.Right),
            kwFull.AndSkip(ZeroOrOne(kwOuter)).Then(static _ => JoinType.Full));
        var joinSpecification = OneOf(
            kwOn.SkipAnd(expr).Then(static e => (JoinCondition)new OnCondition(e)),
            kwUsing.SkipAnd(Parens(Separated(comma, identifier))).Then(static c => (JoinCondition)new UsingCondition(c)));

        var crossJoin = kwCross.SkipAnd(kwJoin).SkipAnd(tablePrimary)
            .Then(static r => new JoinPart(JoinType.Cross, false, r, null));
        var naturalJoin = kwNatural.SkipAnd(ZeroOrOne(joinType)).AndSkip(kwJoin).And(tablePrimary)
            .Then(static t => new JoinPart(t.Item1, true, t.Item2, null));
        var qualifiedJoin = ZeroOrOne(joinType).AndSkip(kwJoin).And(tablePrimary).And(joinSpecification)
            .Then(static t => new JoinPart(t.Item1, false, t.Item2, t.Item3));
        var tableReference = tablePrimary.And(ZeroOrMany(OneOf(crossJoin, naturalJoin, qualifiedJoin)))
            .Then(static t =>
            {
                var result = t.Item1;
                foreach (var j in t.Item2)
                    result = new JoinedTable(result, j.Type, j.Natural, j.Right, j.Condition);
                return result;
            });

        var querySpecification = kwSelect.SkipAnd(setQuantifier)
            .And(selectList)
            .AndSkip(kwFrom)
            .And(Separated(comma, tableReference))
            .And(ZeroOrOne(kwWhere.SkipAnd(expr)))
            .And(ZeroOrOne(kwGroup.SkipAnd(kwBy).SkipAnd(Separated(comma, expr))))
            .And(ZeroOrOne(kwHaving.SkipAnd(expr)))
            .Then(static t => (QueryExpression)new QuerySpecification(
                t.Item1, t.Item2, t.Item3, t.Item4, t.Item5 ?? NoExpressions, t.Item6));

        var allOpt = ZeroOrOne(kwAll.Then(static _ => true));
        var queryPrimary = OneOf(querySpecification, Parens(queryExpr));

        Parser<QueryExpression> QueryTerm(Parser<QueryExpression> first) =>
            first.And(ZeroOrMany(kwIntersect.SkipAnd(allOpt).And(queryPrimary)))
                .Then(static t =>
                {
                    var result = t.Item1;
                    foreach (var (all, right) in t.Item2)
                        result = new SetOperation(result, SetOperator.Intersect, all, right);
                    return result;
                });

        var queryTerm = QueryTerm(queryPrimary);
        var unionOperator = OneOf(
            kwUnion.Then(static _ => SetOperator.Union),
            kwExcept.Then(static _ => SetOperator.Except));

        Parser<QueryExpression> QueryExpr(Parser<QueryExpression> firstTerm) =>
            firstTerm.And(ZeroOrMany(unionOperator.And(allOpt).And(queryTerm)))
                .Then(static t =>
                {
                    var result = t.Item1;
                    foreach (var (op, all, right) in t.Item2)
                        result = new SetOperation(result, op, all, right);
                    return result;
                });

        queryExpr.Parser = QueryExpr(queryTerm);
        // Subqueries inside expressions must start with SELECT (see README).
        subqueryExpr.Parser = QueryExpr(QueryTerm(querySpecification));

        // ------------------------------------------------------------ statement

        // ZeroOrOne yields default(SortDirection) == Ascending when absent.
        var direction = ZeroOrOne(OneOf(
            kwAsc.Then(static _ => SortDirection.Ascending),
            kwDesc.Then(static _ => SortDirection.Descending)));
        var orderItem = expr.And(direction).Then(static t => new OrderByItem(t.Item1, t.Item2));
        var orderBy = kwOrder.SkipAnd(kwBy).SkipAnd(Separated(comma, orderItem));

        return skip.SkipAnd(queryExpr)
            .And(ZeroOrOne(orderBy))
            .AndSkip(ZeroOrOne(Sym(';')))
            .Eof()
            .Then(static t => new SelectStatement(t.Item1, t.Item2 ?? Array.Empty<OrderByItem>()));
    }

    private static Parser<Expression> LeftAssociative(Parser<Expression> operand, Parser<BinaryOperator> op) =>
        operand.And(ZeroOrMany(op.And(operand)))
            .Then(static t =>
            {
                var result = t.Item1;
                foreach (var (o, right) in t.Item2)
                    result = new BinaryExpression(o, result, right);
                return result;
            });
}
