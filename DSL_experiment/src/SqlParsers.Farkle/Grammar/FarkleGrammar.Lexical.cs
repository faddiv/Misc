using Farkle.Builder;
using Farkle.Parser;

namespace SqlParsers.Farkle;

internal sealed partial class FarkleGrammar
{
    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (keywords are literals, which win over this regex)
    private readonly IGrammarSymbol<string> identifierToken = Terminal.Create(
        "IDENTIFIER",
        Regex.FromRegexString("[A-Za-z_][A-Za-z0-9_]*"),
        (ref ParserState _, ReadOnlySpan<char> chars) => chars.ToString());

    // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
    private readonly IGrammarSymbol<string> quotedIdentifier = Terminal.Create(
        "QUOTED_IDENTIFIER",
        Regex.FromRegexString("\"([^\"]|\"\")+\""),
        (ref ParserState _, ReadOnlySpan<char> chars) => Unquote(chars, '"'));

    // STRING ::= "'" ( [^'] | "''" )* "'"
    private readonly IGrammarSymbol<string> stringLiteral = Terminal.Create(
        "STRING",
        Regex.FromRegexString("'([^']|'')*'"),
        (ref ParserState _, ReadOnlySpan<char> chars) => Unquote(chars, '\''));

    // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?   -- kept as written
    private readonly IGrammarSymbol<string> number = Terminal.Create(
        "NUMBER",
        Regex.FromRegexString(@"([0-9]+(\.[0-9]*)?|\.[0-9]+)([eE][+\-]?[0-9]+)?"),
        (ref ParserState _, ReadOnlySpan<char> chars) => chars.ToString());

    private static string Unquote(ReadOnlySpan<char> chars, char quote) =>
        chars[1..^1].ToString().Replace(new string(quote, 2), quote.ToString());
}
