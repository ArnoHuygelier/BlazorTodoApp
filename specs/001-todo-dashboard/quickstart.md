# Quickstart - Basic Todo Dashboard

## Prerequisites
- .NET SDK 10.0 preview (matching `BlazorTodoApp.csproj` target)
- Trust the ASP.NET Core HTTPS dev certificate (`dotnet dev-certs https --trust`)
- Modern Chromium/Firefox/Edge browser for WASM testing

## One-Time Setup
1. `dotnet restore` – restores Blazor + test dependencies.
2. (Optional) `dotnet workload update wasm-tools` if WASM tooling is missing.

## Run the Dashboard
```bash
dotnet watch run --project BlazorTodoApp/BlazorTodoApp.csproj
```
- Open the printed `https://localhost:7226` URL.
- Verify localStorage keys `todoItems.v1` and `todoFilter.v1` are created after adding/editing items.

## Tests & Quality Gates
```bash
dotnet format
dotnet test
```
- Component tests (`BlazorTodoApp.Tests/Components/*`) cover CRUD/filter flows.
- Service tests (`BlazorTodoApp.Tests/Services/*`) validate serialization + localStorage interactions via JSInterop mocks.

## Manual Verification Checklist
1. Create, edit (modal), toggle, and delete todos; ensure they persist after reload.
2. Switch filters (All/Active/Completed) and confirm counts update instantly.
3. Seed >120 todos (use quick-add helper) and scroll; `<Virtualize>` keeps the UI responsive.
4. Confirm modals trap focus, buttons have keyboard shortcuts, and color contrast passes WCAG AA.
5. Inspect browser console for `ILogger` output when forcing storage failures (devtools localStorage quota errors).

## Resetting State
Clear `todoItems.v1` and `todoFilter.v1` from browser devtools or run:
```js
localStorage.removeItem('todoItems.v1');
localStorage.removeItem('todoFilter.v1');
```
Reload the app to see the empty state prompt again.
