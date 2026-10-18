using Parlot.Fluent;
using SqlParsers.Ast;

namespace SqlParsers.Parlot;

internal sealed partial class ParlotGrammar
{
    private readonly Rule<TableReference> tableReference = new() { Name = "tableReference" };
    private readonly Rule<JoinPart> joinPart = new() { Name = "joinPart" };
    private readonly Rule<JoinType> joinType = new() { Name = "joinType" };
    private readonly Rule<JoinCondition> joinSpecification = new() { Name = "joinSpecification" };
    private readonly Rule<TableReference> tablePrimary = new() { Name = "tablePrimary" };

    private void DefineFromClause()
    {
        // tableReference ::= tablePrimary joinPart*
        tableReference.Definition = tablePrimary
            .And(ZeroOrMany(joinPart))
            .Then(Build.Joins);

        // joinPart ::= CROSS JOIN tablePrimary
        //            | NATURAL joinType? JOIN tablePrimary
        //            | joinType? JOIN tablePrimary joinSpecification
        //   (ZeroOrOne(joinType) yields JoinType.Inner when absent)
        joinPart.Definition = OneOf(
            K.Cross.SkipAnd(K.Join).SkipAnd(tablePrimary).Then(Build.CrossJoin),
            K.Natural.SkipAnd(ZeroOrOne(joinType)).AndSkip(K.Join).And(tablePrimary).Then(Build.NaturalJoin),
            ZeroOrOne(joinType).AndSkip(K.Join).And(tablePrimary).And(joinSpecification).Then(Build.QualifiedJoin));

        // joinType ::= INNER | (LEFT | RIGHT | FULL) OUTER?
        joinType.Definition = OneOf(
            K.Inner.Then(static _ => JoinType.Inner),
            K.Left.AndSkip(ZeroOrOne(K.Outer)).Then(static _ => JoinType.Left),
            K.Right.AndSkip(ZeroOrOne(K.Outer)).Then(static _ => JoinType.Right),
            K.Full.AndSkip(ZeroOrOne(K.Outer)).Then(static _ => JoinType.Full));

        // joinSpecification ::= ON expression
        //                     | USING '(' identifier (',' identifier)* ')'
        joinSpecification.Definition = OneOf(
            K.On.SkipAnd(expression).Then(Build.On),
            K.Using.SkipAnd(Lex.Parens(Separated(Lex.Comma, identifier))).Then(Build.Using));

        // tablePrimary ::= qualifiedName alias?
        //                | '(' queryExpression ')' alias
        tablePrimary.Definition = OneOf(
            qualifiedName.And(ZeroOrOne(alias)).Then(Build.NamedTable),
            Lex.Parens(queryExpression).And(alias).Then(Build.DerivedTable));
    }
}
