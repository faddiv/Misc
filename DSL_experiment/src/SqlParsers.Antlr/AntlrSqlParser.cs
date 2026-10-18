using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using SqlParsers.Antlr.Generated;
using SqlParsers.Ast;

namespace SqlParsers.Antlr;

/// <summary>ANTLR 4 generated lexer/parser (ALL(*)) plus a parse-tree to AST conversion.</summary>
public sealed class AntlrSqlParser : ISqlParser
{
    public string Name => "ANTLR";

    // The lexer and parser are reused (one pair per thread): creating them per parse allocates their
    // interpreters and token buffers.
    [ThreadStatic] private static Sql92SelectLexer? _lexer;
    [ThreadStatic] private static Sql92SelectParser? _parser;

    public SelectStatement Parse(string sql)
    {
        var lexer = _lexer ??= CreateLexer();
        var parser = _parser ??= new Sql92SelectParser(null);
        lexer.SetInputStream(CharStreams.fromString(sql));
        var tokens = new CommonTokenStream(lexer);
        parser.TokenStream = tokens;

        // Two-stage parsing, as recommended by ANTLR: first with the faster SLL prediction, which bails out on
        // the first syntax error. Only if that fails is the input parsed again with full LL prediction, which
        // either succeeds (the rare inputs SLL cannot decide) or reports the real syntax error.
        parser.RemoveErrorListeners();
        parser.Interpreter.PredictionMode = PredictionMode.SLL;
        parser.ErrorHandler = new BailErrorStrategy();
        try
        {
            return AntlrAstBuilder.ToAst(parser.statement());
        }
        catch (ParseCanceledException)
        {
            tokens.Seek(0);
            parser.Reset();
            parser.AddErrorListener(ThrowingErrorListener.Instance);
            parser.Interpreter.PredictionMode = PredictionMode.LL;
            parser.ErrorHandler = new DefaultErrorStrategy();
            return AntlrAstBuilder.ToAst(parser.statement());
        }
    }

    private static Sql92SelectLexer CreateLexer()
    {
        var lexer = new Sql92SelectLexer(null);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);
        return lexer;
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
