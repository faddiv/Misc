using Antlr4.Runtime;
using SqlParsers.Antlr.Generated;
using SqlParsers.Ast;

namespace SqlParsers.Antlr;

/// <summary>ANTLR 4 generated lexer/parser (ALL(*)) plus a parse-tree to AST conversion.</summary>
public sealed class AntlrSqlParser : ISqlParser
{
    public string Name => "ANTLR";

    public SelectStatement Parse(string sql)
    {
        var lexer = new Sql92SelectLexer(CharStreams.fromString(sql));
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);

        var parser = new Sql92SelectParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowingErrorListener.Instance);

        return AntlrAstBuilder.Statement(parser.statement());
    }
}

internal sealed class ThrowingErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    public static readonly ThrowingErrorListener Instance = new();

    public override void SyntaxError(
        TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
        int line, int charPositionInLine, string msg, RecognitionException e) =>
        throw new SqlParseException($"line {line}:{charPositionInLine} {msg}", e);

    public void SyntaxError(
        TextWriter output, IRecognizer recognizer, int offendingSymbol,
        int line, int charPositionInLine, string msg, RecognitionException e) =>
        throw new SqlParseException($"line {line}:{charPositionInLine} {msg}", e);
}
