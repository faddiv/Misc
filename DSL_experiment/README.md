# DSL experiment: SQL-92 SELECT parsers in .NET

Compares the speed and memory use of seven .NET parser libraries. Each one parses the same SQL-92 `SELECT`
grammar into the same AST.

| Project | Library | Style |
|---|---|---|
| `SqlParsers.Pidgin` | [Pidgin](https://github.com/benjamin-hodgson/Pidgin) | Scannerless combinators |
| `SqlParsers.Superpower` | [Superpower](https://github.com/datalust/superpower) | Tokenizer + token combinators |
| `SqlParsers.Parlot` | [Parlot](https://github.com/sebastienros/parlot) | Scannerless fluent combinators |
| `SqlParsers.Sprache` | [Sprache](https://github.com/sprache/Sprache) | Scannerless, fully backtracking combinators |
| `SqlParsers.Antlr` | [ANTLR 4](https://www.antlr.org/) | Generated ALL(*) lexer/parser + tree conversion |
| `SqlParsers.Irony` | [Irony](https://github.com/IronyProject/Irony) | LALR(1) grammar in C# + tree conversion |
| `SqlParsers.Farkle` | [Farkle](https://github.com/teo-tsirpanis/Farkle) | LALR(1)/IELR(1) grammar in C#, semantic actions build the AST directly |

### What each parser can handle

What a library can parse depends on its algorithm and grammar class, not on this project. The SQL subset here
is deliberately small enough that all seven can parse it.

| Library | Algorithm | Grammar class | Languages it can parse |
|---|---|---|---|
| Pidgin | [Parser combinators](https://en.wikipedia.org/wiki/Parser_combinator), [recursive descent](https://en.wikipedia.org/wiki/Recursive_descent_parser) | [LL(1)](https://en.wikipedia.org/wiki/LL_parser) by default; arbitrary lookahead with `Try` | [Context-sensitive languages](https://en.wikipedia.org/wiki/Context-sensitive_language), because a monadic `Bind` can choose the next parser from earlier results |
| Superpower | [Parser combinators](https://en.wikipedia.org/wiki/Parser_combinator) over a token list | [LL(1)](https://en.wikipedia.org/wiki/LL_parser) by default; arbitrary lookahead with `Try()` | [Context-sensitive languages](https://en.wikipedia.org/wiki/Context-sensitive_language), through monadic `Then` (LINQ) |
| Parlot | [Parser combinators](https://en.wikipedia.org/wiki/Parser_combinator), always backtracking | [Parsing expression grammar](https://en.wikipedia.org/wiki/Parsing_expression_grammar) (ordered choice) | [PEG-recognizable languages](https://en.wikipedia.org/wiki/Parsing_expression_grammar) |
| Sprache | [Parser combinators](https://en.wikipedia.org/wiki/Parser_combinator), always backtracking | [Parsing expression grammar](https://en.wikipedia.org/wiki/Parsing_expression_grammar) (ordered choice) | [Context-sensitive languages](https://en.wikipedia.org/wiki/Context-sensitive_language), through monadic LINQ `SelectMany` |
| ANTLR 4 | Adaptive [LL(*)](https://en.wikipedia.org/wiki/LL_parser), ALL(*) (see [ANTLR](https://en.wikipedia.org/wiki/ANTLR)) | Any [context-free grammar](https://en.wikipedia.org/wiki/Context-free_grammar) without indirect left recursion | [Context-free languages](https://en.wikipedia.org/wiki/Context-free_language) (semantic predicates go beyond them) |
| Irony | [LALR parser](https://en.wikipedia.org/wiki/LALR_parser) | [LALR(1)](https://en.wikipedia.org/wiki/LALR_parser) | [Deterministic context-free languages](https://en.wikipedia.org/wiki/Deterministic_context-free_language) |
| Farkle | [LALR parser](https://en.wikipedia.org/wiki/LALR_parser), or IELR(1) when configured, with a [DFA](https://en.wikipedia.org/wiki/Deterministic_finite_automaton) tokenizer | [LALR(1)](https://en.wikipedia.org/wiki/LALR_parser), or [LR(1)](https://en.wikipedia.org/wiki/Canonical_LR_parser) grammars with IELR(1) | [Deterministic context-free languages](https://en.wikipedia.org/wiki/Deterministic_context-free_language) |

LALR(1) and LR(1) recognize the same set of languages. They differ in which grammars they accept without
conflicts, which is why Farkle's IELR(1) option can accept grammars that Irony rejects.

`SqlParsers.Core` holds the shared AST (`Ast.cs`), `ISqlParser`, the reserved words, the benchmark queries
and `SqlFormatter`. `SqlFormatter` writes a fully parenthesized canonical form, and the tests use it to compare trees.

## Requirements

- .NET 10 SDK
- Network access on the first build. NuGet packages are restored, and **Antlr4BuildTasks** downloads a Java
  runtime and the ANTLR tool jar to generate the ANTLR parser.

## Build, test, benchmark

```powershell
dotnet build
dotnet test                       # TUnit on Microsoft.Testing.Platform (configured in global.json)
dotnet run -c Release --project benchmarks/SqlParsers.Benchmarks -- --filter *
```

Run a subset of the benchmarks with a filter, for example `--filter *Parse*`. BenchmarkDotNet puts its reports in `BenchmarkDotNet.Artifacts/`.

## What the tests check

- **Expected trees:** each parser is checked against 25 valid queries (`TestQueries.Valid`), comparing the canonical form.
- **Invalid input:** every parser must throw `SqlParseException` on the queries in `TestQueries.Invalid`.
- **Benchmark inputs:** all parsers must produce identical trees for every query in `SampleQueries`. Without this, the
  benchmark would not compare like with like.

## Supported grammar

- `SELECT [DISTINCT | ALL] select-list FROM table-ref, ... [WHERE] [GROUP BY] [HAVING]`
- `UNION / EXCEPT [ALL]` and `INTERSECT [ALL]`, where `INTERSECT` binds tighter than the others. Parenthesized query expressions and
  `ORDER BY expr [ASC|DESC], ...` on the whole statement are also supported. A trailing `;` is optional.
- Select items: `*`, `t.*`, `expr [[AS] alias]`
- Tables: `name [[AS] alias]`, `(query) [AS] alias`. Joins: `CROSS JOIN`, `[NATURAL] [INNER | LEFT | RIGHT | FULL [OUTER]] JOIN`,
  `ON expr` / `USING (cols)`. Joins are left-associative.
- Expressions, from lowest to highest precedence:
  `OR` < `AND` < `NOT` < predicates (`= <> < > <= >=`, `IS [NOT] NULL`, `[NOT] BETWEEN`, `[NOT] IN`, `[NOT] LIKE ... [ESCAPE]`,
  `op ANY|SOME|ALL (subquery)`) < `+ - ||` < `* /` < unary `+ -` < primary
- Primaries: numeric literals (`1`, `2.5`, `.5`, `3.`, `1.5E3`), `'strings'` with `''` escaping, `NULL`, column references,
  `"quoted identifiers"`, function calls (`COUNT(*)`, `COUNT(DISTINCT x)`, `f(a, b)`, `f()`), `CASE` (simple and searched),
  `CAST(x AS type[(p[, s])])`, `EXISTS (subquery)`, scalar subqueries, parentheses
- Keywords are case-insensitive and reserved. `--` line comments are supported.

### Deliberate limitations

These keep the seven implementations identical:

- A subquery inside an expression (scalar, `IN`, `EXISTS`, `ANY/ALL`) must start with `SELECT`. Without this rule,
  `(` would be ambiguous in LALR(1).
- Parenthesized joined tables, `CORRESPONDING`, `UNION JOIN`, typed date/time/interval literals and special-syntax
  functions (`SUBSTRING(x FROM a FOR b)`, `TRIM`, `EXTRACT`, ...) are not supported.
- A comparison takes one operator, so `a = b = c` is rejected.

## Fairness notes

- Grammar construction (Irony's and Farkle's LALR tables, the combinator graphs, ANTLR's static ATN) happens once
  and is not measured. ANTLR's DFA cache also warms up during the BenchmarkDotNet warmup phase.
- Farkle builds its tables at startup. Its build-time precompiler (`Farkle.Tools.MSBuild`) only speeds up startup,
  so it would not change the measured parse times and is not used.
- Irony's `Parser` is not thread-safe, so `IronySqlParser` keeps one instance per thread. Farkle's `CharParser` is
  immutable and thread-safe.
- Irony and ANTLR build a parse tree first and then convert it to the AST. Farkle, like the combinator libraries,
  builds the AST directly in its semantic actions.
- ANTLR uses the default `LL` prediction mode with a throwing error listener. Switching to `SLL` mode is an easy
  experiment if you want to see how much it changes.

## Package versions

`Directory.Build.props` turns on `RestorePackagesWithLockFile`. The `*.csproj` files use floating versions
(for example `Pidgin 3.*`), and each project's `packages.lock.json` records the exact versions that were restored.
Commit the lock files so that later benchmark runs use the same libraries.
