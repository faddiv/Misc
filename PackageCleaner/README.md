# PackageCleaner

NuGet keeps every package version it ever downloaded, so old versions pile up in the global packages folder.
`dotnet nuget locals all --clear` frees the space, but it also removes the packages still in use, and they all have to be
downloaded again. PackageCleaner removes only the packages that no project under the given folders uses.

More cleaners can be added later. Running the tool without a command runs all of them. NuGet is the only one so far.

## Install

From this folder:

```bash
dotnet pack PackageCleaner -o nupkg
```

```bash
dotnet tool install --global PackageCleaner --add-source nupkg
```

To run it without installing, use `dotnet run --project PackageCleaner -- <arguments>`.

## Usage

```
package-cleaner <folders>... [--dry-run] [--yes] [--packages-path <dir>]         run all cleaners
package-cleaner nuget <folders>... [--dry-run] [--yes] [--packages-path <dir>]   run the NuGet cleaner only
```

| Option | Meaning |
|---|---|
| `<folders>` | One or more folders to scan. Required. Packages used by any of them are kept. |
| `--dry-run` | List what would be deleted, then stop. |
| `-y`, `--yes` | Delete without asking. Needed when input is not interactive. |
| `--packages-path <dir>` | Packages folder to clean, instead of the one NuGet is configured to use. |

A normal run lists what it found and the total size, asks `y/N`, then deletes:

```bash
package-cleaner nuget D:\Projects D:\Work --dry-run
```

```bash
package-cleaner D:\Projects D:\Work
```

The exit code is 0 on success, including when you answer no, and 1 on an error or if any deletion failed.

A folder literally named `nuget` is read as the command, so write it as `.\nuget`.

## Before you run it

The packages folder is shared by every project on the machine, and anything not found under the given folders counts as
unused. **Pass every folder that holds projects you care about**, and start with `--dry-run`.

A deleted package that turns out to be needed is downloaded again by the next `dotnet restore`, unless it is no longer
available from its feed.

## How it works

### Finding projects

It recurses through the given folders, looking for `*.csproj`, `*.fsproj`, `*.vbproj` and `Directory.Packages.props`.

- A folder with a project file is not searched any deeper.
- `.git`, `node_modules`, `bin`, `obj`, `.vs` and `.idea` are skipped, as are symbolic links and junctions.
- `packages.config` is not supported.

### Deciding what is used

A project file lists only direct references, and can't resolve versions like `1.*`. So the main source is
`obj/project.assets.json`, which `dotnet restore` writes with every package a project uses, including transitive ones, at the
exact versions NuGet chose.

These also count as used:

| Source | Example |
|---|---|
| `PackageDownload` items in the assets file | `microsoft.netcore.app.ref` |
| `<PackageVersion>` and `<GlobalPackageReference>` in `Directory.Packages.props` | Even if no restored project uses them |
| MSBuild SDKs in restored projects | `Sdk="Aspire.AppHost.Sdk/13.6.1"`, `<Sdk Name Version>`, `<Import Sdk>` |
| `msbuild-sdks` in `global.json` | |
| Local tools in `dotnet-tools.json` | They restore into the same folder |

A plain version such as `1.2.3` keeps exactly that version. A floating version or a range keeps every cached version inside it.
A version that can't be read, such as `$(SomeVersion)`, keeps every cached version of that package.

### Deleting

The packages folder comes from NuGet's own configuration: `NUGET_PACKAGES`, then `nuget.config`, then the default location.

- Only `<id>/<version>/` folders that contain `.nupkg.metadata` or `*.nupkg.sha512` are touched.
- A folder is renamed to `.deleting-<version>` before it is deleted. If a file is locked, the rename fails, so the package stays
  whole, gets reported, and the run continues. Leftovers from an interrupted delete are removed on the next run.
- The folder of a package is removed along with its last version.

The run stops with nothing deleted if no restored project is found, which usually means a wrong folder, or if a
`project.assets.json` can't be read. A malformed project, props, `global.json` or tool manifest file only gets a warning.

## Limitations

- **Unrestored projects are ignored.** A project with no `obj/project.assets.json` counts as using nothing, and it is listed in
  the output. Run `dotnet restore` first in any project whose packages you want to keep.
- **Custom obj locations look unrestored.** This applies to `UseArtifactsOutput` and `BaseIntermediateOutputPath`.
- **Nested projects are not found**, because the search stops at the first project file.
- **Assets files can be out of date.** A stale file keeps old versions until the project is restored again.
- **Only the global packages folder is cleaned.** The HTTP cache (`v3-cache`) and other NuGet folders are left alone.

## Adding a cleaner

1. Implement `ICleaner`: `Scan` finds what can go without changing anything, and `Delete` removes one item.
2. In `CommandLineApp`, add it to the root action, and add a subcommand for it.

`CleanerRunner` handles the listing, the confirmation and failed deletions.

## Development

```bash
dotnet build PackageCleaner.slnx
```

```bash
dotnet test --solution PackageCleaner.slnx
```

Tests use TUnit on Microsoft.Testing.Platform, which is selected in `global.json`. They work on real temporary folders and
never touch the real packages folder.
