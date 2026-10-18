using Sprache;
using SqlParsers.Ast;

namespace SqlParsers.Sprache;

/// <summary>
/// Sprache implementation of <c>grammar/sql92-select.ebnf</c>.
/// <para>
/// Every grammar rule is a field named exactly like its EBNF rule, and every definition is preceded by the
/// EBNF text it implements. Rules are created with <see cref="Rule.Create{T}"/> and defined later with
/// <see cref="Rule.Define{T}"/>, so they can be written in any order and refer to each other recursively; the files
/// follow the sections of the reference grammar:
/// </para>
/// <list type="bullet">
/// <item><c>SpracheGrammar.Statement.cs</c>: statement</item>
/// <item><c>SpracheGrammar.Queries.cs</c>: query expressions</item>
/// <item><c>SpracheGrammar.FromClause.cs</c>: FROM clause</item>
/// <item><c>SpracheGrammar.Expressions.cs</c>: expressions and names</item>
/// <item><c>Lexical.cs</c> and <c>Keywords.cs</c>: tokens</item>
/// </list>
/// Tree construction is kept out of the rules, in <see cref="Build"/>.
/// <para>
/// In a LINQ query only the first <c>from</c> source is evaluated once; every later source is evaluated again on
/// each parse. Combinators used there (<c>Optional()</c>, <c>Many()</c>, <c>Or</c>, ...) are therefore built
/// beforehand into local variables, so that parsing does not rebuild and allocate them.
/// </para>
/// </summary>
internal sealed partial class SpracheGrammar
{
    public static readonly Parser<SelectStatement> Statement = new SpracheGrammar().statement;

    private SpracheGrammar()
    {
        DefineStatement();
        DefineQueries();
        DefineFromClause();
        DefineExpressions();
    }
}
