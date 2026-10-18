using Pidgin;
using SqlParsers.Ast;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace SqlParsers.Pidgin;

internal sealed partial class PidginGrammar
{
    private readonly Rule<TableReference> tableReference = new(nameof(tableReference));
    private readonly Rule<JoinPart> joinPart = new(nameof(joinPart));
    private readonly Rule<JoinType> joinType = new(nameof(joinType));
    private readonly Rule<JoinCondition> joinSpecification = new(nameof(joinSpecification));
    private readonly Rule<TableReference> tablePrimary = new(nameof(tablePrimary));

    private void DefineFromClause()
    {
        // tableReference ::= tablePrimary joinPart*
        tableReference.Definition = Map(Build.Joins, tablePrimary, joinPart.Many());

        // joinPart ::= CROSS JOIN tablePrimary
        //            | NATURAL joinType? JOIN tablePrimary
        //            | joinType? JOIN tablePrimary joinSpecification
        joinPart.Definition = OneOf(
            K.Cross.Then(K.Join).Then(tablePrimary).Select(Build.CrossJoin),
            K.Natural.Then(Map(Build.NaturalJoin, joinType.Optional(), K.Join.Then(tablePrimary))),
            Map(Build.QualifiedJoin, joinType.Optional(), K.Join.Then(tablePrimary), joinSpecification));

        // joinType ::= INNER | (LEFT | RIGHT | FULL) OUTER?
        joinType.Definition = OneOf(
            K.Inner.ThenReturn(JoinType.Inner),
            K.Left.Then(K.Outer.Optional()).ThenReturn(JoinType.Left),
            K.Right.Then(K.Outer.Optional()).ThenReturn(JoinType.Right),
            K.Full.Then(K.Outer.Optional()).ThenReturn(JoinType.Full));

        // joinSpecification ::= ON expression
        //                     | USING '(' identifier (',' identifier)* ')'
        joinSpecification.Definition = OneOf(
            K.On.Then(expression).Select(Build.On),
            K.Using.Then(Lex.Parens(identifier.SeparatedAtLeastOnce(Lex.Comma))).Select(Build.Using));

        // tablePrimary ::= qualifiedName alias?
        //                | '(' queryExpression ')' alias
        tablePrimary.Definition = OneOf(
            Map(Build.NamedTable, qualifiedName, alias.Optional()),
            Map(Build.DerivedTable, Lex.Parens(queryExpression), alias));
    }
}
