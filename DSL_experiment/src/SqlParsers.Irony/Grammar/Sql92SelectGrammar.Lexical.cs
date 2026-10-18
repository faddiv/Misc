using Irony.Parsing;

namespace SqlParsers.Irony;

internal sealed partial class Sql92SelectGrammar
{
    // NUMBER ::= ( [0-9]+ ('.' [0-9]*)? | '.' [0-9]+ ) ( [eE] [+-]? [0-9]+ )?   -- the AST keeps the text as written
    private readonly NumberLiteral number = new("NUMBER", NumberOptions.AllowStartEndDot);

    // STRING ::= "'" ( [^'] | "''" )* "'"
    private readonly StringLiteral stringLiteral = new("STRING", "'", StringOptions.AllowsDoubledQuote | StringOptions.NoEscapes);

    // QUOTED_IDENTIFIER ::= '"' ( [^"] | '""' )+ '"'
    private readonly StringLiteral quotedIdentifier = new("QUOTED_IDENTIFIER", "\"", StringOptions.AllowsDoubledQuote | StringOptions.NoEscapes);

    // IDENTIFIER ::= [A-Za-z_] [A-Za-z0-9_]*   (excluding the keywords, see MarkReservedWords)
    private readonly IdentifierTerminal identifierToken = new("IDENTIFIER");

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
