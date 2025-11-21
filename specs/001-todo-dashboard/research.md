# Research Log - Basic Todo Dashboard

## Decision: Local storage persistence strategy
- **Decision**: Implement `LocalStorageTodoRepository` that uses `IJSRuntime` to call `window.localStorage` and persist todos plus filter state as JSON blobs (`todoItems.v1`, `todoFilter.v1`).
- **Rationale**: Keeps the app fully client-side per requirement, avoids introducing a backend, and provides predictable read/write semantics with minimal dependencies.
- **Alternatives considered**:
  - `Blazored.LocalStorage` package – solid wrapper but unnecessary dependency overhead for a single repository.
  - IndexedDB via `IJSRuntime` – offers larger quota but adds schema complexity with no >5MB requirement right now.

## Decision: Modal-based editing UX
- **Decision**: Use a reusable `TodoEditModal` component invoked from the dashboard; edit forms live in the modal with Save/Cancel and validation messaging.
- **Rationale**: Matches the clarified modal requirement, keeps the dashboard uncluttered, and allows focus trapping + keyboard shortcuts for accessibility.
- **Alternatives considered**:
  - Inline editing in the list – violates clarified requirement and complicates keyboard navigation.
  - Side panel drawer – overkill for a single-user dashboard and harder to keep under 200 LOC.

## Decision: Scaling list rendering past 100 items
- **Decision**: Leverage Blazor's built-in `<Virtualize>` component within `TodoList` when total items > 75 to keep DOM node count low, falling back to simple loop otherwise.
- **Rationale**: Satisfies edge-case requirement to handle >100 todos without lag while keeping implementation simple and dependency-free.
- **Alternatives considered**:
  - Manual pagination UI – provides control but adds extra UX the spec did not request.
  - Third-party grid/virtualization libraries – heavier payload and unnecessary for ≤200 items.
