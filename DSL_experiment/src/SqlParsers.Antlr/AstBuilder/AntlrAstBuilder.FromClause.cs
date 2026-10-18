using SqlParsers.Ast;
using static SqlParsers.Antlr.Generated.Sql92SelectParser;

namespace SqlParsers.Antlr;

internal static partial class AntlrAstBuilder
{
    // tableReference : tablePrimary joinPart* ;
    // joinPart
    //     : CROSS JOIN tablePrimary                          # crossJoin
    //     | NATURAL joinType? JOIN tablePrimary              # naturalJoin
    //     | joinType? JOIN tablePrimary joinSpecification    # qualifiedJoin
    //     ;
    private static TableReference ToAst(TableReferenceContext c)
    {
        var result = ToAst(c.tablePrimary());
        foreach (var join in c.joinPart())
        {
            result = join switch
            {
                CrossJoinContext cross => new JoinedTable(result, JoinType.Cross, false, ToAst(cross.tablePrimary()), null),
                NaturalJoinContext natural => new JoinedTable(result, ToAst(natural.joinType()), true, ToAst(natural.tablePrimary()), null),
                QualifiedJoinContext q => new JoinedTable(result, ToAst(q.joinType()), false, ToAst(q.tablePrimary()), ToAst(q.joinSpecification())),
                _ => throw new InvalidOperationException(join.GetType().Name),
            };
        }
        return result;
    }

    // joinType : INNER | LEFT OUTER? | RIGHT OUTER? | FULL OUTER? ;   (absent: JoinType.Inner)
    private static JoinType ToAst(JoinTypeContext? c) =>
        c is null || c.INNER() is not null ? JoinType.Inner
        : c.LEFT() is not null ? JoinType.Left
        : c.RIGHT() is not null ? JoinType.Right
        : JoinType.Full;

    // joinSpecification
    //     : ON expression
    //     | USING LPAREN identifier (COMMA identifier)* RPAREN
    //     ;
    private static JoinCondition ToAst(JoinSpecificationContext c) =>
        c.ON() is not null
            ? new OnCondition(ToAst(c.expression()))
            : new UsingCondition(c.identifier().Select(ToAst).ToArray());

    // tablePrimary
    //     : qualifiedName (AS? identifier)?                  # namedTable
    //     | LPAREN queryExpression RPAREN AS? identifier     # derivedTable
    //     ;
    private static TableReference ToAst(TablePrimaryContext c) => c switch
    {
        NamedTableContext n => new NamedTable(ToAst(n.qualifiedName()), n.identifier() is { } alias ? ToAst(alias) : null),
        DerivedTableContext d => new DerivedTable(ToAst(d.queryExpression()), ToAst(d.identifier())),
        _ => throw new InvalidOperationException(c.GetType().Name),
    };
}
