using Parlot.Fluent;
using SqlParsers.Ast;

namespace SqlParsers.Parlot;

/// <summary>
/// Parlot implementation of <c>grammar/sql92-select.ebnf</c>.
/// <para>
/// Every grammar rule is a field named exactly like its EBNF rule, and every definition is preceded by the
/// EBNF text it implements. Rules are <see cref="Rule{T}"/> forward references, so they can be defined in any order and refer
/// to each other recursively; the files follow the sections of the reference grammar:
/// </para>
/// <list type="bullet">
/// <item><c>ParlotGrammar.Statement.cs</c>: statement</item>
/// <item><c>ParlotGrammar.Queries.cs</c>: query expressions</item>
/// <item><c>ParlotGrammar.FromClause.cs</c>: FROM clause</item>
/// <item><c>ParlotGrammar.Expressions.cs</c>: expressions and names</item>
/// <item><c>Lexical.cs</c> and <c>Keywords.cs</c>: tokens</item>
/// </list>
/// Tree construction is kept out of the rules, in <see cref="Build"/>.
/// </summary>
internal sealed partial class ParlotGrammar
{
    public static readonly Parser<SelectStatement> Statement = new ParlotGrammar().statement;

    private ParlotGrammar()
    {
        DefineStatement();
        DefineQueries();
        DefineFromClause();
        DefineExpressions();
    }
}
