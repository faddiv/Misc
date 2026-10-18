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

### Reference grammar

[`grammar/sql92-select.ebnf`](grammar/sql92-select.ebnf) is the library-neutral definition of the language, and
every implementation follows it. It is left-factored (a shared prefix is written once), so recursive-descent and PEG
parsers can follow it rule by rule without backtracking. LALR and ALL(*) implementations spell some alternatives out
separately and quote the reference text above them.

All seven implementations are laid out the same way:

- one rule per EBNF rule, with the same name and order, each preceded by the EBNF text it implements;
- one file per grammar section: `*.Statement.cs`, `*.Queries.cs`, `*.FromClause.cs`, `*.Expressions.cs`
  (ANTLR: `Grammar/Sql92Select.g4` plus the same split in `AstBuilder/`);
- tokens in `Lexical.cs` (keywords in `Keywords.cs`, e.g. `K.Select`, for the combinator libraries);
- AST construction in named methods (`Build.QuerySpecification`, ...), so the rules contain only grammar.
  Irony keeps each rule's conversion (`ToAst`) right under its BNF; ANTLR has one `ToAst` overload per rule context.

| Library | How rules refer to each other before they are defined |
|---|---|
| Parlot | `Rule<T>`, a minimal `Parser<T>` subclass |
| Pidgin | `Rule<T>`, overriding `Parser<char, T>.TryParse` |
| Superpower, Sprache | `Rule.Create` / `rule.Define(...)`: a parser delegate bound to a small forwarding object |
| Irony | `Rule<T> : NonTerminal`, which also carries the `ToAst` conversion |
| Farkle | built in: `Nonterminal.Create<T>()` and `SetProductions(...)` later |
| ANTLR | not needed: the `.g4` file is compiled by the ANTLR tool |

Lessons from keeping the restructured parsers at least as fast as the originals:

- **Parlot:** `Deferred<T>` does cycle detection and recursion tracking on every call; using it for every rule made
  parsing 1.4-1.7x slower and added about 670 bytes per parse. A plain forward reference (`Rule<T>`) is enough
  because the grammar has no left recursion.
- **Pidgin and Sprache:** repetitions come back as `IEnumerable<T>`; a `foreach` over them boxes the enumerator and
  allocates for every repetition, while LINQ (`Aggregate`, `ToArray`) takes a fast path.
- **Pidgin:** writing `('+' | '-') unaryExpression` as `Map(sign, unaryExpression)` cost about 3%, because every
  primary passes through that rule. It is written as two alternatives.
- **Superpower and Sprache:** in a LINQ query only the first `from` source is evaluated once; later sources are
  evaluated on every parse. Combinators used there (`Optional()`, `Many()`, `Or`, ...) are built beforehand into
  local variables.
- **Irony and ANTLR:** the `expression -> orExpression` pass-through level was merged away, saving a parse-tree node
  per expression. A separate `alias` rule cost ANTLR 2-4% (a rule call and a prediction per select item), so it is
  inlined.
- **Farkle:** lists written as explicit rules need an initial capacity of 4 to allocate the same as `SeparatedBy`.

### Optimizations

After the restructuring, each parser was profiled and its hot spots were fixed. The grammar code itself is
unchanged, apart from the noted exceptions. The table compares the BenchmarkDotNet runs before
([`BenchmarkResults/`](BenchmarkResults/)) and after ([`BenchmarkResults_Optimized/`](BenchmarkResults_Optimized/))
the optimizations, as ranges over the four queries.

| Library | Speedup | Allocated | What changed |
|---|---|---|---|
| Sprache | 3.8-5.0x | -82..-83% | `SourceInput`: Sprache's `Input` allocates an object and a memo dictionary for every character it advances over; this one creates each position once per parse. Primitive keyword/identifier/symbol/trivia parsers. Allocation-free `SelectMany`, `Select`, `Return`, `Then`, `Or`, `Many`, `AtLeastOnce`, `DelimitedBy`, `Optional`, `ChainOperator` (Sprache builds new parsers and closures while parsing). |
| Superpower | 3.1-3.9x | -77..-81% | A hand-written tokenizer instead of `TokenizerBuilder` with regexes; every keyword gets its own token kind. `Lex.Kind` instead of `Token.EqualTo`, which boxes the enum on every comparison. Allocation-free `SelectMany`, `Many`, `AtLeastOnceDelimitedBy`. Single-token operator sets (`Lex.OneOf`), and `primary`'s alternatives in frequency order. |
| Pidgin | 2.3-2.8x | -73..-77% | Primitive keyword/identifier/trivia parsers (`CIString` lower-cases through the culture tables, and trivia skipping recorded two failures after every token). A per-thread array pool for Pidgin's "expected" lists instead of `ArrayPool<T>.Shared`. |
| Parlot | 1.6-1.7x | 0..-10% | Primitive keyword/identifier/trivia parsers that implement `ISeekable`, so `OneOf` jumps straight to the alternatives that can start with the next character. (`Compile()` did not help: it needs `Deferred` rules, which are slower.) |
| Irony | 1.2-1.6x | -29..-41% | Key terms compared ordinally: Irony compares with culture-aware `InvariantCultureIgnoreCase`, which is slow (especially with ICU on Linux). A quoted-literal terminal for the same reason. Transient expression levels, so a plain column is no longer a chain of ten parse-tree nodes (each level is a transient choice plus an `xOperation` rule). |
| ANTLR | 1.0-1.2x | -12..-19% | Two-stage parsing (SLL with bail-out, LL only on failure), a reused lexer/parser per thread, and AST conversions that take a single child directly instead of calling the list accessors, which allocate. |
| Farkle | unchanged | unchanged | The time is spent in Farkle's LR and DFA loops, and it allocates little more than the AST. Folding the expression levels into one rule with operator precedence would save reductions, but would no longer follow the reference grammar. |

The primitive token parsers use each library's documented extension point (a `Parser` subclass, or a delegate for
the delegate-based libraries), but they bypass the library's own combinators for the lexical level. For the
combinator libraries the numbers therefore show what is possible with them, not their out-of-the-box speed; the
previous run in `BenchmarkResults/` shows the latter.

### Results

Optimized parsers, BenchmarkDotNet on Windows (AMD Ryzen 5 2600, .NET 10;
[`BenchmarkResults_Optimized/`](BenchmarkResults_Optimized/)). Mean time and allocated memory per parse. The rank
uses the geometric mean, over the four queries, of each value divided by the best value for that query
("vs best": 1.00x would be the best on every query).

**Speed**

| Rank | Library | Simple | Medium | Complex | Large | vs best |
|---|---|---:|---:|---:|---:|---:|
| 1 | Parlot | 3.7 µs | 22.5 µs | 44.4 µs | 750.0 µs | 1.01x |
| 2 | Farkle | 3.6 µs | 25.8 µs | 49.2 µs | 867.1 µs | 1.10x |
| 3 | Superpower | 10.6 µs | 61.5 µs | 123.9 µs | 2.33 ms | 2.90x |
| 4 | ANTLR | 10.4 µs | 77.1 µs | 144.3 µs | 3.94 ms | 3.62x |
| 5 | Irony | 11.1 µs | 92.7 µs | 172.1 µs | 3.76 ms | 3.98x |
| 6 | Pidgin | 15.5 µs | 89.5 µs | 169.3 µs | 3.91 ms | 4.31x |
| 7 | Sprache | 22.7 µs | 141.1 µs | 304.6 µs | 7.39 ms | 7.21x |

**Memory**

| Rank | Library | Simple | Medium | Complex | Large | vs best |
|---|---|---:|---:|---:|---:|---:|
| 1 | Farkle | 1.1 KB | 6.7 KB | 11.8 KB | 168.8 KB | 1.00x |
| 2 | Parlot | 1.3 KB | 8.3 KB | 13.2 KB | 252.9 KB | 1.26x |
| 3 | Pidgin | 1.5 KB | 9.3 KB | 15.5 KB | 270.1 KB | 1.42x |
| 4 | Superpower | 4.1 KB | 28.4 KB | 55.2 KB | 1.03 MB | 4.69x |
| 5 | Irony | 11.6 KB | 82.2 KB | 145.4 KB | 2.61 MB | 12.75x |
| 6 | ANTLR | 12.4 KB | 82.1 KB | 137.6 KB | 2.99 MB | 13.22x |
| 7 | Sprache | 37.6 KB | 243.1 KB | 435.2 KB | 9.71 MB | 40.97x |

Parlot and Farkle are close: Farkle is slightly faster on the simplest query and allocates the least everywhere,
Parlot is 11-16% faster on the larger ones. On the larger queries Pidgin, Irony and ANTLR take about the same time, but Pidgin allocates
about a tenth of what the two parse-tree builders allocate.

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
- ANTLR parses in two stages: SLL prediction with a bail-out error strategy first, and full LL prediction with
  a throwing error listener only if that fails.

## Package versions

`Directory.Build.props` turns on `RestorePackagesWithLockFile`. The `*.csproj` files use floating versions
(for example `Pidgin 3.*`), and each project's `packages.lock.json` records the exact versions that were restored.
Commit the lock files so that later benchmark runs use the same libraries.
