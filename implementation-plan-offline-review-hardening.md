# Offline Knowledge + Review Heuristics Implementation Plan

**Goal:** Remove DeepWiki/runtime dependence from ReactiveMcp.Server responses and add richer C# review heuristics with best-practice snippet guidance.

**Architecture:** Keep the MCP server fully offline-first by embedding all guidance directly in catalog manifests and response models. Extend review results so findings can include evidence, rationale, and concrete replacement snippets for common reactive C# anti-patterns.

**Tasks:**
1. Add failing tests for offline manifest content and richer review output.
2. Extend review/result models to carry snippet guidance.
3. Replace DeepWiki-linked catalog/source metadata with offline-first local content.
4. Add new heuristics for real C# snippets: undisposed subscriptions, Subject-as-state, async void, sleep-based timing tests, event-handler conversion, and scheduler boundary issues.
5. Rebuild and rerun the full test suite.
