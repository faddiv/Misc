using Superpower;
using SqlParsers.Ast;

namespace SqlParsers.Superpower;

/// <summary>
/// Superpower implementation of <c>grammar/sql92-select.ebnf</c>.
/// <para>
/// Every grammar rule is a field named exactly like its EBNF rule, and every definition is preceded by the
/// EBNF text it implements. Rules are created with <see cref="Rule.Create{T}"/> and defined later with
/// <see cref="Rule.Define{T}"/>, so they can be written in any order and refer to each other recursively; the files
/// follow the sections of the reference grammar:
/// </para>
/// <list type="bullet">
/// <item><c>SuperpowerGrammar.Statement.cs</c>: statement</item>
/// <item><c>SuperpowerGrammar.Queries.cs</c>: query expressions</item>
/// <item><c>SuperpowerGrammar.FromClause.cs</c>: FROM clause</item>
/// <item><c>SuperpowerGrammar.Expressions.cs</c>: expressions and names</item>
/// <item><c>Lexical.cs</c> and <c>Keywords.cs</c>: tokenizer and tokens</item>
/// </list>
/// Tree construction is kept out of the rules, in <see cref="Build"/>.
/// <para>
/// In a LINQ query only the first <c>from</c> source is evaluated once; every later source is evaluated again on
/// each parse. Combinators used there (<c>OptionalOrDefault()</c>, <c>Many()</c>, <c>OneOf</c>, ...) are therefore
/// built beforehand into local variables, so that parsing does not rebuild and allocate them.
/// </para>
/// <para>
/// Superpower does not backtrack once an alternative has consumed a token. The left-factored reference grammar
/// makes that a non-issue except in <c>selectItem</c>, where <c>Try()</c> is needed.
/// </para>
/// </summary>
internal sealed partial class SuperpowerGrammar
{
    public static readonly TokenListParser<SqlToken, SelectStatement> Statement = new SuperpowerGrammar().statement;

    private SuperpowerGrammar()
    {
        DefineStatement();
        DefineQueries();
        DefineFromClause();
        DefineExpressions();
    }
}
