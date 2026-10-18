using Farkle.Builder;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

internal sealed partial class FarkleGrammar
{
    private readonly Nonterminal<List<TableReference>> tableReferenceList = Nonterminal.Create<List<TableReference>>(nameof(tableReferenceList));
    private readonly Nonterminal<TableReference> tableReference = Nonterminal.Create<TableReference>(nameof(tableReference));
    private readonly Nonterminal<JoinType> joinTypeOpt = Nonterminal.Create<JoinType>(nameof(joinTypeOpt));
    private readonly Nonterminal<JoinCondition> joinSpecification = Nonterminal.Create<JoinCondition>(nameof(joinSpecification));
    private readonly Nonterminal<List<string>> identifierList = Nonterminal.Create<List<string>>(nameof(identifierList));
    private readonly Nonterminal<TableReference> tablePrimary = Nonterminal.Create<TableReference>(nameof(tablePrimary));
    private readonly Nonterminal<string> derivedTableAlias = Nonterminal.Create<string>(nameof(derivedTableAlias));

    private void DefineFromClause()
    {
        DefineList(tableReferenceList, tableReference, ",");

        // tableReference ::= tablePrimary joinPart*
        // joinPart       ::= CROSS JOIN tablePrimary
        //                  | NATURAL joinType? JOIN tablePrimary
        //                  | joinType? JOIN tablePrimary joinSpecification
        //   joinPart* is written as left recursion on tableReference.
        tableReference.SetProductions(
            tablePrimary.AsProduction(),
            tableReference.Extended().Append("CROSS").Append("JOIN").Extend(tablePrimary)
                .Finish(Build.CrossJoin),
            tableReference.Extended().Append("NATURAL").Extend(joinTypeOpt).Append("JOIN").Extend(tablePrimary)
                .Finish(Build.NaturalJoin),
            tableReference.Extended().Extend(joinTypeOpt).Append("JOIN").Extend(tablePrimary).Extend(joinSpecification)
                .Finish(Build.QualifiedJoin));

        // joinType ::= INNER | (LEFT | RIGHT | FULL) OUTER?
        var outerOpt = Nonterminal.CreateUntyped("outerOpt", Empty, "OUTER".Appended());
        joinTypeOpt.SetProductions(
            Empty.FinishConstant(JoinType.Inner),
            "INNER".Appended().FinishConstant(JoinType.Inner),
            "LEFT".Appended().Append(outerOpt).FinishConstant(JoinType.Left),
            "RIGHT".Appended().Append(outerOpt).FinishConstant(JoinType.Right),
            "FULL".Appended().Append(outerOpt).FinishConstant(JoinType.Full));

        // joinSpecification ::= ON expression
        //                     | USING '(' identifier (',' identifier)* ')'
        joinSpecification.SetProductions(
            "ON".Appended().Extend(expression).Finish(Build.On),
            "USING".Appended().Append("(").Extend(identifierList).Append(")").Finish(Build.Using));
        DefineList(identifierList, identifier, ",");

        // tablePrimary ::= qualifiedName alias?
        //                | '(' queryExpression ')' alias
        tablePrimary.SetProductions(
            qualifiedName.Extended().Extend(aliasOpt).Finish(Build.NamedTable),
            "(".Appended().Extend(queryExpression).Append(")").Extend(derivedTableAlias).Finish(Build.DerivedTable));

        derivedTableAlias.SetProductions(
            identifier.AsProduction(),
            "AS".Appended().Extend(identifier).AsProduction());
    }
}
