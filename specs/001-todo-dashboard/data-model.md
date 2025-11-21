# Data Model - Basic Todo Dashboard

## Entities

### TodoItem
| Field | Type | Constraints / Notes |
|-------|------|---------------------|
| `Id` | `Guid` | Generated client-side; unique per todo. |
| `Title` | `string` | Required, trimmed, 1-120 chars, unique when case- and whitespace-normalized. |
| `Note` | `string?` | Optional, trimmed, ≤500 chars. |
| `IsCompleted` | `bool` | `true` when marked done; drives filters and summary counts. |
| `CreatedAt` | `DateTimeOffset` | UTC timestamp when added; immutable. |
| `UpdatedAt` | `DateTimeOffset` | Updated on every edit/toggle/delete for auditing. |
| `DueDay` | `DateOnly?` | Optional calendar day (no time); ignored for filtering today. |

**Validation Rules**
- Reject save if `Title` is null/empty after trimming or duplicates an existing title ignoring whitespace and case.
- `Note` stored as `null` when trimmed text is empty.
- `DueDay` must be ≥ current day (cannot schedule in the past) though it is optional.

### TodoFilter
| Field | Type | Constraints / Notes |
|-------|------|---------------------|
| `Selection` | `TodoFilterOption` | Enum: `All`, `Active`, `Completed`; defaults to `All`. |

Persisted separately to restore user context across refreshes.

### DashboardSummary (Derived)
| Field | Type | Source |
|-------|------|--------|
| `Total` | `int` | Count of all todos. |
| `Active` | `int` | Count where `IsCompleted == false`. |
| `Completed` | `int` | Count where `IsCompleted == true`. |
| `Filter` | `TodoFilterOption` | Mirrors `TodoFilter.Selection`. |

Computed every time the state mutates; not persisted directly.

## Relationships
- `TodoDashboard` owns a collection of `TodoItem` plus a `TodoFilter`.
- `TodoStateService` exposes `IReadOnlyList<TodoItem>` filtered views but stores the canonical list.
- `DashboardSummary` derives only from the canonical list and current filter.

## State Transitions
1. **New ➝ Active**: On creation, `IsCompleted = false`, `CreatedAt = UpdatedAt = now`.
2. **Active ➝ Completed**: Toggle sets `IsCompleted = true`, updates `UpdatedAt`.
3. **Completed ➝ Active**: Toggle resets `IsCompleted = false`, updates `UpdatedAt`.
4. **Any ➝ Deleted**: Remove from list and persist deletion (no soft-delete requirement).

Any mutation triggers persistence via `ITodoRepository.SaveAsync` and summary recalculation.
