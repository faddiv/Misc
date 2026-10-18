using System.Globalization;
using Farkle;
using Farkle.Builder;
using Farkle.Parser;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

/// <summary>
/// The same LALR(1) grammar as the Irony implementation, expressed with Farkle's builder.
/// Keywords are literals; Farkle gives literals priority over the identifier regex, which makes them reserved.
/// </summary>
internal static class FarkleGrammar
{
    private static readonly Expression[] NoExpressions = [];
    private static readonly OrderByItem[] NoOrderBy = [];
    private static readonly int[] NoTypeParameters = [];

    public static readonly CharParser<SelectStatement> Parser = Build();

    private readonly record struct FunctionArguments(SetQuantifier Quantifier, IReadOnlyList<Expression> Arguments, bool IsStar);

    private static CharParser<SelectStatement> Build()
    {
        // ------------------------------------------------------------ terminals

        var identifier = Terminal.Create(
            "Identifier",
            Regex.FromRegexString("[A-Za-z_][A-Za-z0-9_]*"),
            (ref ParserState _, ReadOnlySpan<char> chars) => chars.ToString());
        var quotedIdentifier = Terminal.Create(
            "QuotedIdentifier",
            Regex.FromRegexString("\"([^\"]|\"\")+\""),
            (ref ParserState _, ReadOnlySpan<char> chars) => Unquote(chars, '"'));
        var stringLiteral = Terminal.Create(
            "String",
            Regex.FromRegexString("'([^']|'')*'"),
            (ref ParserState _, ReadOnlySpan<char> chars) => Unquote(chars, '\''));
        var number = Terminal.Create(
            "Number",
            Regex.FromRegexString(@"([0-9]+(\.[0-9]*)?|\.[0-9]+)([eE][+\-]?[0-9]+)?"),
            (ref ParserState _, ReadOnlySpan<char> chars) => chars.ToString());

        var empty = ProductionBuilder.Empty;

        // ------------------------------------------------------------ forward declarations

        var expr = Nonterminal.Create<Expression>("Expression");
        var queryExpr = Nonterminal.Create<QueryExpression>("QueryExpression");
        var subqueryExpr = Nonterminal.Create<QueryExpression>("SubqueryExpression");

        var exprList = expr.SeparatedBy<Expression, List<Expression>>(Terminal.Literal(","), atLeastOnce: true);

        var subquery = Nonterminal.Create("Subquery",
            "(".Appended().Extend(subqueryExpr).Append(")").AsProduction());

        // ------------------------------------------------------------ names

        var name = Nonterminal.Create("Name",
            identifier.AsProduction(),
            quotedIdentifier.AsProduction());
        var qualifiedName = name.SeparatedBy<string, List<string>>(Terminal.Literal("."), atLeastOnce: true);

        var aliasOpt = Nonterminal.Create<string?>("AliasOpt",
            empty.FinishConstant<string?>(null),
            name.Extended().Finish(n => (string?)n),
            "AS".Appended().Extend(name).Finish(n => (string?)n));

        var quantOpt = Nonterminal.Create("QuantifierOpt",
            empty.FinishConstant(SetQuantifier.None),
            "DISTINCT".Appended().FinishConstant(SetQuantifier.Distinct),
            "ALL".Appended().FinishConstant(SetQuantifier.All));

        var notOpt = Nonterminal.Create("NotOpt",
            empty.FinishConstant(false),
            "NOT".Appended().FinishConstant(true));

        // ------------------------------------------------------------ expressions

        var compOp = Nonterminal.Create("ComparisonOperator",
            "=".Appended().FinishConstant(BinaryOperator.Equal),
            "<>".Appended().FinishConstant(BinaryOperator.NotEqual),
            "<".Appended().FinishConstant(BinaryOperator.LessThan),
            ">".Appended().FinishConstant(BinaryOperator.GreaterThan),
            "<=".Appended().FinishConstant(BinaryOperator.LessThanOrEqual),
            ">=".Appended().FinishConstant(BinaryOperator.GreaterThanOrEqual));
        var quantifier = Nonterminal.Create("Quantifier",
            "ANY".Appended().FinishConstant(Quantifier.Any),
            "SOME".Appended().FinishConstant(Quantifier.Some),
            "ALL".Appended().FinishConstant(Quantifier.All));
        var addOp = Nonterminal.Create("AdditiveOperator",
            "+".Appended().FinishConstant(BinaryOperator.Add),
            "-".Appended().FinishConstant(BinaryOperator.Subtract),
            "||".Appended().FinishConstant(BinaryOperator.Concat));
        var mulOp = Nonterminal.Create("MultiplicativeOperator",
            "*".Appended().FinishConstant(BinaryOperator.Multiply),
            "/".Appended().FinishConstant(BinaryOperator.Divide));

        var functionArguments = Nonterminal.Create("FunctionArguments",
            empty.FinishConstant(new FunctionArguments(SetQuantifier.None, NoExpressions, false)),
            "*".Appended().FinishConstant(new FunctionArguments(SetQuantifier.None, NoExpressions, true)),
            quantOpt.Extended().Extend(exprList).Finish((q, args) => new FunctionArguments(q, args, false)));

        var whenClause = Nonterminal.Create("WhenClause",
            "WHEN".Appended().Extend(expr).Append("THEN").Extend(expr).Finish((c, r) => new WhenClause(c, r)));
        var whenList = whenClause.Many<WhenClause, List<WhenClause>>(atLeastOnce: true);
        var caseOperandOpt = Nonterminal.Create<Expression?>("CaseOperandOpt",
            empty.FinishConstant<Expression?>(null),
            expr.Extended().Finish(e => (Expression?)e));
        var elseOpt = Nonterminal.Create<Expression?>("ElseOpt",
            empty.FinishConstant<Expression?>(null),
            "ELSE".Appended().Extend(expr).Finish(e => (Expression?)e));
        var caseExpression = Nonterminal.Create<Expression>("CaseExpression",
            "CASE".Appended().Extend(caseOperandOpt).Extend(whenList).Extend(elseOpt).Append("END")
                .Finish((operand, whens, @else) => new CaseExpression(operand, whens, @else)));

        var integer = Nonterminal.Create("Integer",
            number.Extended().Finish(text => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var v)
                ? v
                : throw new SqlParseException($"Invalid type parameter '{text}'")));
        var typeWords = identifier.Many<string, List<string>>(atLeastOnce: true);
        var typeParamsOpt = Nonterminal.Create<IReadOnlyList<int>>("TypeParametersOpt",
            empty.FinishConstant<IReadOnlyList<int>>(NoTypeParameters),
            "(".Appended().Extend(integer.SeparatedBy<int, List<int>>(Terminal.Literal(","), atLeastOnce: true)).Append(")")
                .Finish(p => (IReadOnlyList<int>)p));
        var dataType = Nonterminal.Create("DataType",
            typeWords.Extended().Extend(typeParamsOpt)
                .Finish((words, p) => new DataType(string.Join(" ", words.Select(w => w.ToUpperInvariant())), p)));
        var castExpression = Nonterminal.Create<Expression>("CastExpression",
            "CAST".Appended().Append("(").Extend(expr).Append("AS").Extend(dataType).Append(")")
                .Finish((e, t) => new CastExpression(e, t)));

        var primary = Nonterminal.Create<Expression>("Primary",
            number.Extended().Finish(n => (Expression)new NumericLiteral(n)),
            stringLiteral.Extended().Finish(s => (Expression)new StringLiteral(s)),
            "NULL".Appended().FinishConstant<Expression>(NullLiteral.Instance),
            caseExpression.AsProduction(),
            castExpression.AsProduction(),
            "EXISTS".Appended().Extend(subquery).Finish(q => (Expression)new ExistsExpression(q)),
            qualifiedName.Extended().Append("(").Extend(functionArguments).Append(")")
                .Finish((n, a) => (Expression)new FunctionCall(string.Join(".", n), a.Quantifier, a.Arguments, a.IsStar)),
            qualifiedName.Extended().Finish(n => (Expression)new ColumnReference(n)),
            subquery.Extended().Finish(q => (Expression)new ScalarSubquery(q)),
            "(".Appended().Extend(expr).Append(")").AsProduction());

        var unary = Nonterminal.Create<Expression>("Unary");
        unary.SetProductions(
            primary.AsProduction(),
            "-".Appended().Extend(unary).Finish(e => (Expression)new UnaryExpression(UnaryOperator.Minus, e)),
            "+".Appended().Extend(unary).Finish(e => (Expression)new UnaryExpression(UnaryOperator.Plus, e)));

        var mulExpr = Nonterminal.Create<Expression>("Multiplicative");
        mulExpr.SetProductions(
            unary.AsProduction(),
            mulExpr.Extended().Extend(mulOp).Extend(unary).Finish((l, op, r) => (Expression)new BinaryExpression(op, l, r)));

        var addExpr = Nonterminal.Create<Expression>("Additive");
        addExpr.SetProductions(
            mulExpr.AsProduction(),
            addExpr.Extended().Extend(addOp).Extend(mulExpr).Finish((l, op, r) => (Expression)new BinaryExpression(op, l, r)));

        var escapeOpt = Nonterminal.Create<Expression?>("EscapeOpt",
            empty.FinishConstant<Expression?>(null),
            "ESCAPE".Appended().Extend(addExpr).Finish(e => (Expression?)e));

        var predicate = Nonterminal.Create<Expression>("Predicate",
            addExpr.AsProduction(),
            addExpr.Extended().Extend(compOp).Extend(addExpr)
                .Finish((l, op, r) => (Expression)new BinaryExpression(op, l, r)),
            addExpr.Extended().Extend(compOp).Extend(quantifier).Extend(subquery)
                .Finish((l, op, q, s) => (Expression)new QuantifiedComparison(l, op, q, s)),
            addExpr.Extended().Append("IS").Extend(notOpt).Append("NULL")
                .Finish((l, n) => (Expression)new IsNullExpression(l, n)),
            addExpr.Extended().Extend(notOpt).Append("BETWEEN").Extend(addExpr).Append("AND").Extend(addExpr)
                .Finish((l, n, lo, hi) => (Expression)new BetweenExpression(l, n, lo, hi)),
            addExpr.Extended().Extend(notOpt).Append("IN").Extend(subquery)
                .Finish((l, n, q) => (Expression)new InSubqueryExpression(l, n, q)),
            addExpr.Extended().Extend(notOpt).Append("IN").Append("(").Extend(exprList).Append(")")
                .Finish((l, n, values) => (Expression)new InListExpression(l, n, values)),
            addExpr.Extended().Extend(notOpt).Append("LIKE").Extend(addExpr).Extend(escapeOpt)
                .Finish((l, n, p, esc) => (Expression)new LikeExpression(l, n, p, esc)));

        var notExpr = Nonterminal.Create<Expression>("NotExpression");
        notExpr.SetProductions(
            "NOT".Appended().Extend(notExpr).Finish(e => (Expression)new UnaryExpression(UnaryOperator.Not, e)),
            predicate.AsProduction());

        var andExpr = Nonterminal.Create<Expression>("AndExpression");
        andExpr.SetProductions(
            notExpr.AsProduction(),
            andExpr.Extended().Append("AND").Extend(notExpr).Finish((l, r) => (Expression)new BinaryExpression(BinaryOperator.And, l, r)));

        expr.SetProductions(
            andExpr.AsProduction(),
            expr.Extended().Append("OR").Extend(andExpr).Finish((l, r) => (Expression)new BinaryExpression(BinaryOperator.Or, l, r)));

        // ------------------------------------------------------------ select list and FROM clause

        var selectItem = Nonterminal.Create<SelectItem>("SelectItem",
            expr.Extended().Extend(aliasOpt).Finish((e, a) => (SelectItem)new ExpressionItem(e, a)),
            qualifiedName.Extended().Append(".").Append("*").Finish(q => (SelectItem)new AllColumns(q)));
        var selectAll = (IReadOnlyList<SelectItem>)[new AllColumns([])];
        var selectList = Nonterminal.Create<IReadOnlyList<SelectItem>>("SelectList",
            "*".Appended().FinishConstant(selectAll),
            selectItem.SeparatedBy<SelectItem, List<SelectItem>>(Terminal.Literal(","), atLeastOnce: true)
                .Extended().Finish(items => (IReadOnlyList<SelectItem>)items));

        var derivedAlias = Nonterminal.Create("DerivedAlias",
            name.AsProduction(),
            "AS".Appended().Extend(name).AsProduction());
        var tablePrimary = Nonterminal.Create<TableReference>("TablePrimary",
            qualifiedName.Extended().Extend(aliasOpt).Finish((n, a) => (TableReference)new NamedTable(n, a)),
            "(".Appended().Extend(queryExpr).Append(")").Extend(derivedAlias)
                .Finish((q, a) => (TableReference)new DerivedTable(q, a)));

        var outerOpt = Nonterminal.CreateUntyped("OuterOpt", empty, "OUTER".Appended());
        var joinTypeOpt = Nonterminal.Create("JoinTypeOpt",
            empty.FinishConstant(JoinType.Inner),
            "INNER".Appended().FinishConstant(JoinType.Inner),
            "LEFT".Appended().Append(outerOpt).FinishConstant(JoinType.Left),
            "RIGHT".Appended().Append(outerOpt).FinishConstant(JoinType.Right),
            "FULL".Appended().Append(outerOpt).FinishConstant(JoinType.Full));
        var joinSpecification = Nonterminal.Create<JoinCondition>("JoinSpecification",
            "ON".Appended().Extend(expr).Finish(e => (JoinCondition)new OnCondition(e)),
            "USING".Appended().Append("(").Extend(name.SeparatedBy<string, List<string>>(Terminal.Literal(","), atLeastOnce: true)).Append(")")
                .Finish(c => (JoinCondition)new UsingCondition(c)));

        var tableReference = Nonterminal.Create<TableReference>("TableReference");
        tableReference.SetProductions(
            tablePrimary.AsProduction(),
            tableReference.Extended().Append("CROSS").Append("JOIN").Extend(tablePrimary)
                .Finish((l, r) => (TableReference)new JoinedTable(l, JoinType.Cross, false, r, null)),
            tableReference.Extended().Append("NATURAL").Extend(joinTypeOpt).Append("JOIN").Extend(tablePrimary)
                .Finish((l, t, r) => (TableReference)new JoinedTable(l, t, true, r, null)),
            tableReference.Extended().Extend(joinTypeOpt).Append("JOIN").Extend(tablePrimary).Extend(joinSpecification)
                .Finish((l, t, r, c) => (TableReference)new JoinedTable(l, t, false, r, c)));

        // ------------------------------------------------------------ queries

        var whereOpt = Nonterminal.Create<Expression?>("WhereOpt",
            empty.FinishConstant<Expression?>(null),
            "WHERE".Appended().Extend(expr).Finish(e => (Expression?)e));
        var groupByOpt = Nonterminal.Create<IReadOnlyList<Expression>>("GroupByOpt",
            empty.FinishConstant<IReadOnlyList<Expression>>(NoExpressions),
            "GROUP".Appended().Append("BY").Extend(exprList).Finish(l => (IReadOnlyList<Expression>)l));
        var havingOpt = Nonterminal.Create<Expression?>("HavingOpt",
            empty.FinishConstant<Expression?>(null),
            "HAVING".Appended().Extend(expr).Finish(e => (Expression?)e));

        var querySpecification = Nonterminal.Create<QueryExpression>("QuerySpecification",
            "SELECT".Appended().Extend(quantOpt).Extend(selectList)
                .Append("FROM").Extend(tableReference.SeparatedBy<TableReference, List<TableReference>>(Terminal.Literal(","), atLeastOnce: true))
                .Extend(whereOpt).Extend(groupByOpt).Extend(havingOpt)
                .Finish((q, items, from, where, groupBy, having) =>
                    (QueryExpression)new QuerySpecification(q, items, from, where, groupBy, having)));

        var allOpt = Nonterminal.Create("AllOpt",
            empty.FinishConstant(false),
            "ALL".Appended().FinishConstant(true));
        var unionOp = Nonterminal.Create("UnionOperator",
            "UNION".Appended().Extend(allOpt).Finish(all => (Operator: SetOperator.Union, All: all)),
            "EXCEPT".Appended().Extend(allOpt).Finish(all => (Operator: SetOperator.Except, All: all)));

        var queryPrimary = Nonterminal.Create("QueryPrimary",
            querySpecification.AsProduction(),
            "(".Appended().Extend(queryExpr).Append(")").AsProduction());

        var queryTerm = Nonterminal.Create<QueryExpression>("QueryTerm");
        queryTerm.SetProductions(
            queryPrimary.AsProduction(),
            queryTerm.Extended().Append("INTERSECT").Extend(allOpt).Extend(queryPrimary)
                .Finish((l, all, r) => (QueryExpression)new SetOperation(l, SetOperator.Intersect, all, r)));

        queryExpr.SetProductions(
            queryTerm.AsProduction(),
            queryExpr.Extended().Extend(unionOp).Extend(queryTerm)
                .Finish((l, op, r) => (QueryExpression)new SetOperation(l, op.Operator, op.All, r)));

        // Subqueries inside expressions must start with SELECT; this keeps "(" unambiguous in expressions.
        var subqueryTerm = Nonterminal.Create<QueryExpression>("SubqueryTerm");
        subqueryTerm.SetProductions(
            querySpecification.AsProduction(),
            subqueryTerm.Extended().Append("INTERSECT").Extend(allOpt).Extend(queryPrimary)
                .Finish((l, all, r) => (QueryExpression)new SetOperation(l, SetOperator.Intersect, all, r)));
        subqueryExpr.SetProductions(
            subqueryTerm.AsProduction(),
            subqueryExpr.Extended().Extend(unionOp).Extend(queryTerm)
                .Finish((l, op, r) => (QueryExpression)new SetOperation(l, op.Operator, op.All, r)));

        // ------------------------------------------------------------ statement

        var orderItem = Nonterminal.Create("OrderItem",
            expr.Extended().Finish(e => new OrderByItem(e, SortDirection.Ascending)),
            expr.Extended().Append("ASC").Finish(e => new OrderByItem(e, SortDirection.Ascending)),
            expr.Extended().Append("DESC").Finish(e => new OrderByItem(e, SortDirection.Descending)));
        var orderByOpt = Nonterminal.Create<IReadOnlyList<OrderByItem>>("OrderByOpt",
            empty.FinishConstant<IReadOnlyList<OrderByItem>>(NoOrderBy),
            "ORDER".Appended().Append("BY").Extend(orderItem.SeparatedBy<OrderByItem, List<OrderByItem>>(Terminal.Literal(","), atLeastOnce: true))
                .Finish(l => (IReadOnlyList<OrderByItem>)l));
        var semicolonOpt = Nonterminal.CreateUntyped("SemicolonOpt", empty, ";".Appended());

        var statement = Nonterminal.Create("Statement",
            queryExpr.Extended().Extend(orderByOpt).Append(semicolonOpt)
                .Finish((q, o) => new SelectStatement(q, o)));

        var parser = statement
            .CaseSensitive(false)
            .AddLineComment("--")
            .Build();

        if (parser.IsFailing)
            throw new InvalidOperationException("Farkle grammar failed to build: " + parser.Parse("").Error);

        return parser;
    }

    private static string Unquote(ReadOnlySpan<char> chars, char quote) =>
        chars[1..^1].ToString().Replace(new string(quote, 2), quote.ToString());
}
