<!--
Sync Impact Report
- Version change: 0.0.0 -> 1.0.0 (initial ratification)
- Modified principles: I. Component-First Modularity; II. Strongly Typed Task Domain; III. Responsive & Accessible Experience; IV. Test-Led Observability; V. Performance-Safe Simplicity
- Added sections: Technical Stack & Constraints; Delivery Workflow & Quality Gates
- Removed sections: None
- Templates requiring updates (updated/pending):
  - updated: .specify/templates/plan-template.md
  - updated: .specify/templates/spec-template.md
  - updated: .specify/templates/tasks-template.md
- Follow-up TODOs: None
-->

# BlazorTodoApp Constitution

## Core Principles

### I. Component-First Modularity
- Razor UI MUST be built from focused components whose markup stays under 250 lines; split views as soon as they mix responsibilities.
- Component logic MUST live in `.razor.cs` partial classes that inject services via DI; markup files only handle bindings and events.
- Shared state flows through dedicated state containers or cascading parameters, never through static or global variables.
- Rationale: This keeps features independently testable, makes refactors cheap, and satisfies the "clean and modular code" mandate.

### II. Strongly Typed Task Domain
- `TodoItem` records (Id, Title, Notes, Priority, DueDate, CompletedAt) are the only writing path to persistence; every mutation goes through `ITodoRepository`.
- Validation MUST use DataAnnotations or FluentValidation, rejecting empty titles, duplicate IDs, and due dates in the past unless explicitly snoozed.
- State transitions are captured via domain events (`TaskCreated`, `TaskCompleted`, `TaskReopened`) to keep the UI and storage in sync.
- Rationale: Enforcing a single source of truth prevents inconsistent task state across pages and enables reliable synchronization.

### III. Responsive & Accessible Experience
- Every interactive element exposes keyboard shortcuts, focus management, and ARIA labels that meet WCAG 2.1 AA.
- Forms MUST use `<EditForm>` with Blazor input components and display validation feedback within 150 ms of user action.
- Visual updates MUST be double-buffered (e.g., `ObservableCollection` or immutable snapshots) so completing or editing a task never blocks the UI thread.
- Rationale: Accessibility and instant feedback are non-negotiable for a productivity app and drive user trust.

### IV. Test-Led Observability
- Features start with tests: `xUnit` for repositories, `bUnit` for components, and Playwright for high-value flows; CI fails if coverage < 85% on touched files.
- Each state mutation logs a structured message via `ILogger<T>` plus emits Activity events so diagnostics tooling can reconstruct a task timeline.
- Telemetry (Application Insights-compatible) MUST include `task_id`, `action`, and `latency_ms` fields for adds, edits, deletes, and completions.
- Rationale: Observability allows confident iteration and supports regression-free releases.

### V. Performance-Safe Simplicity
- The project targets .NET 9 / C# 13 with nullable reference types, analyzers, and `dotnet format` enforced before merging.
- Client payload (compressed) must stay under 150 KB, and any render path must stay under 50 ms on a mid-tier laptop; prefer incremental rendering over full reloads.
- JS interop is a last resort; when unavoidable, wrap it in a typed service and back it with unit tests.
- Rationale: Simplicity keeps the todo experience fast, ensures maintainability, and honors the user's request for best-practice Blazor code.

## Technical Stack & Constraints
- Runtime: Blazor Web App on .NET 9 with interactive WASM components plus server-side data endpoints.
- Language & Tools: C# 13, nullable reference types, StyleCop analyzers, `dotnet format`, and Git hooks that block merges on warning-level diagnostics.
- Data: EF Core 9 with SQLite for local development and an abstraction-friendly `ITodoRepository` for future storage (Cosmos DB/Azure SQL) swaps.
- UI: Bootstrap 5 theme with project-wide CSS lives in `wwwroot/css/app.css`; dark mode variations must reuse tokens defined there.
- Testing: `dotnet test`, `bunit` for components, `Playwright` smoke tests run via `dotnet test --filter Category=E2E`.
- Deployment: GitHub Actions pipeline must restore, build, test, publish, and run `dotnet format --verify-no-changes`.

## Delivery Workflow & Quality Gates
- Every feature begins with `/speckit.specify` (spec), `/speckit.plan`, and `/speckit.tasks` so work remains traceable to user value.
- Branch naming follows `feature/<issue-id>-<slug>` and cannot be merged without an attached plan + spec.
- Plans must enumerate Razor components, repositories, and telemetry events per Principles I-IV; missing details block kickoff.
- Pull requests require successful CI (format + tests), an accessibility review (keyboard + screen reader smoke tests), and confirmation that telemetry names did not change without migration notes.
- Deployments happen only after observability dashboards show no regressions during staging for 24 hours or after manual sign-off when urgent.

## Governance
- This constitution supersedes any conflicting documentation; exceptions require written approval from the project owner and a mitigation plan.
- Amendments: propose via PR updating this file, review by at least two maintainers (one focused on architecture, one on QA), and attach impact notes. Approved amendments bump the version per semantic rules.
- Compliance: release managers review adherence every sprint by sampling at least one feature and checking component decomposition, validation, accessibility, telemetry, and performance evidence.
- Enforcement: violations halt merges until remedied; repeated non-compliance can freeze feature work until an action plan is documented.
- Versioning policy: MAJOR for removing/replacing principles, MINOR for new principles/sections, PATCH for clarifications or governance wording changes.

**Version**: 1.0.0 | **Ratified**: 2025-11-21 | **Last Amended**: 2025-11-21
