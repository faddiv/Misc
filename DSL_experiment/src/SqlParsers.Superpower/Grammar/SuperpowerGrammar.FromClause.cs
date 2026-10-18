using Superpower;
using SqlParsers.Ast;

namespace SqlParsers.Superpower;

internal sealed partial class SuperpowerGrammar
{
    private readonly TokenListParser<SqlToken, TableReference> tableReference = Rule.Create<TableReference>(nameof(tableReference));
    private readonly TokenListParser<SqlToken, JoinPart> joinPart = Rule.Create<JoinPart>(nameof(joinPart));
    private readonly TokenListParser<SqlToken, JoinType> joinType = Rule.Create<JoinType>(nameof(joinType));
    private readonly TokenListParser<SqlToken, JoinCondition> joinSpecification = Rule.Create<JoinCondition>(nameof(joinSpecification));
    private readonly TokenListParser<SqlToken, TableReference> tablePrimary = Rule.Create<TableReference>(nameof(tablePrimary));

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
        var joinTypeOrInner = joinType.OptionalOrDefault(JoinType.Inner);
        joinPart.Define(Parse.OneOf(
            from _ in K.Cross
            from __ in K.Join
            from right in tablePrimary
            select Build.CrossJoin(right),

            from _ in K.Natural
            from type in joinTypeOrInner
            from __ in K.Join
            from right in tablePrimary
            select Build.NaturalJoin(type, right),

            from type in joinTypeOrInner
            from _ in K.Join
            from right in tablePrimary
            from condition in joinSpecification
            select Build.QualifiedJoin(type, right, condition)));

        // joinType ::= INNER | (LEFT | RIGHT | FULL) OUTER?
        joinType.Define(Parse.OneOf(
            K.Inner.Value(JoinType.Inner),
            K.Left.IgnoreThen(K.Outer.Optional()).Value(JoinType.Left),
            K.Right.IgnoreThen(K.Outer.Optional()).Value(JoinType.Right),
            K.Full.IgnoreThen(K.Outer.Optional()).Value(JoinType.Full)));

        // joinSpecification ::= ON expression
        //                     | USING '(' identifier (',' identifier)* ')'
        joinSpecification.Define(
            K.On.IgnoreThen(expression).Select(Build.On)
                .Or(K.Using.IgnoreThen(Lex.Parens(identifier.AtLeastOnceDelimitedBy(Lex.Comma))).Select(Build.Using)));

        // tablePrimary ::= qualifiedName alias?
        //                | '(' queryExpression ')' alias
        var optionalAlias = alias.OptionalOrDefault();
        tablePrimary.Define(
            (from name in qualifiedName
             from a in optionalAlias
             select Build.NamedTable(name, a))
            .Or(from query in Lex.Parens(queryExpression)
                from a in alias
                select Build.DerivedTable(query, a)));
    }
}
