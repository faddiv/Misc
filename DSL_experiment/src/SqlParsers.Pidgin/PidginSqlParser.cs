using System.Globalization;
using Pidgin;
using SqlParsers.Ast;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace SqlParsers.Pidgin;

/// <summary>Scannerless parser built with Pidgin combinators.</summary>
public sealed class PidginSqlParser : ISqlParser
{
    public string Name => "Pidgin";

    public SelectStatement Parse(string sql)
    {
        var result = PidginGrammar.Statement.Parse(sql);
        return result.Success
            ? result.Value
            : throw new SqlParseException(result.Error?.ToString() ?? "Invalid SQL");
    }
}

internal static class PidginGrammar
{
    // Declared before Statement: static fields initialize in textual order and Build() uses it.
    private static readonly Expression[] NoExpressions = [];

    public static readonly Parser<char, SelectStatement> Statement = Build();

    private static Parser<char, SelectStatement> Build()
    {
        // ------------------------------------------------------------ lexical

        var comment = Try(String("--")).Then(Token(c => c != '\r' && c != '\n').SkipMany());
        var skip = Whitespace.SkipAtLeastOnce().Or(comment).SkipMany();

        Parser<char, T> Tok<T>(Parser<char, T> p) => p.Before(skip);
        Parser<char, char> Sym(char c) => Tok(Char(c));
        Parser<char, string> Op(string s) => Tok(Try(String(s)));

        var identPart = LetterOrDigit.Or(Char('_'));
        var rawWord = Letter.Or(Char('_')).Then(identPart.ManyString(), (h, t) => h + t);

        Parser<char, Unit> Kw(string k) => Tok(Try(CIString(k).Then(Not(identPart))));

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

        var regularIdentifier = Tok(Try(rawWord.Assert(w => !SqlKeywords.IsReserved(w), "identifier")));
        var quotedIdentifier = Tok(
            Char('"')
                .Then(Token(c => c != '"').Or(Try(String("\"\"")).ThenReturn('"')).AtLeastOnceString())
                .Before(Char('"')));
        var identifier = quotedIdentifier.Or(regularIdentifier);

        var stringLiteral = Tok(
            Char('\'')
                .Then(Token(c => c != '\'').Or(Try(String("''")).ThenReturn('\'')).ManyString())
                .Before(Char('\'')));

        var digits = Digit.AtLeastOnceString();
        var sign = Char('+').Or(Char('-')).Select(c => c.ToString()).Or(Return(string.Empty));
        var exponent = Try(Map((e, s, d) => e + s + d, Char('e').Or(Char('E')).Select(c => c.ToString()), sign, digits));
        var mantissa = Map((i, f) => i + f, digits, Char('.').Then(Digit.ManyString(), (p, d) => p + d).Or(Return(string.Empty)))
            .Or(Try(Char('.').Then(digits, (p, d) => p + d)));
        var number = Tok(Map((m, e) => m + e, mantissa, exponent.Or(Return(string.Empty))));
        var integer = Tok(digits).Select(s => int.Parse(s, CultureInfo.InvariantCulture));

        var comma = Sym(',');
        var dot = Sym('.');
        var star = Sym('*');
        var lParen = Sym('(');
        var rParen = Sym(')');
        Parser<char, T> Parens<T>(Parser<char, T> p) => lParen.Then(p).Before(rParen);

        // ------------------------------------------------------------ forward references

        Parser<char, Expression> expression = null!;
        var expr = Rec(() => expression);
        Parser<char, QueryExpression> queryExpression = null!;
        var queryExpr = Rec(() => queryExpression);
        Parser<char, QueryExpression> subqueryExpression = null!;
        var subqueryExpr = Rec(() => subqueryExpression);
        var subquery = Parens(subqueryExpr);

        // ------------------------------------------------------------ expressions

        var setQuantifier = OneOf(
            kwDistinct.ThenReturn(SetQuantifier.Distinct),
            kwAll.ThenReturn(SetQuantifier.All),
            Return(SetQuantifier.None));

        var qualifiedName = identifier.SeparatedAtLeastOnce(dot).Select(x => (IReadOnlyList<string>)x.ToArray());

        var functionArguments = Parens(OneOf(
            star.ThenReturn((Quantifier: SetQuantifier.None, Args: (IReadOnlyList<Expression>)NoExpressions, IsStar: true)),
            Map((q, args) => (Quantifier: q, Args: (IReadOnlyList<Expression>)args.ToArray(), IsStar: false),
                setQuantifier, expr.SeparatedAtLeastOnce(comma)),
            Return((Quantifier: SetQuantifier.None, Args: (IReadOnlyList<Expression>)NoExpressions, IsStar: false))));

        var nameOrCall = Map(
            (name, args) => args.HasValue
                ? (Expression)new FunctionCall(string.Join(".", name), args.Value.Quantifier, args.Value.Args, args.Value.IsStar)
                : new ColumnReference(name),
            qualifiedName, functionArguments.Optional());

        var whenClause = Map((c, r) => new WhenClause(c, r), kwWhen.Then(expr), kwThen.Then(expr));
        var caseExpression = kwCase.Then(Map(
                (operand, whens, @else) => (Expression)new CaseExpression(
                    operand.HasValue ? operand.Value : null,
                    whens.ToArray(),
                    @else.HasValue ? @else.Value : null),
                expr.Optional(), whenClause.AtLeastOnce(), kwElse.Then(expr).Optional()))
            .Before(kwEnd);

        var dataType = Map(
            (words, args) => new DataType(
                string.Join(" ", words.Select(w => w.ToUpperInvariant())),
                args.HasValue ? args.Value.ToArray() : Array.Empty<int>()),
            regularIdentifier.AtLeastOnce(), Parens(integer.SeparatedAtLeastOnce(comma)).Optional());
        var castExpression = kwCast.Then(Parens(Map((e, t) => (Expression)new CastExpression(e, t), expr.Before(kwAs), dataType)));

        var existsExpression = kwExists.Then(subquery).Select(q => (Expression)new ExistsExpression(q));

        // "(" starts either a scalar subquery (must begin with SELECT) or a parenthesized expression.
        var parenthesized = Parens(OneOf(subqueryExpr.Select(q => (Expression)new ScalarSubquery(q)), expr));

        var primary = OneOf(
            number.Select(n => (Expression)new NumericLiteral(n)),
            stringLiteral.Select(s => (Expression)new StringLiteral(s)),
            kwNull.ThenReturn((Expression)NullLiteral.Instance),
            caseExpression,
            castExpression,
            existsExpression,
            parenthesized,
            nameOrCall);

        Parser<char, Expression> unary = null!;
        var unaryRef = Rec(() => unary);
        unary = OneOf(
            Sym('-').Then(unaryRef).Select(e => (Expression)new UnaryExpression(UnaryOperator.Minus, e)),
            Sym('+').Then(unaryRef).Select(e => (Expression)new UnaryExpression(UnaryOperator.Plus, e)),
            primary);

        var multiplicativeOperator = OneOf(
            star.ThenReturn(BinaryOperator.Multiply),
            Sym('/').ThenReturn(BinaryOperator.Divide));
        var additiveOperator = OneOf(
            Op("||").ThenReturn(BinaryOperator.Concat),
            Sym('+').ThenReturn(BinaryOperator.Add),
            Sym('-').ThenReturn(BinaryOperator.Subtract));
        var comparisonOperator = OneOf(
            Op("<>").ThenReturn(BinaryOperator.NotEqual),
            Op("<=").ThenReturn(BinaryOperator.LessThanOrEqual),
            Op(">=").ThenReturn(BinaryOperator.GreaterThanOrEqual),
            Op("=").ThenReturn(BinaryOperator.Equal),
            Op("<").ThenReturn(BinaryOperator.LessThan),
            Op(">").ThenReturn(BinaryOperator.GreaterThan));

        var multiplicative = LeftAssociative(unaryRef, multiplicativeOperator);
        var additive = LeftAssociative(multiplicative, additiveOperator);

        var quantifier = OneOf(
            kwAny.ThenReturn(Quantifier.Any),
            kwSome.ThenReturn(Quantifier.Some),
            kwAll.ThenReturn(Quantifier.All));

        var comparisonSuffix = comparisonOperator.Bind(op => OneOf(
            Map((q, s) => (Func<Expression, Expression>)(l => new QuantifiedComparison(l, op, q, s)), quantifier, subquery),
            additive.Select(r => (Func<Expression, Expression>)(l => new BinaryExpression(op, l, r)))));

        var notOpt = kwNot.ThenReturn(true).Or(Return(false));

        var isNullSuffix = kwIs.Then(Map(
            (negated, _) => (Func<Expression, Expression>)(l => new IsNullExpression(l, negated)),
            notOpt, kwNull));

        var betweenTail = kwBetween.Then(Map(
            (lo, hi) => (Func<bool, Func<Expression, Expression>>)(n => l => new BetweenExpression(l, n, lo, hi)),
            additive, kwAnd.Then(additive)));
        var inTail = kwIn.Then(Parens(OneOf(
            subqueryExpr.Select(q => (Func<bool, Func<Expression, Expression>>)(n => l => new InSubqueryExpression(l, n, q))),
            expr.SeparatedAtLeastOnce(comma).Select(items =>
            {
                var values = items.ToArray();
                return (Func<bool, Func<Expression, Expression>>)(n => l => new InListExpression(l, n, values));
            }))));
        var likeTail = kwLike.Then(Map(
            (pattern, escape) => (Func<bool, Func<Expression, Expression>>)(n => l =>
                new LikeExpression(l, n, pattern, escape.HasValue ? escape.Value : null)),
            additive, kwEscape.Then(additive).Optional()));
        var negatableSuffix = Map((negated, tail) => tail(negated), notOpt, OneOf(betweenTail, inTail, likeTail));

        var predicate = Map(
            (left, suffix) => suffix.HasValue ? suffix.Value(left) : left,
            additive, OneOf(comparisonSuffix, isNullSuffix, negatableSuffix).Optional());

        Parser<char, Expression> notExpression = null!;
        var notRef = Rec(() => notExpression);
        notExpression = kwNot.Then(notRef).Select(e => (Expression)new UnaryExpression(UnaryOperator.Not, e)).Or(predicate);

        var andExpression = LeftAssociative(notRef, kwAnd.ThenReturn(BinaryOperator.And));
        expression = LeftAssociative(andExpression, kwOr.ThenReturn(BinaryOperator.Or));

        // ------------------------------------------------------------ queries

        var alias = kwAs.Then(identifier).Or(identifier);

        var qualifiedStar = Try(identifier.Before(dot).AtLeastOnce().Before(star))
            .Select(q => (SelectItem)new AllColumns(q.ToArray()));
        var expressionItem = Map(
            (e, a) => (SelectItem)new ExpressionItem(e, a.HasValue ? a.Value : null),
            expr, alias.Optional());
        var selectAll = (IReadOnlyList<SelectItem>)[new AllColumns([])];
        var selectList = OneOf(
            star.ThenReturn(selectAll),
            OneOf(qualifiedStar, expressionItem).SeparatedAtLeastOnce(comma).Select(x => (IReadOnlyList<SelectItem>)x.ToArray()));

        var derivedTable = Map((q, a) => (TableReference)new DerivedTable(q, a), Parens(queryExpr), alias);
        var namedTable = Map(
            (n, a) => (TableReference)new NamedTable(n, a.HasValue ? a.Value : null),
            qualifiedName, alias.Optional());
        var tablePrimary = OneOf(derivedTable, namedTable);

        var joinType = OneOf(
            kwInner.ThenReturn(JoinType.Inner),
            kwLeft.Then(kwOuter.Optional()).ThenReturn(JoinType.Left),
            kwRight.Then(kwOuter.Optional()).ThenReturn(JoinType.Right),
            kwFull.Then(kwOuter.Optional()).ThenReturn(JoinType.Full));
        var joinTypeOrInner = joinType.Or(Return(JoinType.Inner));
        var joinSpecification = OneOf(
            kwOn.Then(expr).Select(e => (JoinCondition)new OnCondition(e)),
            kwUsing.Then(Parens(identifier.SeparatedAtLeastOnce(comma)))
                .Select(c => (JoinCondition)new UsingCondition(c.ToArray())));

        var crossJoin = kwCross.Then(kwJoin).Then(tablePrimary)
            .Select(r => (Func<TableReference, TableReference>)(l => new JoinedTable(l, JoinType.Cross, false, r, null)));
        var naturalJoin = kwNatural.Then(Map(
            (t, r) => (Func<TableReference, TableReference>)(l => new JoinedTable(l, t, true, r, null)),
            joinTypeOrInner, kwJoin.Then(tablePrimary)));
        var qualifiedJoin = Map(
            (t, r, c) => (Func<TableReference, TableReference>)(l => new JoinedTable(l, t, false, r, c)),
            joinTypeOrInner, kwJoin.Then(tablePrimary), joinSpecification);
        var tableReference = Map(
            (first, joins) => joins.Aggregate(first, (l, join) => join(l)),
            tablePrimary, OneOf(crossJoin, naturalJoin, qualifiedJoin).Many());

        var querySpecification = Map(
            (q, items, from, where, groupBy, having) => (QueryExpression)new QuerySpecification(
                q,
                items,
                from.ToArray(),
                where.HasValue ? where.Value : null,
                groupBy.HasValue ? groupBy.Value.ToArray() : NoExpressions,
                having.HasValue ? having.Value : null),
            kwSelect.Then(setQuantifier),
            selectList,
            kwFrom.Then(tableReference.SeparatedAtLeastOnce(comma)),
            kwWhere.Then(expr).Optional(),
            kwGroup.Then(kwBy).Then(expr.SeparatedAtLeastOnce(comma)).Optional(),
            kwHaving.Then(expr).Optional());

        var allOpt = kwAll.ThenReturn(true).Or(Return(false));
        var queryPrimary = querySpecification.Or(Parens(queryExpr));

        Parser<char, QueryExpression> QueryTerm(Parser<char, QueryExpression> first) => Map(
            (f, rest) => rest.Aggregate(f, (l, x) => (QueryExpression)new SetOperation(l, SetOperator.Intersect, x.All, x.Right)),
            first,
            kwIntersect.Then(Map((all, right) => (All: all, Right: right), allOpt, queryPrimary)).Many());

        var queryTerm = QueryTerm(queryPrimary);
        var unionOperator = kwUnion.ThenReturn(SetOperator.Union).Or(kwExcept.ThenReturn(SetOperator.Except));

        Parser<char, QueryExpression> QueryExpr(Parser<char, QueryExpression> firstTerm) => Map(
            (f, rest) => rest.Aggregate(f, (l, x) => (QueryExpression)new SetOperation(l, x.Op, x.All, x.Right)),
            firstTerm,
            Map((op, all, right) => (Op: op, All: all, Right: right), unionOperator, allOpt, queryTerm).Many());

        queryExpression = QueryExpr(queryTerm);
        // Subqueries inside expressions must start with SELECT (see README).
        subqueryExpression = QueryExpr(QueryTerm(querySpecification));

        // ------------------------------------------------------------ statement

        var direction = OneOf(
            kwAsc.ThenReturn(SortDirection.Ascending),
            kwDesc.ThenReturn(SortDirection.Descending),
            Return(SortDirection.Ascending));
        var orderItem = Map((e, d) => new OrderByItem(e, d), expr, direction);
        var orderBy = kwOrder.Then(kwBy).Then(orderItem.SeparatedAtLeastOnce(comma))
            .Select(x => (IReadOnlyList<OrderByItem>)x.ToArray())
            .Or(Return((IReadOnlyList<OrderByItem>)Array.Empty<OrderByItem>()));

        return skip
            .Then(Map((q, o) => new SelectStatement(q, o), queryExpr, orderBy))
            .Before(Sym(';').Optional())
            .Before(End);
    }

    private static Parser<char, Expression> LeftAssociative(Parser<char, Expression> operand, Parser<char, BinaryOperator> op) =>
        Map(
            (first, rest) => rest.Aggregate(first, (l, x) => (Expression)new BinaryExpression(x.Op, l, x.Right)),
            operand,
            op.Then(operand, (o, r) => (Op: o, Right: r)).Many());
}
