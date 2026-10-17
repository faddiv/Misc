using System.Globalization;
using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

/// <summary>Scannerless, fully backtracking parser built with Sprache combinators.</summary>
public sealed class SpracheSqlParser : ISqlParser
{
    public string Name => "Sprache";

    public SelectStatement Parse(string sql)
    {
        var result = SpracheGrammar.Statement.TryParse(sql);
        return result.WasSuccessful ? result.Value : throw new SqlParseException(result.ToString());
    }
}

internal static class SpracheGrammar
{
    private static readonly IReadOnlyList<Expression> NoExpressions = Array.Empty<Expression>();

    public static readonly Parser<SelectStatement> Statement = Build();

    private static Parser<SelectStatement> Build()
    {
        // ------------------------------------------------------------ lexical

        var comment =
            from _ in Parse.String("--")
            from __ in Parse.CharExcept(c => c == '\r' || c == '\n', "end of line").Many()
            select string.Empty;
        var skip = Parse.WhiteSpace.AtLeastOnce().Return(string.Empty).Or(comment).Many();

        Parser<T> Tok<T>(Parser<T> p) =>
            from value in p
            from _ in skip
            select value;
        Parser<char> Sym(char c) => Tok(Parse.Char(c));
        Parser<string> Op(string s) => Tok(Parse.String(s).Text());

        var identPart = Parse.LetterOrDigit.Or(Parse.Char('_'));
        var rawWord =
            from head in Parse.Letter.Or(Parse.Char('_'))
            from tail in identPart.Many().Text()
            select head + tail;

        Parser<string> Kw(string k) => Tok(
            from _ in Parse.IgnoreCase(k)
            from __ in identPart.Not()
            select k);

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

        var regularIdentifier = Tok(rawWord.Where(w => !SqlKeywords.IsReserved(w)));
        var quotedIdentifier = Tok(
            from _ in Parse.Char('"')
            from chars in Parse.CharExcept('"').Or(Parse.String("\"\"").Return('"')).AtLeastOnce().Text()
            from __ in Parse.Char('"')
            select chars);
        var identifier = quotedIdentifier.Or(regularIdentifier);

        var stringLiteral = Tok(
            from _ in Parse.Char('\'')
            from chars in Parse.CharExcept('\'').Or(Parse.String("''").Return('\'')).Many().Text()
            from __ in Parse.Char('\'')
            select chars);

        var digits = Parse.Digit.AtLeastOnce().Text();
        var fraction =
            from _ in Parse.Char('.')
            from d in Parse.Digit.Many().Text()
            select "." + d;
        var exponent =
            from e in Parse.Chars('e', 'E')
            from s in Parse.Chars('+', '-').Optional()
            from d in digits
            select e + (s.IsDefined ? s.Get().ToString() : string.Empty) + d;
        var mantissa =
            (from i in digits
             from f in fraction.Optional()
             select i + f.GetOrElse(string.Empty))
            .Or(from _ in Parse.Char('.')
                from d in digits
                select "." + d);
        var number = Tok(
            from m in mantissa
            from e in exponent.Optional()
            select m + e.GetOrElse(string.Empty));
        var integer = Tok(digits).Select(s => int.Parse(s, CultureInfo.InvariantCulture));

        var comma = Sym(',');
        var dot = Sym('.');
        var star = Sym('*');
        var lParen = Sym('(');
        var rParen = Sym(')');
        Parser<T> Parens<T>(Parser<T> p) =>
            from _ in lParen
            from value in p
            from __ in rParen
            select value;

        // ------------------------------------------------------------ forward references

        Parser<Expression> expression = null!;
        var expr = Parse.Ref(() => expression);
        Parser<QueryExpression> queryExpression = null!;
        var queryExpr = Parse.Ref(() => queryExpression);
        Parser<QueryExpression> subqueryExpression = null!;
        var subqueryExpr = Parse.Ref(() => subqueryExpression);
        var subquery = Parens(subqueryExpr);

        // ------------------------------------------------------------ expressions

        var setQuantifier = kwDistinct.Return(SetQuantifier.Distinct)
            .Or(kwAll.Return(SetQuantifier.All))
            .Or(Parse.Return(SetQuantifier.None));

        var qualifiedName = identifier.DelimitedBy(dot).Select(x => (IReadOnlyList<string>)x.ToArray());

        var functionArguments = Parens(
            star.Return((Quantifier: SetQuantifier.None, Args: NoExpressions, IsStar: true))
                .Or(from q in setQuantifier
                    from args in expr.DelimitedBy(comma)
                    select (Quantifier: q, Args: (IReadOnlyList<Expression>)args.ToArray(), IsStar: false))
                .Or(Parse.Return((Quantifier: SetQuantifier.None, Args: NoExpressions, IsStar: false))));

        var nameOrCall =
            from name in qualifiedName
            from args in functionArguments.Optional()
            select args.IsDefined
                ? (Expression)new FunctionCall(string.Join(".", name), args.Get().Quantifier, args.Get().Args, args.Get().IsStar)
                : new ColumnReference(name);

        var whenClause =
            from _ in kwWhen
            from condition in expr
            from __ in kwThen
            from result in expr
            select new WhenClause(condition, result);
        var caseExpression =
            from _ in kwCase
            from operand in expr.Optional()
            from whens in whenClause.AtLeastOnce()
            from @else in kwElse.Then(k => expr).Optional()
            from __ in kwEnd
            select (Expression)new CaseExpression(operand.GetOrDefault(), whens.ToArray(), @else.GetOrDefault());

        var dataType =
            from words in regularIdentifier.AtLeastOnce()
            from args in Parens(integer.DelimitedBy(comma)).Optional()
            select new DataType(
                string.Join(" ", words.Select(w => w.ToUpperInvariant())),
                args.IsDefined ? args.Get().ToArray() : Array.Empty<int>());
        var castExpression =
            from _ in kwCast
            from cast in Parens(
                from operand in expr
                from __ in kwAs
                from type in dataType
                select new CastExpression(operand, type))
            select (Expression)cast;

        var existsExpression =
            from _ in kwExists
            from q in subquery
            select (Expression)new ExistsExpression(q);

        // "(" starts either a scalar subquery (must begin with SELECT) or a parenthesized expression.
        var parenthesized = Parens(subqueryExpr.Select(q => (Expression)new ScalarSubquery(q)).Or(expr));

        var primary = number.Select(n => (Expression)new NumericLiteral(n))
            .Or(stringLiteral.Select(s => (Expression)new StringLiteral(s)))
            .Or(kwNull.Return((Expression)NullLiteral.Instance))
            .Or(caseExpression)
            .Or(castExpression)
            .Or(existsExpression)
            .Or(parenthesized)
            .Or(nameOrCall);

        Parser<Expression> unary = null!;
        var unaryRef = Parse.Ref(() => unary);
        unary = (from _ in Sym('-') from e in unaryRef select (Expression)new UnaryExpression(UnaryOperator.Minus, e))
            .Or(from _ in Sym('+') from e in unaryRef select (Expression)new UnaryExpression(UnaryOperator.Plus, e))
            .Or(primary);

        var multiplicativeOperator = star.Return(BinaryOperator.Multiply)
            .Or(Sym('/').Return(BinaryOperator.Divide));
        var additiveOperator = Op("||").Return(BinaryOperator.Concat)
            .Or(Sym('+').Return(BinaryOperator.Add))
            .Or(Sym('-').Return(BinaryOperator.Subtract));
        var comparisonOperator = Op("<>").Return(BinaryOperator.NotEqual)
            .Or(Op("<=").Return(BinaryOperator.LessThanOrEqual))
            .Or(Op(">=").Return(BinaryOperator.GreaterThanOrEqual))
            .Or(Op("=").Return(BinaryOperator.Equal))
            .Or(Op("<").Return(BinaryOperator.LessThan))
            .Or(Op(">").Return(BinaryOperator.GreaterThan));

        Parser<Expression> LeftAssociative(Parser<Expression> operand, Parser<BinaryOperator> op) =>
            Parse.ChainOperator(op, operand, (o, l, r) => (Expression)new BinaryExpression(o, l, r));

        var multiplicative = LeftAssociative(unaryRef, multiplicativeOperator);
        var additive = LeftAssociative(multiplicative, additiveOperator);

        var quantifier = kwAny.Return(Quantifier.Any)
            .Or(kwSome.Return(Quantifier.Some))
            .Or(kwAll.Return(Quantifier.All));

        var comparisonSuffix =
            from op in comparisonOperator
            from suffix in
                (from q in quantifier
                 from s in subquery
                 select (Func<Expression, Expression>)(l => new QuantifiedComparison(l, op, q, s)))
                .Or(additive.Select(r => (Func<Expression, Expression>)(l => new BinaryExpression(op, l, r))))
            select suffix;

        var notOpt = kwNot.Return(true).Or(Parse.Return(false));

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
        var inTail =
            from _ in kwIn
            from tail in Parens(
                subqueryExpr.Select(q => (Func<bool, Func<Expression, Expression>>)(n => l => new InSubqueryExpression(l, n, q)))
                    .Or(expr.DelimitedBy(comma).Select(items =>
                    {
                        var values = items.ToArray();
                        return (Func<bool, Func<Expression, Expression>>)(n => l => new InListExpression(l, n, values));
                    })))
            select tail;
        var likeTail =
            from _ in kwLike
            from pattern in additive
            from escape in kwEscape.Then(k => additive).Optional()
            select (Func<bool, Func<Expression, Expression>>)(n => l => new LikeExpression(l, n, pattern, escape.GetOrDefault()));
        var negatableSuffix =
            from negated in notOpt
            from tail in betweenTail.Or(inTail).Or(likeTail)
            select tail(negated);

        var predicate =
            from left in additive
            from suffix in comparisonSuffix.Or(isNullSuffix).Or(negatableSuffix).Optional()
            select suffix.IsDefined ? suffix.Get()(left) : left;

        Parser<Expression> notExpression = null!;
        var notRef = Parse.Ref(() => notExpression);
        notExpression = (from _ in kwNot from e in notRef select (Expression)new UnaryExpression(UnaryOperator.Not, e))
            .Or(predicate);

        var andExpression = LeftAssociative(notRef, kwAnd.Return(BinaryOperator.And));
        expression = LeftAssociative(andExpression, kwOr.Return(BinaryOperator.Or));

        // ------------------------------------------------------------ queries

        var alias = kwAs.Then(_ => identifier).Or(identifier);

        var qualifiedStar =
            from parts in (from id in identifier from d in dot select id).AtLeastOnce()
            from _ in star
            select (SelectItem)new AllColumns(parts.ToArray());
        var expressionItem =
            from e in expr
            from a in alias.Optional()
            select (SelectItem)new ExpressionItem(e, a.GetOrDefault());
        var selectAll = (IReadOnlyList<SelectItem>)[new AllColumns([])];
        var selectList = star.Return(selectAll)
            .Or(qualifiedStar.Or(expressionItem).DelimitedBy(comma).Select(x => (IReadOnlyList<SelectItem>)x.ToArray()));

        var derivedTable =
            from q in Parens(queryExpr)
            from a in alias
            select (TableReference)new DerivedTable(q, a);
        var namedTable =
            from n in qualifiedName
            from a in alias.Optional()
            select (TableReference)new NamedTable(n, a.GetOrDefault());
        var tablePrimary = derivedTable.Or(namedTable);

        var joinType = kwInner.Return(JoinType.Inner)
            .Or(from _ in kwLeft from __ in kwOuter.Optional() select JoinType.Left)
            .Or(from _ in kwRight from __ in kwOuter.Optional() select JoinType.Right)
            .Or(from _ in kwFull from __ in kwOuter.Optional() select JoinType.Full);
        var joinTypeOrInner = joinType.Or(Parse.Return(JoinType.Inner));
        var joinSpecification =
            (from _ in kwOn from e in expr select (JoinCondition)new OnCondition(e))
            .Or(from _ in kwUsing
                from columns in Parens(identifier.DelimitedBy(comma))
                select (JoinCondition)new UsingCondition(columns.ToArray()));

        var crossJoin =
            from _ in kwCross
            from __ in kwJoin
            from right in tablePrimary
            select (Func<TableReference, TableReference>)(tableRef => new JoinedTable(tableRef, JoinType.Cross, false, right, null));
        var naturalJoin =
            from _ in kwNatural
            from type in joinTypeOrInner
            from __ in kwJoin
            from right in tablePrimary
            select (Func<TableReference, TableReference>)(tableRef => new JoinedTable(tableRef, type, true, right, null));
        var qualifiedJoin =
            from type in joinTypeOrInner
            from _ in kwJoin
            from right in tablePrimary
            from condition in joinSpecification
            select (Func<TableReference, TableReference>)(tableRef => new JoinedTable(tableRef, type, false, right, condition));
        var tableReference =
            from first in tablePrimary
            from joins in crossJoin.Or(naturalJoin).Or(qualifiedJoin).Many()
            select joins.Aggregate(first, (tableRef, currentJoin) => currentJoin(tableRef));

        var querySpecification =
            from _ in kwSelect
            from q in setQuantifier
            from items in selectList
            from __ in kwFrom
            from tables in tableReference.DelimitedBy(comma)
            from condition in kwWhere.Then(k => expr).Optional()
            from groupBy in (from g in kwGroup from b in kwBy from list in expr.DelimitedBy(comma) select list).Optional()
            from having in kwHaving.Then(k => expr).Optional()
            select (QueryExpression)new QuerySpecification(
                q,
                items,
                tables.ToArray(),
                condition.GetOrDefault(),
                groupBy.IsDefined ? groupBy.Get().ToArray() : NoExpressions,
                having.GetOrDefault());

        var allOpt = kwAll.Return(true).Or(Parse.Return(false));
        var queryPrimary = querySpecification.Or(Parens(queryExpr));

        Parser<QueryExpression> QueryTerm(Parser<QueryExpression> first) =>
            from f in first
            from rest in (from _ in kwIntersect
                          from all in allOpt
                          from right in queryPrimary
                          select (All: all, Right: right)).Many()
            select rest.Aggregate(f, (l, x) => (QueryExpression)new SetOperation(l, SetOperator.Intersect, x.All, x.Right));

        var queryTerm = QueryTerm(queryPrimary);
        var unionOperator = kwUnion.Return(SetOperator.Union).Or(kwExcept.Return(SetOperator.Except));

        Parser<QueryExpression> QueryExpr(Parser<QueryExpression> firstTerm) =>
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

        var direction = kwAsc.Return(SortDirection.Ascending)
            .Or(kwDesc.Return(SortDirection.Descending))
            .Or(Parse.Return(SortDirection.Ascending));
        var orderItem =
            from e in expr
            from d in direction
            select new OrderByItem(e, d);
        var orderBy =
            from _ in kwOrder
            from __ in kwBy
            from items in orderItem.DelimitedBy(comma)
            select (IReadOnlyList<OrderByItem>)items.ToArray();

        var statement =
            from _ in skip
            from q in queryExpr
            from o in orderBy.Optional()
            from __ in Sym(';').Optional()
            select new SelectStatement(q, o.GetOrElse(Array.Empty<OrderByItem>()));

        return statement.End();
    }
}
