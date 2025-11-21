# Local Storage Contract - Basic Todo Dashboard

## Storage Keys
| Key | Payload | Notes |
|-----|---------|-------|
| `todoItems.v1` | JSON array of `TodoItemDto` | Canonical todo collection persisted on every mutation. |
| `todoFilter.v1` | JSON object `{ "selection": "All\|Active\|Completed" }` | Restores the user's selected filter on reload. |

Version suffix (`.v1`) reserves room for non-breaking schema updates; bump the suffix when changing field names or semantics.

## `TodoItemDto` Schema
```json
{
  "id": "7f9725cb-63ce-4cf6-9372-5a2ffb541082",
  "title": "Buy cat food",
  "note": "Check grain-free brand",
  "isCompleted": false,
  "createdAt": "2025-02-14T10:32:25.040Z",
  "updatedAt": "2025-02-14T10:32:25.040Z",
  "dueDay": "2025-02-20"
}
```
- `dueDay` omitted when `null`.
- `title` trimmed before serialization; `note` dropped when empty.

## Operations
| Operation | Method | Description | Failure Handling |
|-----------|--------|-------------|------------------|
| `LoadAsync()` | JS interop `localStorage.getItem` | Returns deserialized array or `Array.Empty<TodoItem>()` when missing. | Log warning + return empty list if JSON is invalid. |
| `SaveAsync(items)` | JS interop `localStorage.setItem` | Serializes todos and writes them atomically. | Catch `JSException`, log error, surface toast to user. |
| `LoadFilterAsync()` | `localStorage.getItem` | Returns stored filter selection or defaults to `All`. | On invalid value, reset to `All` and overwrite storage. |
| `SaveFilterAsync(selection)` | `localStorage.setItem` | Persists the enum string value. | Same as above. |

## Concurrency & Versioning
- Single-user assumption eliminates write conflicts; nevertheless, repository writes the full array each time to keep state deterministic.
- If future schemas add fields, migrate in-memory data before writing (e.g., map missing `dueDay` to `null`).
- If a `.v1` key is missing while `.v0` exists, migration logic should attempt to parse `.v0`, transform, then delete legacy data.
