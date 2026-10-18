using Parlot;
using Parlot.Fluent;

namespace SqlParsers.Parlot;

/// <summary>The reserved words of the grammar, one parser each (e.g. <c>K.Select</c>).</summary>
internal static class K
{
    public static readonly Parser<TextSpan> All = Lex.Keyword("ALL");
    public static readonly Parser<TextSpan> And = Lex.Keyword("AND");
    public static readonly Parser<TextSpan> Any = Lex.Keyword("ANY");
    public static readonly Parser<TextSpan> As = Lex.Keyword("AS");
    public static readonly Parser<TextSpan> Asc = Lex.Keyword("ASC");
    public static readonly Parser<TextSpan> Between = Lex.Keyword("BETWEEN");
    public static readonly Parser<TextSpan> By = Lex.Keyword("BY");
    public static readonly Parser<TextSpan> Case = Lex.Keyword("CASE");
    public static readonly Parser<TextSpan> Cast = Lex.Keyword("CAST");
    public static readonly Parser<TextSpan> Cross = Lex.Keyword("CROSS");
    public static readonly Parser<TextSpan> Desc = Lex.Keyword("DESC");
    public static readonly Parser<TextSpan> Distinct = Lex.Keyword("DISTINCT");
    public static readonly Parser<TextSpan> Else = Lex.Keyword("ELSE");
    public static readonly Parser<TextSpan> End = Lex.Keyword("END");
    public static readonly Parser<TextSpan> Escape = Lex.Keyword("ESCAPE");
    public static readonly Parser<TextSpan> Except = Lex.Keyword("EXCEPT");
    public static readonly Parser<TextSpan> Exists = Lex.Keyword("EXISTS");
    public static readonly Parser<TextSpan> From = Lex.Keyword("FROM");
    public static readonly Parser<TextSpan> Full = Lex.Keyword("FULL");
    public static readonly Parser<TextSpan> Group = Lex.Keyword("GROUP");
    public static readonly Parser<TextSpan> Having = Lex.Keyword("HAVING");
    public static readonly Parser<TextSpan> In = Lex.Keyword("IN");
    public static readonly Parser<TextSpan> Inner = Lex.Keyword("INNER");
    public static readonly Parser<TextSpan> Intersect = Lex.Keyword("INTERSECT");
    public static readonly Parser<TextSpan> Is = Lex.Keyword("IS");
    public static readonly Parser<TextSpan> Join = Lex.Keyword("JOIN");
    public static readonly Parser<TextSpan> Left = Lex.Keyword("LEFT");
    public static readonly Parser<TextSpan> Like = Lex.Keyword("LIKE");
    public static readonly Parser<TextSpan> Natural = Lex.Keyword("NATURAL");
    public static readonly Parser<TextSpan> Not = Lex.Keyword("NOT");
    public static readonly Parser<TextSpan> Null = Lex.Keyword("NULL");
    public static readonly Parser<TextSpan> On = Lex.Keyword("ON");
    public static readonly Parser<TextSpan> Or = Lex.Keyword("OR");
    public static readonly Parser<TextSpan> Order = Lex.Keyword("ORDER");
    public static readonly Parser<TextSpan> Outer = Lex.Keyword("OUTER");
    public static readonly Parser<TextSpan> Right = Lex.Keyword("RIGHT");
    public static readonly Parser<TextSpan> Select = Lex.Keyword("SELECT");
    public static readonly Parser<TextSpan> Some = Lex.Keyword("SOME");
    public static readonly Parser<TextSpan> Then = Lex.Keyword("THEN");
    public static readonly Parser<TextSpan> Union = Lex.Keyword("UNION");
    public static readonly Parser<TextSpan> Using = Lex.Keyword("USING");
    public static readonly Parser<TextSpan> When = Lex.Keyword("WHEN");
    public static readonly Parser<TextSpan> Where = Lex.Keyword("WHERE");
}
