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


The server is implemented in C# on `net10.0` using:
- `ModelContextProtocol` `1.2.0`

## Quick Install

Click to install in your preferred environment:

[![VS Code - Install Reactive MCP](https://img.shields.io/badge/VS_Code-Install_Reactive_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=reactive-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.Reactive.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)
[![VS Code Insiders - Install Reactive MCP](https://img.shields.io/badge/VS_Code_Insiders-Install_Reactive_MCP-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=reactive-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.Reactive.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D&quality=insiders)
[![Visual Studio - Install Reactive MCP](https://img.shields.io/badge/Visual_Studio-Install_Reactive_MCP-5C2D91?style=flat-square&logo=visualstudio&logoColor=white)](https://vs-open.link/mcp-install?%7B%22name%22%3A%22CP.Reactive.Mcp.Server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.Reactive.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)

Note:
- These install links are prepared for the intended NuGet package identity `CP.ReactiveUI.Mcp.Server`.
- If the latest package has not been published yet, use the manual source-build configuration below.


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


## MCP metadata

Server metadata file:
- `.mcp/server.json`

Current working identifiers:
- MCP server name: `io.github.chrispulman/reactive-mcp-server`
- package id: `CP.Reactive.Mcp.Server`
- version: `0.1.0`

## Notes for future publishing

Before publishing the package, update:
- package metadata in `src/ReactiveMcp.Server/ReactiveMcp.Server.csproj`
- version in `.mcp/server.json`
- install badge links if the package id or version changes

## License

MIT License — see `LICENSE`.

---

**CP.Reactive.Mcp.Server** - Empowering Agentic Automation with Reactive Technology ⚡🏭