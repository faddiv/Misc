using System.Globalization;
using SqlParsers.Ast;
using static SqlParsers.Antlr.Generated.Sql92SelectParser;

namespace SqlParsers.Antlr;

internal static partial class AntlrAstBuilder
{
    // expression : andExpression (OR andExpression)* ;
    private static Expression ToAst(ExpressionContext c)
    {
        var operands = c.andExpression();
        var result = ToAst(operands[0]);
        for (var i = 1; i < operands.Length; i++)
            result = new BinaryExpression(BinaryOperator.Or, result, ToAst(operands[i]));
        return result;
    }

    // andExpression : notExpression (AND notExpression)* ;
    private static Expression ToAst(AndExpressionContext c)
    {
        var operands = c.notExpression();
        var result = ToAst(operands[0]);
        for (var i = 1; i < operands.Length; i++)
            result = new BinaryExpression(BinaryOperator.And, result, ToAst(operands[i]));
        return result;
    }

    // notExpression : NOT notExpression | predicate ;
    private static Expression ToAst(NotExpressionContext c) =>
        c.NOT() is not null
            ? new UnaryExpression(UnaryOperator.Not, ToAst(c.notExpression()))
            : ToAst(c.predicate());

    // predicate : additiveExpression predicateSuffix? ;
    // predicateSuffix
    //     : comparisonOperator (ANY | SOME | ALL) subquery                     # quantifiedComparison
    //     | comparisonOperator additiveExpression                              # comparison
    //     | IS NOT? NULL                                                       # isNull
    //     | NOT? BETWEEN additiveExpression AND additiveExpression             # between
    //     | NOT? IN subquery                                                   # inSubquery
    //     | NOT? IN LPAREN expression (COMMA expression)* RPAREN               # inList
    //     | NOT? LIKE additiveExpression (ESCAPE additiveExpression)?          # like
    //     ;
    private static Expression ToAst(PredicateContext c)
    {
        var left = ToAst(c.additiveExpression());
        return c.predicateSuffix() switch
        {
            null => left,
            QuantifiedComparisonContext q => new QuantifiedComparison(
                left,
                ToAst(q.comparisonOperator()),
                q.ANY() is not null ? Quantifier.Any : q.SOME() is not null ? Quantifier.Some : Quantifier.All,
                ToAst(q.subquery())),
            ComparisonContext cmp => new BinaryExpression(ToAst(cmp.comparisonOperator()), left, ToAst(cmp.additiveExpression())),
            IsNullContext isNull => new IsNullExpression(left, isNull.NOT() is not null),
            BetweenContext between => new BetweenExpression(
                left, between.NOT() is not null, ToAst(between.additiveExpression(0)), ToAst(between.additiveExpression(1))),
            InSubqueryContext inSubquery => new InSubqueryExpression(left, inSubquery.NOT() is not null, ToAst(inSubquery.subquery())),
            InListContext inList => new InListExpression(left, inList.NOT() is not null, inList.expression().Select(ToAst).ToArray()),
            LikeContext like => new LikeExpression(
                left,
                like.NOT() is not null,
                ToAst(like.additiveExpression(0)),
                like.ESCAPE() is not null ? ToAst(like.additiveExpression(1)) : null),
            var other => throw new InvalidOperationException(other.GetType().Name),
        };
    }

    // comparisonOperator : EQ | NEQ | LT | GT | LTE | GTE ;
    private static BinaryOperator ToAst(ComparisonOperatorContext c) => c.Start.Type switch
    {
        EQ => BinaryOperator.Equal,
        NEQ => BinaryOperator.NotEqual,
        LT => BinaryOperator.LessThan,
        GT => BinaryOperator.GreaterThan,
        LTE => BinaryOperator.LessThanOrEqual,
        _ => BinaryOperator.GreaterThanOrEqual,
    };

    // additiveExpression : multiplicativeExpression (additiveOperator multiplicativeExpression)* ;
    // additiveOperator : PLUS | MINUS | CONCAT ;
    private static Expression ToAst(AdditiveExpressionContext c)
    {
        var operands = c.multiplicativeExpression();
        var operators = c.additiveOperator();
        var result = ToAst(operands[0]);
        for (var i = 0; i < operators.Length; i++)
        {
            var op = operators[i].Start.Type switch
            {
                PLUS => BinaryOperator.Add,
                MINUS => BinaryOperator.Subtract,
                _ => BinaryOperator.Concat,
            };
            result = new BinaryExpression(op, result, ToAst(operands[i + 1]));
        }
        return result;
    }

    // multiplicativeExpression : unaryExpression (multiplicativeOperator unaryExpression)* ;
    // multiplicativeOperator : STAR | SLASH ;
    private static Expression ToAst(MultiplicativeExpressionContext c)
    {
        var operands = c.unaryExpression();
        var operators = c.multiplicativeOperator();
        var result = ToAst(operands[0]);
        for (var i = 0; i < operators.Length; i++)
        {
            var op = operators[i].Start.Type == STAR ? BinaryOperator.Multiply : BinaryOperator.Divide;
            result = new BinaryExpression(op, result, ToAst(operands[i + 1]));
        }
        return result;
    }

    // unaryExpression : (PLUS | MINUS) unaryExpression | primary ;
    private static Expression ToAst(UnaryExpressionContext c) =>
        c.primary() is { } primary
            ? ToAst(primary)
            : new UnaryExpression(c.MINUS() is not null ? UnaryOperator.Minus : UnaryOperator.Plus, ToAst(c.unaryExpression()));

    // primary
    //     : NUMBER                                                # numberLiteral
    //     | STRING                                                # stringLiteral
    //     | NULL                                                  # nullLiteral
    //     | CASE caseOperand? whenClause+ elseClause? END         # caseExpression
    //     | CAST LPAREN expression AS dataType RPAREN             # castExpression
    //     | EXISTS subquery                                       # existsExpression
    //     | qualifiedName LPAREN functionArguments? RPAREN        # functionCall
    //     | qualifiedName                                         # columnReference
    //     | subquery                                              # scalarSubquery
    //     | LPAREN expression RPAREN                              # parenthesized
    //     ;
    // caseOperand : expression ;  whenClause : WHEN expression THEN expression ;  elseClause : ELSE expression ;
    private static Expression ToAst(PrimaryContext c) => c switch
    {
        NumberLiteralContext n => new NumericLiteral(n.NUMBER().GetText()),
        StringLiteralContext s => new Ast.StringLiteral(Unquote(s.STRING(), '\'')),
        NullLiteralContext => NullLiteral.Instance,
        CaseExpressionContext ce => new CaseExpression(
            ce.caseOperand() is { } operand ? ToAst(operand.expression()) : null,
            ce.whenClause().Select(w => new WhenClause(ToAst(w.expression(0)), ToAst(w.expression(1)))).ToArray(),
            ce.elseClause() is { } @else ? ToAst(@else.expression()) : null),
        CastExpressionContext cast => new CastExpression(ToAst(cast.expression()), ToAst(cast.dataType())),
        ExistsExpressionContext exists => new ExistsExpression(ToAst(exists.subquery())),
        FunctionCallContext f => FunctionCall(string.Join(".", ToAst(f.qualifiedName())), f.functionArguments()),
        ColumnReferenceContext column => new ColumnReference(ToAst(column.qualifiedName())),
        ScalarSubqueryContext scalar => new ScalarSubquery(ToAst(scalar.subquery())),
        ParenthesizedContext p => ToAst(p.expression()),
        _ => throw new InvalidOperationException(c.GetType().Name),
    };

    // functionArguments : STAR | setQuantifier? expression (COMMA expression)* ;
    private static Expression FunctionCall(string name, FunctionArgumentsContext? arguments) =>
        arguments is null ? new Ast.FunctionCall(name, SetQuantifier.None, NoExpressions, false)
        : arguments.STAR() is not null ? new Ast.FunctionCall(name, SetQuantifier.None, NoExpressions, true)
        : new Ast.FunctionCall(name, ToAst(arguments.setQuantifier()), arguments.expression().Select(ToAst).ToArray(), false);

    // dataType : IDENTIFIER+ (LPAREN NUMBER (COMMA NUMBER)* RPAREN)? ;
    private static DataType ToAst(DataTypeContext c) =>
        new(
            string.Join(" ", c.IDENTIFIER().Select(i => i.GetText().ToUpperInvariant())),
            c.NUMBER().Select(n => int.TryParse(n.GetText(), NumberStyles.None, CultureInfo.InvariantCulture, out var v)
                ? v
                : throw new SqlParseException($"Invalid type parameter '{n.GetText()}'")).ToArray());

    // qualifiedName : identifier (DOT identifier)* ;
    private static string[] ToAst(QualifiedNameContext c) => c.identifier().Select(ToAst).ToArray();

    // identifier : IDENTIFIER | QUOTED_IDENTIFIER ;
    private static string ToAst(IdentifierContext c) =>
        c.QUOTED_IDENTIFIER() is { } quoted ? Unquote(quoted, '"') : c.IDENTIFIER().GetText();
}
