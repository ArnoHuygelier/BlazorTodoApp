# Implementation Plan: Basic Todo Dashboard

**Branch**: `001-todo-dashboard` | **Date**: 2025-02-14 | **Spec**: `/specs/001-todo-dashboard/spec.md`
**Input**: Feature specification from `/specs/001-todo-dashboard/spec.md`

## Summary

Deliver a single-page Blazor WebAssembly dashboard (no server rendering) that lets the solo user add, edit (modal dialog), delete, and filter todos while persisting data plus the selected filter entirely in `window.localStorage`. Focused components (dashboard shell, list, row, filters, empty state) hydrate from a `TodoStateService`, and derived counts sync instantly for up to ~200 items with accessible interactions and offline resilience.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (Blazor WebAssembly)  
**Primary Dependencies**: ASP.NET Core Blazor WebAssembly, `Microsoft.Extensions.Logging`, `System.Text.Json`, xUnit + bUnit  
**Storage**: Browser `localStorage` (JSON payloads) via an injected JS-interop repository; no server or database  
**Testing**: `dotnet test` executing xUnit + bUnit suites plus JS interop helpers  
**Target Platform**: Modern desktop/mobile browsers executing WASM over HTTPS (no server-side fallback)  
**Project Type**: Single-project WebAssembly SPA (`BlazorTodoApp`)  
**Performance Goals**: Filter/list updates < 1s for ≤200 todos; modal open/save interactions < 150ms; persistence writes < 50ms per operation  
**Constraints**: Offline-capable, no authentication, persist filter selection, components <200 LOC with scoped CSS, JS interop isolated in services  
**Scale/Scope**: Single-user workload, ≈1 dashboard page, up to 200 todos + 4 supporting components/services in scope

## Constitution Check

- **P1 Modular Blazor Composition**: Implement `TodoDashboard`, `TodoList`, `TodoItemRow`, `TodoFilters`, and `EmptyState` components with scoped CSS to keep each under 200 lines and reusable.  
- **P2 Clean Dependency Boundaries**: UI components depend on a `TodoStateService`, which wraps an `ITodoRepository` backed by `LocalStorageTodoRepository`; only the repository touches JS interop, preserving Pages → Components → Services layering.  
- **P3 Specification-Backed Delivery**: Approved `/specs/001-todo-dashboard/spec.md` anchors this plan; `/speckit.plan` (this file) and upcoming `/speckit.tasks` keep documentation synchronized before implementation.  
- **P4 .NET Quality & Testing Discipline**: Plan commits to xUnit/bUnit coverage for CRUD/filter flows and repository serialization plus enforcing `dotnet format`/`dotnet test` gates to satisfy analyzers and coverage expectations.  
- **P5 Operational Observability & Accessibility**: Services log load/save failures via `ILogger<T>`; the dashboard is wrapped in `ErrorBoundary`, modals are focus-trapped with ARIA labels, filters support keyboard navigation, and colors meet WCAG AA.

Gate Status: **PASS** (no violations or unresolved clarifications).

## Project Structure

### Documentation (this feature)

```text
specs/001-todo-dashboard/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md          # produced later by /speckit.tasks
```

### Source Code (repository root)

```text
BlazorTodoApp/
├── Pages/
│   ├── TodoDashboard.razor
│   └── TodoDashboard.razor.cs
├── Components/
│   ├── TodoList.razor
│   ├── TodoItemRow.razor
│   ├── TodoFilters.razor
│   └── EmptyState.razor
├── Services/
│   ├── ITodoRepository.cs
│   ├── LocalStorageTodoRepository.cs
│   └── TodoStateService.cs
├── Models/
│   ├── TodoItem.cs
│   └── TodoFilter.cs
├── wwwroot/css/
│   └── todo-dashboard.css
├── Program.cs
└── BlazorTodoApp.csproj

BlazorTodoApp.Tests/
├── Components/TodoDashboardTests.cs
├── Components/TodoListTests.cs
└── Services/LocalStorageTodoRepositoryTests.cs
```

**Structure Decision**: Remain within the existing single Blazor WebAssembly project, adding the dashboard page, components, services, and models plus mirrored test files so UI and service responsibilities stay isolated in line with Constitution P1/P2.

## Complexity Tracking

Not required; no constitution violations or exceptional complexities identified.
