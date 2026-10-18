using Sprache;

namespace SqlParsers.Sprache;

/// <summary>The reserved words of the grammar, one parser each (e.g. <c>K.Select</c>).</summary>
internal static class K
{
    public static readonly Parser<string> All = Lex.Keyword("ALL");
    public static readonly Parser<string> And = Lex.Keyword("AND");
    public static readonly Parser<string> Any = Lex.Keyword("ANY");
    public static readonly Parser<string> As = Lex.Keyword("AS");
    public static readonly Parser<string> Asc = Lex.Keyword("ASC");
    public static readonly Parser<string> Between = Lex.Keyword("BETWEEN");
    public static readonly Parser<string> By = Lex.Keyword("BY");
    public static readonly Parser<string> Case = Lex.Keyword("CASE");
    public static readonly Parser<string> Cast = Lex.Keyword("CAST");
    public static readonly Parser<string> Cross = Lex.Keyword("CROSS");
    public static readonly Parser<string> Desc = Lex.Keyword("DESC");
    public static readonly Parser<string> Distinct = Lex.Keyword("DISTINCT");
    public static readonly Parser<string> Else = Lex.Keyword("ELSE");
    public static readonly Parser<string> End = Lex.Keyword("END");
    public static readonly Parser<string> Escape = Lex.Keyword("ESCAPE");
    public static readonly Parser<string> Except = Lex.Keyword("EXCEPT");
    public static readonly Parser<string> Exists = Lex.Keyword("EXISTS");
    public static readonly Parser<string> From = Lex.Keyword("FROM");
    public static readonly Parser<string> Full = Lex.Keyword("FULL");
    public static readonly Parser<string> Group = Lex.Keyword("GROUP");
    public static readonly Parser<string> Having = Lex.Keyword("HAVING");
    public static readonly Parser<string> In = Lex.Keyword("IN");
    public static readonly Parser<string> Inner = Lex.Keyword("INNER");
    public static readonly Parser<string> Intersect = Lex.Keyword("INTERSECT");
    public static readonly Parser<string> Is = Lex.Keyword("IS");
    public static readonly Parser<string> Join = Lex.Keyword("JOIN");
    public static readonly Parser<string> Left = Lex.Keyword("LEFT");
    public static readonly Parser<string> Like = Lex.Keyword("LIKE");
    public static readonly Parser<string> Natural = Lex.Keyword("NATURAL");
    public static readonly Parser<string> Not = Lex.Keyword("NOT");
    public static readonly Parser<string> Null = Lex.Keyword("NULL");
    public static readonly Parser<string> On = Lex.Keyword("ON");
    public static readonly Parser<string> Or = Lex.Keyword("OR");
    public static readonly Parser<string> Order = Lex.Keyword("ORDER");
    public static readonly Parser<string> Outer = Lex.Keyword("OUTER");
    public static readonly Parser<string> Right = Lex.Keyword("RIGHT");
    public static readonly Parser<string> Select = Lex.Keyword("SELECT");
    public static readonly Parser<string> Some = Lex.Keyword("SOME");
    public static readonly Parser<string> Then = Lex.Keyword("THEN");
    public static readonly Parser<string> Union = Lex.Keyword("UNION");
    public static readonly Parser<string> Using = Lex.Keyword("USING");
    public static readonly Parser<string> When = Lex.Keyword("WHEN");
    public static readonly Parser<string> Where = Lex.Keyword("WHERE");
}
