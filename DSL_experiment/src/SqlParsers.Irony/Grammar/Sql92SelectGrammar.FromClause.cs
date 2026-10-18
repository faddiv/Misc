using Irony.Parsing;
using SqlParsers.Ast;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    private readonly Rule<TableReference[]> tableReferenceList = new(nameof(tableReferenceList));
    private readonly Rule<TableReference> tableReference = new(nameof(tableReference));
    private readonly Rule<TableReference> joinedTable = new(nameof(joinedTable));
    private readonly Rule<JoinType> joinTypeOpt = new(nameof(joinTypeOpt));
    private readonly NonTerminal outerOpt = new(nameof(outerOpt));
    private readonly Rule<JoinCondition> joinSpecification = new(nameof(joinSpecification));
    private readonly Rule<string[]> identifierList = new(nameof(identifierList));
    private readonly Rule<TableReference> tablePrimary = new(nameof(tablePrimary));
    private readonly Rule<string> derivedTableAlias = new(nameof(derivedTableAlias));

    private void DefineFromClause()
    {
        tableReferenceList.Rule = MakePlusRule(tableReferenceList, ToTerm(","), tableReference);
        tableReferenceList.ToAst = n => n.Items<TableReference>();

        // tableReference ::= tablePrimary joinPart*
        //   joinPart* is written as left recursion: joinedTable ::= tableReference joinPart
        tableReference.Rule = tablePrimary | joinedTable;
        tableReference.ToAst = n => n.Get<TableReference>(0);

        // joinPart ::= CROSS JOIN tablePrimary
        //            | NATURAL joinType? JOIN tablePrimary
        //            | joinType? JOIN tablePrimary joinSpecification
        joinedTable.Rule =
            tableReference + "CROSS" + "JOIN" + tablePrimary
            | tableReference + "NATURAL" + joinTypeOpt + "JOIN" + tablePrimary
            | tableReference + joinTypeOpt + "JOIN" + tablePrimary + joinSpecification;
        joinedTable.ToAst = n =>
            n.Is(1, "CROSS")                           // [tableReference, CROSS, tablePrimary]
                ? Build.Join(n.Get<TableReference>(0), JoinType.Cross, false, n.Get<TableReference>(2), null)
            : n.Is(1, "NATURAL")                       // [tableReference, NATURAL, joinTypeOpt, tablePrimary]
                ? Build.Join(n.Get<TableReference>(0), n.Get<JoinType>(2), true, n.Get<TableReference>(3), null)
                                                       // [tableReference, joinTypeOpt, tablePrimary, joinSpecification]
                : Build.Join(n.Get<TableReference>(0), n.Get<JoinType>(1), false, n.Get<TableReference>(2), n.Get<JoinCondition>(3));

        // joinType ::= INNER | (LEFT | RIGHT | FULL) OUTER?
        joinTypeOpt.Rule = Empty | "INNER" | ToTerm("LEFT") + outerOpt | ToTerm("RIGHT") + outerOpt | ToTerm("FULL") + outerOpt;
        joinTypeOpt.ToAst = n =>
            n.IsEmpty() || n.Is(0, "INNER") ? JoinType.Inner
            : n.Is(0, "LEFT") ? JoinType.Left
            : n.Is(0, "RIGHT") ? JoinType.Right
            : JoinType.Full;

        outerOpt.Rule = Empty | "OUTER";

        // joinSpecification ::= ON expression
        //                     | USING '(' identifier (',' identifier)* ')'
        joinSpecification.Rule = ToTerm("ON") + expression | ToTerm("USING") + "(" + identifierList + ")";
        joinSpecification.ToAst = n => n.Is(0, identifierList)
            ? Build.Using(n.Get<string[]>(0))          // [identifierList]
            : Build.On(n.Get<Expression>(0));          // [expression]

        identifierList.Rule = MakePlusRule(identifierList, ToTerm(","), identifier);
        identifierList.ToAst = n => n.Items<string>();

        // tablePrimary ::= qualifiedName alias?
        //                | '(' queryExpression ')' alias
        tablePrimary.Rule = qualifiedName + aliasOpt | ToTerm("(") + queryExpression + ")" + derivedTableAlias;
        tablePrimary.ToAst = n => n.Is(0, qualifiedName)
            ? Build.NamedTable(n.Get<string[]>(0), n.Get<string?>(1))           // [qualifiedName, aliasOpt]
            : Build.DerivedTable(n.Get<QueryExpression>(0), n.Get<string>(1));  // [queryExpression, derivedTableAlias]

        derivedTableAlias.Rule = identifier | ToTerm("AS") + identifier;
        derivedTableAlias.ToAst = n => n.Get<string>(0);
    }
}
