using Antlr4.Runtime.Tree;
using SqlParsers.Ast;

namespace SqlParsers.Antlr;

/// <summary>
/// Converts the ANTLR parse tree (see <c>Grammar/Sql92Select.g4</c>) into the shared AST.
/// <para>
/// There is one <c>ToAst</c> overload per grammar rule, taking that rule's context, and the files follow the
/// sections of the grammar: <c>.Statement.cs</c>, <c>.Queries.cs</c>, <c>.FromClause.cs</c>, <c>.Expressions.cs</c>.
/// Optional parts of a rule come back from ANTLR as <c>null</c> contexts.
/// </para>
/// </summary>
internal static partial class AntlrAstBuilder
{
    private static readonly Expression[] NoExpressions = [];

    private static string Unquote(ITerminalNode node, char quote)
    {
        var text = node.GetText();
        return text.Substring(1, text.Length - 2).Replace(new string(quote, 2), quote.ToString());
    }
}
