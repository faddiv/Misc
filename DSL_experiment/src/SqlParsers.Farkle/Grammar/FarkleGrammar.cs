using Farkle;
using Farkle.Builder;
using SqlParsers.Ast;

namespace SqlParsers.Farkle;

/// <summary>
/// Farkle implementation of <c>grammar/sql92-select.ebnf</c>.
/// <para>
/// Every rule is a nonterminal field named after its EBNF rule and is preceded by the EBNF text it implements.
/// Farkle nonterminals can be created first and given their productions later (<c>SetProductions</c>), so the
/// rules are written top-down in any order. Farkle builds LALR(1) tables, so EBNF repetition and optional parts become
/// helper nonterminals: <c>xOpt</c> for <c>x?</c> (an empty production plus the alternatives) and <c>xList</c> for
/// <c>x (',' x)*</c> (see <see cref="DefineList{T}"/>). The files follow the sections of the reference grammar:
/// </para>
/// <list type="bullet">
/// <item><c>FarkleGrammar.Statement.cs</c>: statement</item>
/// <item><c>FarkleGrammar.Queries.cs</c>: query expressions</item>
/// <item><c>FarkleGrammar.FromClause.cs</c>: FROM clause</item>
/// <item><c>FarkleGrammar.Expressions.cs</c>: expressions and names</item>
/// <item><c>FarkleGrammar.Lexical.cs</c>: terminals</item>
/// </list>
/// Keywords are written inline as literals (<c>"SELECT".Appended()</c>). Farkle gives literals priority over the
/// identifier regex, which makes them reserved. Tree construction is kept out of the rules, in <see cref="Build"/>.
/// </summary>
internal sealed partial class FarkleGrammar
{
    public static readonly CharParser<SelectStatement> Parser = new FarkleGrammar().CreateParser();

    /// <summary>The empty production, for optional rules (<c>xOpt</c>).</summary>
    private static ProductionBuilder Empty => ProductionBuilder.Empty;

    private FarkleGrammar()
    {
        DefineStatement();
        DefineQueries();
        DefineFromClause();
        DefineExpressions();
    }

    private CharParser<SelectStatement> CreateParser()
    {
        var parser = statement
            .CaseSensitive(false)
            .AddLineComment("--")     // line comments may appear between any two tokens
            .Build();

        if (parser.IsFailing)
            throw new InvalidOperationException("Farkle grammar failed to build: " + parser.Parse("").Error);

        return parser;
    }

    /// <summary>
    /// <c>list ::= item (separator item)*</c>, written as <c>item | list separator item</c>. This is the shape
    /// Farkle's own <c>SeparatedBy</c>/<c>Many</c> generate; spelling it out gives the list a name of its own.
    /// </summary>
    private static void DefineList<T>(Nonterminal<List<T>> list, IGrammarSymbol<T> item, string? separator)
    {
        var next = separator is null
            ? list.Extended().Extend(item)
            : list.Extended().Append(separator).Extend(item);
        list.SetProductions(
            item.Extended().Finish(Build.StartList),
            next.Finish(Build.AddToList));
    }
}
