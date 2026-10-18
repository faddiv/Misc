using Irony.Parsing;
using SqlParsers.Ast;

namespace SqlParsers.Irony;

/// <summary>Irony LALR(1) parser driven by <see cref="Sql92SelectGrammar"/>. The grammar lives in <c>Grammar/</c>.</summary>
public sealed class IronySqlParser : ISqlParser
{
    // Building the LALR tables is expensive, so the language is shared. Irony's Parser keeps
    // per-parse state, so each thread gets its own instance.
    private static readonly LanguageData Language = CreateLanguage();
    private static readonly ThreadLocal<Parser> ThreadParser = new(() => new Parser(Language));

    public string Name => "Irony";

    public SelectStatement Parse(string sql)
    {
        var tree = ThreadParser.Value!.Parse(sql);
        if (tree.HasErrors() || tree.Root is null)
        {
            var messages = tree.ParserMessages.Select(m => $"{m.Location}: {m.Message}");
            throw new SqlParseException(string.Join("; ", messages));
        }

        return tree.Root.ToAst<SelectStatement>();
    }

    private static LanguageData CreateLanguage()
    {
        var language = new LanguageData(new Sql92SelectGrammar());
        var errors = language.Errors.Where(e => e.Level >= GrammarErrorLevel.Conflict).ToList();
        if (errors.Count > 0)
            throw new InvalidOperationException(
                "Irony grammar errors:" + Environment.NewLine + string.Join(Environment.NewLine, errors.Select(e => e.Message)));
        return language;
    }
}
