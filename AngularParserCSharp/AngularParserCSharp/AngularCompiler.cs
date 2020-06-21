using System;
using System.Linq.Expressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace AngularParserCSharp
{
    public class AngularCompiler
    {
        #region  Public Methods

        public Func<JavascriptObject, JavascriptObject, TReturn> Parse<TReturn>(string input)
        {
            return Parse<JavascriptObject, JavascriptObject, TReturn>(input);
        }

        public Func<TScope, TLocals, TReturn> Parse<TScope, TLocals, TReturn>(string input)
        {
            try
            {
                var scopeType = typeof(TScope);
                var localsType = typeof(TLocals);
                var antlrInputStream = new AntlrInputStream(input);
                var lex = new AngularLexer(antlrInputStream);
                lex.AddErrorListener(new StopOnErrorListener());
                var cts = new CommonTokenStream(lex);
                PrintTokens(cts);
                var parse = new AngularParser(cts)
                {
                    ErrorHandler = new BailErrorStrategy()
                };
                parse.AddErrorListener(new StopOnErrorListener());
                var root = parse.program();
                var scopeParameter = Expression.Parameter(scopeType, "s");
                var localsParameter = Expression.Parameter(localsType, "l");
                var helperParameter = Expression.Parameter(typeof(TypedJavascriptObjectHelper), "h");
                CommonAngularBuilderVisitor visitor = new CommonAngularBuilderVisitor(scopeParameter, localsParameter, helperParameter);
                var expression = visitor.Visit(root);
                expression = ExpressionHelpers.CastIfDifferentType(expression, typeof(TReturn));
                var functionExpression = Expression.Lambda<Func<TScope, TLocals, TypedJavascriptObjectHelper, TReturn>>(
                    expression, scopeParameter, localsParameter, helperParameter);
                var function = functionExpression.Compile();
                var helper = new TypedJavascriptObjectHelper();
                return (s, l) => function(s, l, helper);
            }
            catch (ParseCanceledException ex) when (ex.InnerException is InputMismatchException)
            {
                var mismatch = (InputMismatchException) ex.InnerException;
                var token = mismatch.OffendingToken;
                throw new AngularException($"Compile failed on {token.Line}:{token.Column}: {token.Text} ");
            }
        }

        #endregion

        #region  Nonpublic Methods

        private static void PrintTokens(CommonTokenStream cts)
        {
            cts.Fill();
            foreach (var token in cts.GetTokens())
            {
                Console.WriteLine(token);
            }
            cts.Reset();
        }

        #endregion
    }
}