using Irony.Parsing;

namespace SqlParsers.Irony;

/// <summary>
/// Irony implementation of <c>grammar/sql92-select.ebnf</c>.
/// <para>
/// Every rule is a field named after its EBNF rule and is preceded by the EBNF text it implements. Irony builds
/// LALR(1) tables, so EBNF repetition and optional parts become helper nonterminals: <c>xOpt</c> for <c>x?</c>
/// (<c>Empty | ...</c>), <c>xList</c> for <c>x (',' x)*</c> (<c>MakePlusRule</c>), and left recursion for
/// <c>a (op a)*</c>. Each rule's <see cref="Rule{T}.ToAst"/> sits right under its BNF and converts the parse-tree
/// node; its comment lists the node's children once punctuation is removed.
/// </para>
/// <list type="bullet">
/// <item><c>Sql92SelectGrammar.Statement.cs</c>: statement</item>
/// <item><c>Sql92SelectGrammar.Queries.cs</c>: query expressions</item>
/// <item><c>Sql92SelectGrammar.FromClause.cs</c>: FROM clause</item>
/// <item><c>Sql92SelectGrammar.Expressions.cs</c>: expressions and names</item>
/// <item><c>Sql92SelectGrammar.Lexical.cs</c>: terminals, keywords and punctuation</item>
/// </list>
/// Keywords are written inline as strings (<c>"SELECT"</c>); Irony turns them into key terms itself.
/// </summary>
internal sealed partial class Sql92SelectGrammar : Grammar
{
    public Sql92SelectGrammar() : base(caseSensitive: false)
    {
        UseOrdinalKeyTerms();
        DefineStatement();
        DefineQueries();
        DefineFromClause();
        DefineExpressions();
        DefineLexical();
        Root = statement;
    }
}
