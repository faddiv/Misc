using System.Globalization;
using SqlParsers.Ast;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace SqlParsers.Superpower;

public enum SqlToken
{
    Word,
    QuotedIdentifier,
    Number,
    String,
    LParen,
    RParen,
    Comma,
    Dot,
    Semicolon,
    Star,
    Plus,
    Minus,
    Slash,
    Concat,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
}

/// <summary>Two-phase parser: Superpower tokenizer followed by token-list combinators.</summary>
public sealed class SuperpowerSqlParser : ISqlParser
{
    public string Name => "Superpower";

    public SelectStatement Parse(string sql)
    {
        var tokens = SuperpowerGrammar.Tokenizer.TryTokenize(sql);
        if (!tokens.HasValue)
            throw new SqlParseException(tokens.ToString());

        var result = SuperpowerGrammar.Statement.TryParse(tokens.Value);
        return result.HasValue ? result.Value : throw new SqlParseException(result.ToString());
    }
}

internal static class SuperpowerGrammar
{
    private static readonly IReadOnlyList<Expression> NoExpressions = Array.Empty<Expression>();

    public static readonly Tokenizer<SqlToken> Tokenizer = BuildTokenizer();

    public static readonly TokenListParser<SqlToken, SelectStatement> Statement = BuildParser();

    private static Tokenizer<SqlToken> BuildTokenizer() =>
        new TokenizerBuilder<SqlToken>()
            .Ignore(Span.WhiteSpace)
            .Ignore(Span.Regex(@"--[^\r\n]*"))
            .Match(Span.EqualTo("<>"), SqlToken.NotEqual)
            .Match(Span.EqualTo("<="), SqlToken.LessThanOrEqual)
            .Match(Span.EqualTo(">="), SqlToken.GreaterThanOrEqual)
            .Match(Span.EqualTo("||"), SqlToken.Concat)
            .Match(Character.EqualTo('='), SqlToken.Equal)
            .Match(Character.EqualTo('<'), SqlToken.LessThan)
            .Match(Character.EqualTo('>'), SqlToken.GreaterThan)
            .Match(Character.EqualTo('('), SqlToken.LParen)
            .Match(Character.EqualTo(')'), SqlToken.RParen)
            .Match(Character.EqualTo(','), SqlToken.Comma)
            .Match(Character.EqualTo(';'), SqlToken.Semicolon)
            .Match(Character.EqualTo('*'), SqlToken.Star)
            .Match(Character.EqualTo('+'), SqlToken.Plus)
            .Match(Character.EqualTo('-'), SqlToken.Minus)
            .Match(Character.EqualTo('/'), SqlToken.Slash)
            .Match(Span.Regex(@"(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?"), SqlToken.Number, requireDelimiters: true)
            .Match(Character.EqualTo('.'), SqlToken.Dot)
            .Match(Span.Regex("'(?:[^']|'')*'"), SqlToken.String)
            .Match(Span.Regex("\"(?:[^\"]|\"\")+\""), SqlToken.QuotedIdentifier)
            .Match(Span.Regex("[A-Za-z_][A-Za-z0-9_]*"), SqlToken.Word, requireDelimiters: true)
            .Build();

    private static ReadOnlySpan<char> Chars(Token<SqlToken> token) =>
        token.Span.Source.AsSpan(token.Span.Position.Absolute, token.Span.Length);

    private static string Unquote(Token<SqlToken> token, char quote)
    {
        var text = token.ToStringValue();
        return text.Substring(1, text.Length - 2).Replace(new string(quote, 2), quote.ToString());
    }

    private static TokenListParser<SqlToken, Expression> LeftAssociative(
        TokenListParser<SqlToken, Expression> operand,
        TokenListParser<SqlToken, BinaryOperator> op) =>
        Parse.Chain(op, operand, (o, l, r) => (Expression)new BinaryExpression(o, l, r));

    private static TokenListParser<SqlToken, SelectStatement> BuildParser()
    {
        // ------------------------------------------------------------ tokens

        TokenListParser<SqlToken, Token<SqlToken>> Kw(string k) => Token.EqualToValueIgnoreCase(SqlToken.Word, k);

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

        var lParen = Token.EqualTo(SqlToken.LParen);
        var rParen = Token.EqualTo(SqlToken.RParen);
        var comma = Token.EqualTo(SqlToken.Comma);
        var dot = Token.EqualTo(SqlToken.Dot);
        var star = Token.EqualTo(SqlToken.Star);

        TokenListParser<SqlToken, T> Parens<T>(TokenListParser<SqlToken, T> p) =>
            from _ in lParen
            from value in p
            from __ in rParen
            select value;

        var regularIdentifier = Token.EqualTo(SqlToken.Word)
            .Where(t => !SqlKeywords.IsReserved(Chars(t)), "identifier")
            .Select(t => t.ToStringValue());
        var quotedIdentifier = Token.EqualTo(SqlToken.QuotedIdentifier).Select(t => Unquote(t, '"'));
        var identifier = quotedIdentifier.Or(regularIdentifier);

        var number = Token.EqualTo(SqlToken.Number).Select(t => t.ToStringValue());
        var integer = Token.EqualTo(SqlToken.Number)
            .Where(t => int.TryParse(Chars(t), NumberStyles.None, CultureInfo.InvariantCulture, out _), "integer")
            .Select(t => int.Parse(Chars(t), NumberStyles.None, CultureInfo.InvariantCulture));
        var stringLiteral = Token.EqualTo(SqlToken.String).Select(t => Unquote(t, '\''));

        // ------------------------------------------------------------ forward references

        TokenListParser<SqlToken, Expression> expression = null!;
        var expr = Parse.Ref(() => expression);
        TokenListParser<SqlToken, QueryExpression> queryExpression = null!;
        var queryExpr = Parse.Ref(() => queryExpression);
        TokenListParser<SqlToken, QueryExpression> subqueryExpression = null!;
        var subqueryExpr = Parse.Ref(() => subqueryExpression);
        var subquery = Parens(subqueryExpr);

        // ------------------------------------------------------------ expressions

        var setQuantifier = kwDistinct.Value(SetQuantifier.Distinct)
            .Or(kwAll.Value(SetQuantifier.All))
            .OptionalOrDefault(SetQuantifier.None);

        var qualifiedName = identifier.AtLeastOnceDelimitedBy(dot).Select(x => (IReadOnlyList<string>)x);

        var functionArguments = Parens(
            star.Value((Quantifier: SetQuantifier.None, Args: NoExpressions, IsStar: true))
                .Or(from q in setQuantifier
                    from args in expr.AtLeastOnceDelimitedBy(comma)
                    select (Quantifier: q, Args: (IReadOnlyList<Expression>)args, IsStar: false))
                .OptionalOrDefault((Quantifier: SetQuantifier.None, Args: NoExpressions, IsStar: false)));

        var nameOrCall =
            from name in qualifiedName
            from args in functionArguments.Optional()
            select args is { } a
                ? (Expression)new FunctionCall(string.Join(".", name), a.Quantifier, a.Args, a.IsStar)
                : new ColumnReference(name);

        var whenClause =
            from _ in kwWhen
            from condition in expr
            from __ in kwThen
            from result in expr
            select new WhenClause(condition, result);
        var caseExpression =
            from _ in kwCase
            from operand in expr.OptionalOrDefault()
            from whens in whenClause.AtLeastOnce()
            from @else in kwElse.IgnoreThen(expr).OptionalOrDefault()
            from __ in kwEnd
            select (Expression)new CaseExpression(operand, whens, @else);

        var dataType =
            from words in regularIdentifier.AtLeastOnce()
            from args in Parens(integer.AtLeastOnceDelimitedBy(comma)).OptionalOrDefault(Array.Empty<int>())
            select new DataType(string.Join(" ", words.Select(w => w.ToUpperInvariant())), args);
        var castExpression =
            from _ in kwCast
            from cast in Parens(
                from operand in expr
                from __ in kwAs
                from type in dataType
                select new CastExpression(operand, type))
            select (Expression)cast;

        var existsExpression = kwExists.IgnoreThen(subquery).Select(q => (Expression)new ExistsExpression(q));

        // "(" starts either a scalar subquery (must begin with SELECT) or a parenthesized expression.
        var parenthesized = Parens(subqueryExpr.Select(q => (Expression)new ScalarSubquery(q)).Or(expr));

        var primary = Parse.OneOf(
            number.Select(n => (Expression)new NumericLiteral(n)),
            stringLiteral.Select(s => (Expression)new StringLiteral(s)),
            kwNull.Value((Expression)NullLiteral.Instance),
            caseExpression,
            castExpression,
            existsExpression,
            parenthesized,
            nameOrCall);

        TokenListParser<SqlToken, Expression> unary = null!;
        var unaryRef = Parse.Ref(() => unary);
        unary = Token.EqualTo(SqlToken.Minus).IgnoreThen(unaryRef).Select(e => (Expression)new UnaryExpression(UnaryOperator.Minus, e))
            .Or(Token.EqualTo(SqlToken.Plus).IgnoreThen(unaryRef).Select(e => (Expression)new UnaryExpression(UnaryOperator.Plus, e)))
            .Or(primary);

        var multiplicativeOperator = star.Value(BinaryOperator.Multiply)
            .Or(Token.EqualTo(SqlToken.Slash).Value(BinaryOperator.Divide));
        var additiveOperator = Token.EqualTo(SqlToken.Concat).Value(BinaryOperator.Concat)
            .Or(Token.EqualTo(SqlToken.Plus).Value(BinaryOperator.Add))
            .Or(Token.EqualTo(SqlToken.Minus).Value(BinaryOperator.Subtract));
        var comparisonOperator = Parse.OneOf(
            Token.EqualTo(SqlToken.Equal).Value(BinaryOperator.Equal),
            Token.EqualTo(SqlToken.NotEqual).Value(BinaryOperator.NotEqual),
            Token.EqualTo(SqlToken.LessThan).Value(BinaryOperator.LessThan),
            Token.EqualTo(SqlToken.GreaterThan).Value(BinaryOperator.GreaterThan),
            Token.EqualTo(SqlToken.LessThanOrEqual).Value(BinaryOperator.LessThanOrEqual),
            Token.EqualTo(SqlToken.GreaterThanOrEqual).Value(BinaryOperator.GreaterThanOrEqual));

        var multiplicative = LeftAssociative(unaryRef, multiplicativeOperator);
        var additive = LeftAssociative(multiplicative, additiveOperator);

        var quantifier = kwAny.Value(Quantifier.Any)
            .Or(kwSome.Value(Quantifier.Some))
            .Or(kwAll.Value(Quantifier.All));

        var comparisonSuffix = comparisonOperator.Then(op =>
            (from q in quantifier
             from s in subquery
             select (Func<Expression, Expression>)(l => new QuantifiedComparison(l, op, q, s)))
            .Or(additive.Select(r => (Func<Expression, Expression>)(l => new BinaryExpression(op, l, r)))));

        var notOpt = kwNot.Value(true).OptionalOrDefault(false);

        var isNullSuffix =
            from _ in kwIs
            from negated in notOpt
            from __ in kwNull
            select (Func<Expression, Expression>)(l => new IsNullExpression(l, negated));

        var betweenTail =
            from _ in kwBetween
            from low in additive
            from __ in kwAnd
            from high in additive
            select (Func<bool, Func<Expression, Expression>>)(n => l => new BetweenExpression(l, n, low, high));
        var inTail = kwIn.IgnoreThen(Parens(
            subqueryExpr.Select(q => (Func<bool, Func<Expression, Expression>>)(n => l => new InSubqueryExpression(l, n, q)))
                .Or(expr.AtLeastOnceDelimitedBy(comma)
                    .Select(values => (Func<bool, Func<Expression, Expression>>)(n => l => new InListExpression(l, n, values))))));
        var likeTail =
            from _ in kwLike
            from pattern in additive
            from escape in kwEscape.IgnoreThen(additive).OptionalOrDefault()
            select (Func<bool, Func<Expression, Expression>>)(n => l => new LikeExpression(l, n, pattern, escape));
        var negatableSuffix =
            from negated in notOpt
            from tail in betweenTail.Or(inTail).Or(likeTail)
            select tail(negated);

        var predicate =
            from left in additive
            from suffix in comparisonSuffix.Or(isNullSuffix).Or(negatableSuffix).OptionalOrDefault()
            select suffix is null ? left : suffix(left);

        TokenListParser<SqlToken, Expression> notExpression = null!;
        var notRef = Parse.Ref(() => notExpression);
        notExpression = kwNot.IgnoreThen(notRef).Select(e => (Expression)new UnaryExpression(UnaryOperator.Not, e))
            .Or(predicate);

        var andExpression = LeftAssociative(notRef, kwAnd.Value(BinaryOperator.And));
        expression = LeftAssociative(andExpression, kwOr.Value(BinaryOperator.Or));

        // ------------------------------------------------------------ queries

        var alias = kwAs.IgnoreThen(identifier).Or(identifier);

        var qualifiedStar = (
                from parts in identifier.Then(id => dot.Value(id)).AtLeastOnce()
                from _ in star
                select (SelectItem)new AllColumns(parts))
            .Try();
        var expressionItem =
            from e in expr
            from a in alias.OptionalOrDefault()
            select (SelectItem)new ExpressionItem(e, a);
        var selectAll = (IReadOnlyList<SelectItem>)[new AllColumns([])];
        var selectList = star.Value(selectAll)
            .Or(qualifiedStar.Or(expressionItem).AtLeastOnceDelimitedBy(comma).Select(x => (IReadOnlyList<SelectItem>)x));

        var derivedTable =
            from q in Parens(queryExpr)
            from a in alias
            select (TableReference)new DerivedTable(q, a);
        var namedTable =
            from n in qualifiedName
            from a in alias.OptionalOrDefault()
            select (TableReference)new NamedTable(n, a);
        var tablePrimary = derivedTable.Or(namedTable);

        var joinType = Parse.OneOf(
            kwInner.Value(JoinType.Inner),
            kwLeft.IgnoreThen(kwOuter.Optional()).Value(JoinType.Left),
            kwRight.IgnoreThen(kwOuter.Optional()).Value(JoinType.Right),
            kwFull.IgnoreThen(kwOuter.Optional()).Value(JoinType.Full));
        var joinTypeOrInner = joinType.OptionalOrDefault(JoinType.Inner);
        var joinSpecification = kwOn.IgnoreThen(expr).Select(e => (JoinCondition)new OnCondition(e))
            .Or(kwUsing.IgnoreThen(Parens(identifier.AtLeastOnceDelimitedBy(comma)))
                .Select(c => (JoinCondition)new UsingCondition(c)));

        var crossJoin =
            from _ in kwCross
            from __ in kwJoin
            from right in tablePrimary
            select (Func<TableReference, TableReference>)(l => new JoinedTable(l, JoinType.Cross, false, right, null));
        var naturalJoin =
            from _ in kwNatural
            from type in joinTypeOrInner
            from __ in kwJoin
            from right in tablePrimary
            select (Func<TableReference, TableReference>)(l => new JoinedTable(l, type, true, right, null));
        var qualifiedJoin =
            from type in joinTypeOrInner
            from _ in kwJoin
            from right in tablePrimary
            from condition in joinSpecification
            select (Func<TableReference, TableReference>)(l => new JoinedTable(l, type, false, right, condition));
        var tableReference =
            from first in tablePrimary
            from joins in crossJoin.Or(naturalJoin).Or(qualifiedJoin).Many()
            select joins.Aggregate(first, (tableRef, currentJoin) => currentJoin(tableRef));

        var querySpecification =
            from _ in kwSelect
            from q in setQuantifier
            from items in selectList
            from __ in kwFrom
            from tables in tableReference.AtLeastOnceDelimitedBy(comma)
            from condition in kwWhere.IgnoreThen(expr).OptionalOrDefault()
            from groupBy in kwGroup.IgnoreThen(kwBy).IgnoreThen(expr.AtLeastOnceDelimitedBy(comma)).OptionalOrDefault()
            from having in kwHaving.IgnoreThen(expr).OptionalOrDefault()
            select (QueryExpression)new QuerySpecification(q, items, tables, condition, groupBy ?? NoExpressions, having);

        var allOpt = kwAll.Value(true).OptionalOrDefault(false);
        var queryPrimary = querySpecification.Or(Parens(queryExpr));

        TokenListParser<SqlToken, QueryExpression> QueryTerm(TokenListParser<SqlToken, QueryExpression> first) =>
            from f in first
            from rest in (from _ in kwIntersect
                          from all in allOpt
                          from right in queryPrimary
                          select (All: all, Right: right)).Many()
            select rest.Aggregate(f, (l, x) => (QueryExpression)new SetOperation(l, SetOperator.Intersect, x.All, x.Right));

        var queryTerm = QueryTerm(queryPrimary);
        var unionOperator = kwUnion.Value(SetOperator.Union).Or(kwExcept.Value(SetOperator.Except));

        TokenListParser<SqlToken, QueryExpression> QueryExpr(TokenListParser<SqlToken, QueryExpression> firstTerm) =>
            from f in firstTerm
            from rest in (from op in unionOperator
                          from all in allOpt
                          from right in queryTerm
                          select (Op: op, All: all, Right: right)).Many()
            select rest.Aggregate(f, (l, x) => (QueryExpression)new SetOperation(l, x.Op, x.All, x.Right));

        queryExpression = QueryExpr(queryTerm);
        // Subqueries inside expressions must start with SELECT (see README).
        subqueryExpression = QueryExpr(QueryTerm(querySpecification));

        // ------------------------------------------------------------ statement

        var direction = kwAsc.Value(SortDirection.Ascending)
            .Or(kwDesc.Value(SortDirection.Descending))
            .OptionalOrDefault(SortDirection.Ascending);
        var orderItem =
            from e in expr
            from d in direction
            select new OrderByItem(e, d);
        var orderBy = kwOrder.IgnoreThen(kwBy).IgnoreThen(orderItem.AtLeastOnceDelimitedBy(comma))
            .Select(x => (IReadOnlyList<OrderByItem>)x)
            .OptionalOrDefault(Array.Empty<OrderByItem>());

        var statement =
            from q in queryExpr
            from o in orderBy
            from _ in Token.EqualTo(SqlToken.Semicolon).Optional()
            select new SelectStatement(q, o);

        return statement.AtEnd();
    }
}
