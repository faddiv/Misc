using Antlr4.Runtime;

namespace AngularParserCSharp
{
    public class StopOnErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        #region  Public Methods

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            throw new AngularException($"Compile error at {line}: {charPositionInLine}: {msg}", e);
        }

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine, string msg,
            RecognitionException e)
        {
            throw new AngularException($"Compile error at {line}: {charPositionInLine}: {msg}", e);
        }

        #endregion
    }
}