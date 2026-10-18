using Irony.Parsing;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?   -- the AST keeps the text as written
    private readonly NumberLiteral number = new("NUMBER", NumberOptions.AllowStartEndDot);

    // STRING ::= "'" ( [^'] | "''" )* "'"
    private readonly QuotedTerminal stringLiteral = new("STRING", '\'', allowEmpty: true);

    // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
    private readonly QuotedTerminal quotedIdentifier = new("QUOTED_IDENTIFIER", '"', allowEmpty: false);

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords, see MarkReservedWords)
    private readonly IdentifierTerminal identifierToken = new("IDENTIFIER");

    // Every key symbol of the grammar (keywords are SqlKeywords.Reserved).
    private static readonly string[] Symbols = ["(", ")", ",", ".", ";", "*", "/", "+", "-", "||", "=", "<>", "<", ">", "<=", ">="];

    /// <summary>
    /// Makes keyword and symbol matching ordinal. Irony compares key terms with culture-aware
    /// <c>InvariantCultureIgnoreCase</c>, both when it tries a key term at a position (<c>string.Compare</c>) and when
    /// it checks whether an identifier is a keyword (the <see cref="Grammar.KeyTerms"/> dictionary). That took over
    /// 80% of the parse time with ICU on Linux. Called before the rules are defined, so <c>ToTerm</c> and the
    /// strings in the rules find these key terms instead of creating Irony's.
    /// </summary>
    private void UseOrdinalKeyTerms()
    {
        KeyTerms = new KeyTermTable(StringComparer.OrdinalIgnoreCase);
        foreach (var text in SqlKeywords.Reserved.Concat(Symbols))
            KeyTerms[text] = new OrdinalKeyTerm(text.ToLowerInvariant(), text);
    }

    private void DefineLexical()
    {
        // Line comments ('--' up to the end of the line) may appear between any two tokens.
        NonGrammarTerminals.Add(new CommentTerminal("comment", "--", "\n", "\r\n"));

        // Reserved words never match IDENTIFIER.
        MarkReservedWords(SqlKeywords.Reserved.ToArray());

        // Punctuation is dropped from the parse tree, so ToAst child indexes skip these.
        // Keywords that carry information (DISTINCT, ALL, NOT, NULL, UNION, LEFT, ...) stay in the tree.
        MarkPunctuation(
            "(", ")", ",", ".", ";",
            "SELECT", "FROM", "WHERE", "GROUP", "BY", "HAVING", "ORDER", "AS", "JOIN", "ON", "USING",
            "CASE", "WHEN", "THEN", "ELSE", "END", "CAST", "EXISTS",
            "IS", "BETWEEN", "IN", "LIKE", "ESCAPE", "AND", "OR");
    }
}

/// <summary>A <see cref="KeyTerm"/> that matches with an ordinal, case-insensitive comparison.</summary>
internal sealed class OrdinalKeyTerm(string text, string name) : KeyTerm(text, name)
{
    public override Token? TryMatch(ParsingContext context, ISourceStream source)
    {
        var input = source.Text;
        var position = source.PreviewPosition;
        if (position + Text.Length > input.Length
            || !input.AsSpan(position, Text.Length).Equals(Text, StringComparison.OrdinalIgnoreCase))
            return null;

        source.PreviewPosition += Text.Length;
        if (Flags.IsSet(TermFlags.IsKeyword) && !AllowAlphaAfterKeyword)
        {
            var next = source.PreviewChar;
            if (char.IsLetterOrDigit(next) || next == '_') return null;
        }
        return source.CreateToken(OutputTerminal, Text);
    }
}

/// <summary>
/// A quoted string or identifier in which a doubled quote stands for one quote. Its value is the unescaped text.
/// <para>
/// Irony's <c>StringLiteral</c> (with <c>AllowsDoubledQuote</c>) checks for the doubled quote at the closing quote
/// with a culture-aware comparison, which was a fifth of the parse time with ICU on Linux.
/// </para>
/// </summary>
internal sealed class QuotedTerminal(string name, char quote, bool allowEmpty) : Terminal(name)
{
    private readonly string _doubledQuote = new(quote, 2);

    public override IList<string> GetFirsts() => [quote.ToString()];

    public override Token? TryMatch(ParsingContext context, ISourceStream source)
    {
        var input = source.Text;
        var start = source.PreviewPosition;
        if (start >= input.Length || input[start] != quote) return null;

        var escaped = false;
        var end = start + 1;
        while (true)
        {
            end = input.IndexOf(quote, end);
            if (end < 0) return context.CreateErrorToken($"Missing closing {quote}");
            if (end + 1 < input.Length && input[end + 1] == quote)
            {
                escaped = true;
                end += 2;
                continue;
            }
            break;
        }

        var body = input.Substring(start + 1, end - start - 1);
        if (escaped) body = body.Replace(_doubledQuote, quote.ToString());
        if (body.Length == 0 && !allowEmpty) return context.CreateErrorToken($"Empty {Name}");
        source.PreviewPosition = end + 1;
        return source.CreateToken(OutputTerminal, body);
    }
}
