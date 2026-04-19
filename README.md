# Reactive MCP Server

<!-- mcp-name: io.github.chrispulman/reactive-mcp-server -->

Reactive MCP Server is a .NET 10 Model Context Protocol server for helping AI agents and developers design, debug, review, and modernize Reactive Extensions based applications.

It ships with an embedded offline knowledge catalog, so MCP clients can use the server without any DeepWiki or web dependency at runtime.

The server exposes guidance for:

- Rx.NET architecture and core abstractions
- push vs pull sequence design
- observable operators and scheduler selection
- subject selection and subject misuse detection
- deterministic testing with `TestScheduler` and virtual time
- performance and benchmark-aware review guidance
- migration from ad hoc events and legacy reactive code toward clearer pipelines
- choosing between `IObservable<T>`, `IAsyncEnumerable<T>`, Ix, and AsyncRx
- richer review heuristics for real C# snippets, including example replacement code

## Offline-first behavior

All MCP guidance is served from embedded manifests and compiled code-review heuristics in this repository.
No runtime internet access is required for catalog lookups, reviews, prompts, or debugging guidance.

## Available MCP tools

- `reactive_catalog_list`
- `reactive_catalog_search`
- `reactive_catalog_get`
- `reactive_recommend`
- `reactive_review_plan`
- `reactive_review_csharp`
- `reactive_compare`
- `reactive_debug_checklist`
- `reactive_upgrade_plan`
- `reactive_scaffold_prompt`

## Available MCP resources

- `reactive://catalog`
- `reactive://ecosystem/{id}`
- `reactive://best-practices/debugging`

## Available MCP prompts

- `create_reactive_app`
- `debug_reactive_pipeline`
- `modernize_reactive_codebase`

## Richer review heuristics

`reactive_review_plan` now produces stronger feedback for real C# snippets and plans, including:

- undisposed `Subscribe(...)` calls
- `Subject<T>` used as a mutable state store
- `async void`
- sleep-based timing tests
- event-handler code that should be converted with `Observable.FromEventPattern`
- scheduler-boundary issues involving `ObserveOn`, `SubscribeOn`, UI threads, or dispatchers

Findings can include best-practice replacement snippets so the review output is directly actionable.
The dedicated `reactive_review_csharp` tool is intended for reviewing real C# snippets separately from broader plan review.

## Build from source

```bash
sln=$(wslpath -w /mnt/d/Projects/Github/chrispulman/ReactiveMcp.Server/src/ReactiveMcpServer.slnx)
"/mnt/c/Program Files/dotnet/dotnet.exe" build "$sln"
```

## Run tests from WSL

```bash
cd /mnt/d/Projects/Github/chrispulman/ReactiveMcp.Server/src/ReactiveMcp.Tests/bin/Debug/net10.0
"/mnt/c/Program Files/dotnet/dotnet.exe" test --test-modules ReactiveMcp.Tests.dll --output Detailed --no-progress
```
