# Repository Guidelines

## Project Structure & Module Organization

- `docs/` holds the PRD, feature list, and /specify prompts that define product scope.
- `.specify/` contains the constitution, templates, and workflow scaffolding for specs and plans.
- Source code and tests are not yet present; expect future `src/` and `tests/` roots when implementation starts.

## Build, Test, and Development Commands

No build or runtime commands are defined yet. When implementation begins, add a short command list here (for example, `dotnet build`, `dotnet test`, or `dotnet run`) and keep it in sync with the tooling used.

## Coding Style & Naming Conventions

No code style is established yet. For C#/.NET, prefer:
- Indentation: 4 spaces, no tabs.
- Naming: `PascalCase` for types/methods, `camelCase` for locals/parameters.
- Files: one public type per file, matching the type name (e.g., `MappingEngine.cs`).
If you introduce formatters or linters (e.g., `dotnet format`, `.editorconfig`), document them here.

## Testing Guidelines

Testing frameworks are not specified yet. The constitution requires tests for mapping, diff logic, and redaction safeguards, plus integration tests when touching live Azure services or contracts. When tests are added, document:
- Framework(s) (e.g., xUnit).
- Naming conventions (e.g., `*Tests.cs`).
- How to run unit and integration tests.

## Commit & Pull Request Guidelines

Git history currently includes a single seed commit, so no conventions are established. Until a standard emerges, use clear, imperative messages (e.g., `docs: add specification outline`).

PRs should include:
- A concise description of scope and rationale.
- Links to related issues/specs in `docs/` or `.specify/`.
- Notes on tests run (or why not run).

## Security & Configuration Tips

This tool will handle secrets. Never log secret values. Prefer Key Vault reference mode, and require explicit flags for any copy-value workflows. Document required Azure RBAC roles and avoid hard-coded endpoints or credentials.
