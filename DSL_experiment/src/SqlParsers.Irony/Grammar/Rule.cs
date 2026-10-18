using Irony.Parsing;

namespace SqlParsers.Irony;

/// <summary>
/// A nonterminal that knows how to turn its parse-tree nodes into an AST value. The conversion is written next to
/// the rule's BNF, so the child positions it uses can be read straight from the rule above it.
/// </summary>
internal sealed class Rule<T>(string name) : NonTerminal(name)
{
    public Func<ParseTreeNode, T> ToAst { get; set; } =
        _ => throw new InvalidOperationException($"Rule '{name}' has no AST conversion.");
}

/// <summary>
/// Child access for <see cref="Rule{T}.ToAst"/>. Child indexes count only the children that remain after
/// punctuation (parentheses, commas, marked keywords) has been removed from the tree.
/// </summary>
internal static class ParseTreeNodeExtensions
{
    /// <summary>Converts this node with its own rule.</summary>
    public static T ToAst<T>(this ParseTreeNode node) => ((Rule<T>)node.Term).ToAst(node);

    /// <summary>Converts child <paramref name="index"/> with whichever rule produced it.</summary>
    public static T Get<T>(this ParseTreeNode node, int index) => node.ChildNodes[index].ToAst<T>();

    /// <summary>Converts every child; for list rules made with MakePlusRule.</summary>
    public static T[] Items<T>(this ParseTreeNode node)
    {
        var children = node.ChildNodes;
        var items = new T[children.Count];
        for (var i = 0; i < items.Length; i++)
            items[i] = children[i].ToAst<T>();
        return items;
    }

    /// <summary>True for an optional rule that matched its empty alternative.</summary>
    public static bool IsEmpty(this ParseTreeNode node) => node.ChildNodes.Count == 0;

    /// <summary>True if child <paramref name="index"/> is the given keyword or symbol.</summary>
    public static bool Is(this ParseTreeNode node, int index, string keyTerm) =>
        string.Equals(node.ChildNodes[index].Term.Name, keyTerm, StringComparison.OrdinalIgnoreCase);

    /// <summary>True if child <paramref name="index"/> was produced by <paramref name="term"/>.</summary>
    public static bool Is(this ParseTreeNode node, int index, BnfTerm term) => node.ChildNodes[index].Term == term;
}
