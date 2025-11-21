# Tasks: Basic Todo Dashboard

**Input**: Design documents from `/specs/001-todo-dashboard/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Each user story includes explicit bUnit/xUnit coverage tasks as mandated by Constitution P4.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare shared assets and styling hooks required across the dashboard.

- [X] T001 Create `wwwroot/css/todo-dashboard.css` with root tokens for list states, modal overlays, and summary badges in `BlazorTodoApp/wwwroot/css/todo-dashboard.css`
- [X] T002 Import the new stylesheet via `@import "todo-dashboard.css";` inside `BlazorTodoApp/wwwroot/css/app.css`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, storage plumbing, and services required before any UI story can start.

- [ ] T003 Add `TodoItem` entity with validation constraints to `BlazorTodoApp/Models/TodoItem.cs`
- [ ] T004 Add `TodoFilterOption` enum plus `TodoFilter` record to `BlazorTodoApp/Models/TodoFilter.cs`
- [ ] T005 Define `ITodoRepository` abstraction exposing CRUD + filter persistence in `BlazorTodoApp/Services/ITodoRepository.cs`
- [ ] T006 [P] Implement `LocalStorageTodoRepository` using `IJSRuntime` and JSON serialization per contract in `BlazorTodoApp/Services/LocalStorageTodoRepository.cs`
- [ ] T007 [P] Add `todoStorage.js` module that wraps `localStorage` get/set/remove operations in `BlazorTodoApp/wwwroot/js/todoStorage.js` and ensure it is referenced from `wwwroot/index.html`
- [ ] T008 Create `TodoStateService` that loads items/filter, raises change notifications, and enforces invariants in `BlazorTodoApp/Services/TodoStateService.cs`
- [ ] T009 Register `ITodoRepository` + `TodoStateService` in DI inside `BlazorTodoApp/Program.cs`
- [ ] T010 [P] Add xUnit tests covering repository serialization/error handling in `BlazorTodoApp.Tests/Services/LocalStorageTodoRepositoryTests.cs`

**Checkpoint**: Storage + service layer ready for UI work.

---

## Phase 3: User Story 1 - Manage Personal Todos (Priority: P1) — MVP

**Goal**: Solo user can add, edit (modal), toggle, and delete todos directly on the dashboard with persistence.

**Independent Test**: Start from empty state, create a todo, edit title/note, toggle done/undone, delete it, reload page, confirm persistence.

### Tests for User Story 1

- [ ] T011 [P] [US1] Extend `BlazorTodoApp.Tests/Services/TodoStateServiceTests.cs` to cover add/edit/delete/toggle workflows
- [ ] T012 [P] [US1] Add bUnit tests simulating CRUD flows via forms and modal in `BlazorTodoApp.Tests/Components/TodoDashboardTests.cs`

### Implementation for User Story 1

- [ ] T013 [US1] Implement routed dashboard page markup (`@page "/todos"`) with empty state host in `BlazorTodoApp/Pages/TodoDashboard.razor`
- [ ] T014 [US1] Add partial class logic that binds to `TodoStateService`, handles validation, and launches modal in `BlazorTodoApp/Pages/TodoDashboard.razor.cs`
- [ ] T015 [P] [US1] Create `TodoList` component that accepts todos and emits edit/toggle/delete events in `BlazorTodoApp/Components/TodoList.razor`
- [ ] T016 [P] [US1] Create `TodoItemRow` component with buttons, keyboard shortcuts, and visual states in `BlazorTodoApp/Components/TodoItemRow.razor`
- [ ] T017 [P] [US1] Build `TodoEditModal` component with form fields, ARIA attributes, focus trap, and Save/Cancel in `BlazorTodoApp/Components/TodoEditModal.razor`
- [ ] T018 [US1] Add reusable empty-state component with quick-add CTA in `BlazorTodoApp/Components/EmptyState.razor`
- [ ] T019 [US1] Update `Shared/NavMenu.razor` (and default route if needed) to link to `/todos` so the dashboard is accessible from the shell

**Checkpoint**: CRUD experiences fully functional and independently testable (MVP complete).

---

## Phase 4: User Story 2 - Filter by Completion State (Priority: P2)

**Goal**: User can switch between All, Active, and Completed views; selection persists through reload.

**Independent Test**: Seed sample todos, switch filters, ensure list updates instantly without mutating stored data, reload page and confirm last filter selection applied.

### Tests for User Story 2

- [ ] T020 [P] [US2] Add bUnit tests covering filter button behavior, ARIA states, and persistence in `BlazorTodoApp.Tests/Components/TodoFiltersTests.cs`
- [ ] T021 [P] [US2] Extend `BlazorTodoApp.Tests/Services/TodoStateServiceTests.cs` with cases verifying filter selection storage and restored state

### Implementation for User Story 2

- [ ] T022 [US2] Implement `TodoFilters` component with All/Active/Completed controls, badge counts slots, and keyboard shortcuts in `BlazorTodoApp/Components/TodoFilters.razor`
- [ ] T023 [US2] Update `TodoStateService` to expose `CurrentFilter`, filtered views, and persistence hooks in `BlazorTodoApp/Services/TodoStateService.cs`
- [ ] T024 [US2] Persist filter selection within `LocalStorageTodoRepository` using dedicated key per contract in `BlazorTodoApp/Services/LocalStorageTodoRepository.cs`
- [ ] T025 [US2] Integrate filter component into dashboard layout, wiring callbacks and state updates in `BlazorTodoApp/Pages/TodoDashboard.razor`

**Checkpoint**: Filtering UX independently testable with restored context after refresh.

---

## Phase 5: User Story 3 - Review Todo Dashboard (Priority: P3)

**Goal**: Dashboard surfaces summary counts (total/active/completed), responsive list performance (>100 items), and motivating visuals.

**Independent Test**: Populate >100 todos, load dashboard, verify counts are accurate, virtualization keeps scrolling responsive, and summary badges update immediately on toggles.

### Tests for User Story 3

- [ ] T026 [P] [US3] Extend `TodoDashboard` bUnit tests to assert summary badges update with adds/toggles in `BlazorTodoApp.Tests/Components/TodoDashboardTests.cs`
- [ ] T027 [P] [US3] Add `TodoList` rendering test to verify `<Virtualize>` activates beyond 75 items using `BlazorTodoApp.Tests/Components/TodoListTests.cs`

### Implementation for User Story 3

- [ ] T028 [US3] Render summary badges (total/active/completed) in the dashboard header inside `BlazorTodoApp/Pages/TodoDashboard.razor`
- [ ] T029 [US3] Expose derived summary model and change notifications from `TodoStateService` in `BlazorTodoApp/Services/TodoStateService.cs`
- [ ] T030 [US3] Upgrade `TodoList.razor` to switch between `<Virtualize>` and simple `@foreach` based on item count threshold
- [ ] T031 [US3] Enhance `wwwroot/css/todo-dashboard.css` with responsive layout, badge styling, and state colors that meet WCAG AA

**Checkpoint**: Dashboard provides high-level insights and remains performant with large lists.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Harden instrumentation, accessibility, and docs across stories.

- [ ] T032 [P] Add structured logging + ErrorBoundary wrapping for dashboard interactions in `BlazorTodoApp/Pages/TodoDashboard.razor` and `TodoStateService`
- [ ] T033 [P] Update `specs/001-todo-dashboard/quickstart.md` with manual verification steps for filters, counts, and virtualization
- [ ] T034 Run full `dotnet format` + `dotnet test` pipeline and document outputs in `specs/001-todo-dashboard/checklists/qa.md`

---

## Dependencies & Execution Order

1. **Phase 1 → Phase 2**: Styling assets must exist before components reference classes.
2. **Phase 2 → User Stories**: Models/services/repository must be complete before any story work begins.
3. **User Story Order**: US1 (CRUD MVP) → US2 (Filtering) → US3 (Dashboard summary). Each can proceed in parallel after Phase 2, but delivery should respect priority to ensure MVP readiness.
4. **Polish**: Starts only after desired user stories finish; can run in parallel with final verification.

---

## Parallel Execution Examples

- **During US1**: Work on `TodoList.razor` (T015) and `TodoItemRow.razor` (T016) concurrently while another dev builds the modal (T017).
- **During US2**: Execute filter component (T022) in parallel with service/filter persistence changes (T023–T024) since files are distinct.
- **During US3**: Summary badge markup (T028) and virtualization upgrade (T030) can proceed simultaneously once `TodoStateService` exposes counts.

---

## Implementation Strategy

### MVP First
1. Finish Phases 1–2 to establish styling + data foundation.
2. Complete all US1 tasks (T011–T019) and run their tests; this delivers a fully offline CRUD dashboard suitable for demo (MVP).

### Incremental Delivery
1. Deploy MVP (US1) once validated.
2. Add US2 filtering enhancements; release to enable focused work queues.
3. Layer US3 summaries + performance improvements for the motivational dashboard.
4. Conclude with Polish tasks to document verification and ensure instrumentation/accessibility compliance.

### Parallel Team Strategy
- After Phase 2, dedicate separate contributors to each user story as highlighted in the parallel examples, coordinating through the DI service contract to avoid merge conflicts.

---
