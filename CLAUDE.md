# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository overview

This repository is a collection of unrelated experiments, practice projects, and small tools. Each top-level folder is an independent project; there is no root solution, shared build, or CI.

We always work with a single subfolder at a time. The user names it at the start of the session. Stay inside that folder, and don't assume that conventions, dependencies, or SDK versions from one project apply to another. If no folder was named, ask which one.

## Working in a project

Most projects are C# solutions, using `.slnx` or the older `.sln` format. Some are JavaScript/TypeScript (identified by `package.json`). Some C# projects have a frontend too, for example `ViteCommerce` and `IndexHtmlReWriter`.

Run commands from the project folder, passing the solution explicitly:

```bash
dotnet build <Solution>.slnx
```

```bash
dotnet test <Solution>.slnx --filter "FullyQualifiedName~<TestName>"
```

Before running anything, check the project's own setup, because it varies:

- `global.json` pins the SDK and can select the test runner. `DSL_experiment` uses Microsoft.Testing.Platform, so its `dotnet test` arguments may differ from the form above.
- Only some projects have tests, in `*.Tests` or `*.Test` projects.
- JS projects: read the scripts in `package.json`.
- A project's `README.md`, when present, is the reference for its design and results.

## Code style

The root `.editorconfig` applies everywhere: CRLF line endings, UTF-8, final newline, 130-column limit, and 4-space indent (2 for JS/TS, HTML, JSON, and project files). JS/TS uses double quotes.
