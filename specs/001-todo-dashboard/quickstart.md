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
2. Watch the summary badges (Total/Active/Completed) while toggling items�??counts should update immediately and stay in sync after refresh.
3. Switch filters (All/Active/Completed) and confirm both the list *and* badge counts reflect the selection.
4. Seed >120 todos (use quick-add helper) and scroll; `<Virtualize>` should activate (visible in devtools) and keep scrolling responsive.
5. Confirm modals trap focus, buttons have keyboard shortcuts, and color contrast passes WCAG AA.
6. Inspect browser console for `ILogger` output when forcing storage failures (devtools localStorage quota errors) and trigger the ErrorBoundary by throwing from devtools to verify recovery messaging.

## Resetting State
Clear `todoItems.v1` and `todoFilter.v1` from browser devtools or run:
```js
localStorage.removeItem('todoItems.v1');
localStorage.removeItem('todoFilter.v1');
```
Reload the app to see the empty state prompt again.
