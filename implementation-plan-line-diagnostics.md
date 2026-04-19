# Line-Oriented Snippet Diagnostics Plan

Goal: extend `reactive_review_csharp` so findings include approximate line-oriented diagnostics for matched reactive anti-patterns.

Planned shape:
- add line metadata to `ReviewFinding`
- preserve existing message/snippet behavior
- compute approximate line numbers from detected pattern matches in the submitted snippet
- keep implementation heuristic-based, deterministic, and offline-first

TDD slice:
1. add failing tests for snippet line diagnostics
2. extend review model
3. implement line-location extraction
4. build and rerun full suite
