using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

internal sealed partial class SpracheGrammar
{
    private readonly Parser<TableReference> tableReference = Rule.Create<TableReference>(nameof(tableReference));
    private readonly Parser<JoinPart> joinPart = Rule.Create<JoinPart>(nameof(joinPart));
    private readonly Parser<JoinType> joinType = Rule.Create<JoinType>(nameof(joinType));
    private readonly Parser<JoinCondition> joinSpecification = Rule.Create<JoinCondition>(nameof(joinSpecification));
    private readonly Parser<TableReference> tablePrimary = Rule.Create<TableReference>(nameof(tablePrimary));

    private void DefineFromClause()
    {
        // tableReference ::= tablePrimary joinPart*
        var joinParts = joinPart.Many();
        tableReference.Define(
            from first in tablePrimary
            from joins in joinParts
            select Build.Joins(first, joins));

        // joinPart ::= CROSS JOIN tablePrimary
        //            | NATURAL joinType? JOIN tablePrimary
        //            | joinType? JOIN tablePrimary joinSpecification
        var joinTypeOrInner = joinType.Or(Parse.Return(JoinType.Inner));
        joinPart.Define(
            (from _ in K.Cross
             from __ in K.Join
             from right in tablePrimary
             select Build.CrossJoin(right))
            .Or(from _ in K.Natural
                from type in joinTypeOrInner
                from __ in K.Join
                from right in tablePrimary
                select Build.NaturalJoin(type, right))
            .Or(from type in joinTypeOrInner
                from _ in K.Join
                from right in tablePrimary
                from condition in joinSpecification
                select Build.QualifiedJoin(type, right, condition)));

        // joinType ::= INNER | (LEFT | RIGHT | FULL) OUTER?
        var optionalOuter = K.Outer.Optional();
        joinType.Define(
            K.Inner.Return(JoinType.Inner)
                .Or(from _ in K.Left from __ in optionalOuter select JoinType.Left)
                .Or(from _ in K.Right from __ in optionalOuter select JoinType.Right)
                .Or(from _ in K.Full from __ in optionalOuter select JoinType.Full));

        // joinSpecification ::= ON expression
        //                     | USING '(' identifier (',' identifier)* ')'
        var columns = Lex.Parens(identifier.DelimitedBy(Lex.Comma));
        joinSpecification.Define(
            (from _ in K.On
             from condition in expression
             select Build.On(condition))
            .Or(from _ in K.Using
                from names in columns
                select Build.Using(names)));

        // tablePrimary ::= qualifiedName alias?
        //                | '(' queryExpression ')' alias
        var optionalAlias = alias.Optional();
        tablePrimary.Define(
            (from name in qualifiedName
             from a in optionalAlias
             select Build.NamedTable(name, a))
            .Or(from query in Lex.Parens(queryExpression)
                from a in alias
                select Build.DerivedTable(query, a)));
    }
}
