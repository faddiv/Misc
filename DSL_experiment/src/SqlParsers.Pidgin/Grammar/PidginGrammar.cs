using Pidgin;
using SqlParsers.Ast;

namespace SqlParsers.Pidgin;

/// <summary>
/// Pidgin implementation of <c>grammar/sql92-select.ebnf</c>.
/// <para>
/// Every grammar rule is a field named exactly like its EBNF rule, and every definition is preceded by the
/// EBNF text it implements. Rules are <see cref="Rule{T}"/> forward references, so they can be defined in any
/// order and refer to each other recursively; the files follow the sections of the reference grammar:
/// </para>
/// <list type="bullet">
/// <item><c>PidginGrammar.Statement.cs</c>: statement</item>
/// <item><c>PidginGrammar.Queries.cs</c>: query expressions</item>
/// <item><c>PidginGrammar.FromClause.cs</c>: FROM clause</item>
/// <item><c>PidginGrammar.Expressions.cs</c>: expressions and names</item>
/// <item><c>Lexical.cs</c> and <c>Keywords.cs</c>: tokens</item>
/// </list>
/// Tree construction is kept out of the rules, in <see cref="Build"/>.
/// <para>
/// Pidgin does not backtrack once an alternative has consumed input. The left-factored reference grammar
/// makes that a non-issue except in <c>selectItem</c>, where <c>Try</c> is needed.
/// </para>
/// </summary>
internal sealed partial class PidginGrammar
{
    public static readonly Parser<char, SelectStatement> Statement = new PidginGrammar().statement;

    private PidginGrammar()
    {
        DefineStatement();
        DefineQueries();
        DefineFromClause();
        DefineExpressions();
    }
}
