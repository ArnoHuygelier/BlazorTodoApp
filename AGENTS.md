# Repository Guidelines

## Project Structure & Module Organization
The Blazor WebAssembly app is defined by `BlazorTodoApp.csproj`, with hosting configured in `Program.cs` and the root component wired through `App.razor`. Feature UI lives in `Pages/` (e.g., `Home.razor`, `Counter.razor`, `Weather.razor`, `NotFound.razor`), while shared scaffolding sits in `Layout/`. Keep assets, CSS, and manifest files in `wwwroot/`; generated build output remains under `obj/`. Specifications and Codex agent prompts are stored inside `.specify/` and `.codex/`; update those only when the corresponding spec documents change.

## Build, Test, and Development Commands
- `dotnet restore`: install project dependencies before your first build or when packages change.
- `dotnet build`: compile the WebAssembly app and validate for compiler warnings.
- `dotnet watch run`: run the development server with hot reload at `https://localhost:7226` (or the port printed in the console).
- `dotnet publish -c Release`: produce a trimmed bundle for deployment inside `bin/Release/net10.0/publish`.

## Coding Style & Naming Conventions
Use four-space indentation and the default `Nullable`/`ImplicitUsings` settings already enabled in the project. Favor PascalCase for components, classes, and public methods; camelCase for local variables and private fields; kebab-case for static asset names under `wwwroot/`. Keep Razor components focused: define the UI markup first, followed by an `@code` block with strongly typed parameters and private helper methods. Run `dotnet format` locally before opening a PR to enforce Roslyn analyzers.

## Testing Guidelines
Add component and logic tests under a sibling `BlazorTodoApp.Tests` project that references `bunit` or `xUnit`. Name test files after the subject component (e.g., `TodoListTests.cs`) and methods with the pattern `MethodName_State_ExpectedResult`. Execute `dotnet test` from the solution root; target at least smoke coverage for every interactive Razor component before merging.

## Commit & Pull Request Guidelines
Existing history (`git log --oneline`) shows short, imperative messages (e.g., "Init", "Update README.md"); continue this style with a concise subject under 60 characters and optional bullet details in the body. Every PR should include: a summary of the change, linked issue references ("Fixes #123"), screenshots or GIFs for UI tweaks, and a checklist confirming `dotnet build`/`dotnet test` ran locally. Label breaking changes explicitly and request review from another agent when modifying shared layouts or tooling.
