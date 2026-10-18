using Pidgin;

namespace SqlParsers.Pidgin;

/// <summary>The reserved words of the grammar, one parser each (e.g. <c>K.Select</c>).</summary>
internal static class K
{
    public static readonly Parser<char, Unit> All = Lex.Keyword("ALL");
    public static readonly Parser<char, Unit> And = Lex.Keyword("AND");
    public static readonly Parser<char, Unit> Any = Lex.Keyword("ANY");
    public static readonly Parser<char, Unit> As = Lex.Keyword("AS");
    public static readonly Parser<char, Unit> Asc = Lex.Keyword("ASC");
    public static readonly Parser<char, Unit> Between = Lex.Keyword("BETWEEN");
    public static readonly Parser<char, Unit> By = Lex.Keyword("BY");
    public static readonly Parser<char, Unit> Case = Lex.Keyword("CASE");
    public static readonly Parser<char, Unit> Cast = Lex.Keyword("CAST");
    public static readonly Parser<char, Unit> Cross = Lex.Keyword("CROSS");
    public static readonly Parser<char, Unit> Desc = Lex.Keyword("DESC");
    public static readonly Parser<char, Unit> Distinct = Lex.Keyword("DISTINCT");
    public static readonly Parser<char, Unit> Else = Lex.Keyword("ELSE");
    public static readonly Parser<char, Unit> End = Lex.Keyword("END");
    public static readonly Parser<char, Unit> Escape = Lex.Keyword("ESCAPE");
    public static readonly Parser<char, Unit> Except = Lex.Keyword("EXCEPT");
    public static readonly Parser<char, Unit> Exists = Lex.Keyword("EXISTS");
    public static readonly Parser<char, Unit> From = Lex.Keyword("FROM");
    public static readonly Parser<char, Unit> Full = Lex.Keyword("FULL");
    public static readonly Parser<char, Unit> Group = Lex.Keyword("GROUP");
    public static readonly Parser<char, Unit> Having = Lex.Keyword("HAVING");
    public static readonly Parser<char, Unit> In = Lex.Keyword("IN");
    public static readonly Parser<char, Unit> Inner = Lex.Keyword("INNER");
    public static readonly Parser<char, Unit> Intersect = Lex.Keyword("INTERSECT");
    public static readonly Parser<char, Unit> Is = Lex.Keyword("IS");
    public static readonly Parser<char, Unit> Join = Lex.Keyword("JOIN");
    public static readonly Parser<char, Unit> Left = Lex.Keyword("LEFT");
    public static readonly Parser<char, Unit> Like = Lex.Keyword("LIKE");
    public static readonly Parser<char, Unit> Natural = Lex.Keyword("NATURAL");
    public static readonly Parser<char, Unit> Not = Lex.Keyword("NOT");
    public static readonly Parser<char, Unit> Null = Lex.Keyword("NULL");
    public static readonly Parser<char, Unit> On = Lex.Keyword("ON");
    public static readonly Parser<char, Unit> Or = Lex.Keyword("OR");
    public static readonly Parser<char, Unit> Order = Lex.Keyword("ORDER");
    public static readonly Parser<char, Unit> Outer = Lex.Keyword("OUTER");
    public static readonly Parser<char, Unit> Right = Lex.Keyword("RIGHT");
    public static readonly Parser<char, Unit> Select = Lex.Keyword("SELECT");
    public static readonly Parser<char, Unit> Some = Lex.Keyword("SOME");
    public static readonly Parser<char, Unit> Then = Lex.Keyword("THEN");
    public static readonly Parser<char, Unit> Union = Lex.Keyword("UNION");
    public static readonly Parser<char, Unit> Using = Lex.Keyword("USING");
    public static readonly Parser<char, Unit> When = Lex.Keyword("WHEN");
    public static readonly Parser<char, Unit> Where = Lex.Keyword("WHERE");
}
