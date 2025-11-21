<!--
Sync Impact Report
Version: N/A -> 1.0.0
Modified Principles:
- Established P1 Modular Blazor Composition
- Established P2 Clean Dependency Boundaries
- Established P3 Specification-Backed Delivery
- Established P4 .NET Quality & Testing Discipline
- Established P5 Operational Observability & Accessibility
Added Sections:
- Technology & Tooling Standards
- Development Workflow & Quality Gates
Removed Sections:
- None
Templates Requiring Updates:
- [aligned] .specify/templates/plan-template.md (Constitution Check references these rules already)
- [aligned] .specify/templates/spec-template.md (stories/tests already enforced)
- [aligned] .specify/templates/tasks-template.md (structure already story-centric)
Follow-up TODOs:
- None
-->
# BlazorTodoApp Constitution
<!-- Example: Spec Constitution, TaskFlow Constitution, etc. -->

## Core Principles

### P1. Modular Blazor Composition
Every feature MUST start as a focused Razor component plus an optional partial class for logic. Keep components under 200 lines, colocate CSS in `wwwroot/css` or scoped files, and expose shared UI via `Pages/` or `Layout/` folders. Reuse services instead of duplicating stateful code, and document component inputs/outputs inside XML comments.

### P2. Clean Dependency Boundaries
Respect the stack: Pages -> Components -> Services -> Data/Integration. Services must be registered via dependency injection, depend on interfaces, and run asynchronously when I/O is involved. Shared logic belongs in `Services/` or `Models/`; never call browser APIs directly from business logic. Keep configuration in `appsettings*.json` and bind strongly typed options.

### P3. Specification-Backed Delivery
No implementation begins without an approved `/specs/[feature]/spec.md`, plan.md, and tasks.md generated through Speckit commands. Each document MUST cite the constitution checks that apply, list measurable acceptance criteria, and stay in sync with the resulting pull request description. Deviations require updated specs before code merges.

### P4. .NET Quality & Testing Discipline
Follow .NET 10 best practices: nullable reference types on, warnings treated as errors, `dotnet format` run before every PR. All new logic requires bUnit or xUnit tests with meaningful names and Arrange-Act-Assert structure. `dotnet test` must pass in CI with at least 80% line coverage on touched projects, and UI flows must include screenshot evidence when behavior changes.

### P5. Operational Observability & Accessibility
Instrumentation is non-negotiable: use `ILogger<T>` in services, surface failures with actionable messages, and log structured context. UI changes must satisfy WCAG AA basics (color contrast, keyboard navigation, ARIA labels). Feature toggles rely on configuration or wrapper services, and unhandled errors must route through `ErrorBoundary` components.

## Technology & Tooling Standards
Blazor WebAssembly on `net10.0` is the reference stack. Prefer first-party packages before third-party dependencies and remove unused packages immediately. Configuration flows through `Program.cs` via typed options; secrets live in environment variables or user-secrets, never in source. Use git-tracked JSON files for content, reference assets under `wwwroot/`, and keep generated artifacts under `obj/` or `bin/`. Static analysis must include the default Roslyn analyzers plus any repo-specific analyzers declared in the project file.

## Development Workflow & Quality Gates
Work streams proceed in this order: discover requirements -> `/speckit.plan` -> `/speckit.specify` -> `/speckit.tasks` -> implementation. Each pull request must link the relevant spec folder, include `dotnet build` and `dotnet test` output, and capture manual verification steps for UI. Reviews block if principles P1-P5 are not cited and satisfied. Hotfixes follow the same workflow but may prioritize a single P1 story with explicit approval documented in the PR body.

## Governance
This constitution supersedes prior conventions. Amendments require consensus between the acting agent and repository owner, a documented rationale in the pull request, and simultaneous updates to any impacted templates or docs in `.specify/` or `.codex/`. Version numbers follow semantic rules: MAJOR for added/removed principles, MINOR for substantial guidance changes, PATCH for clarifications. Compliance reviews occur at the start of every plan.md and before merging any feature branch.

**Version**: 1.0.0 | **Ratified**: 2025-11-21 | **Last Amended**: 2025-11-21
<!-- Example: Version: 2.1.1 | Ratified: 2025-06-13 | Last Amended: 2025-07-16 -->
