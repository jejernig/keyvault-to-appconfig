# KeyVaultToAppConfig.UnitTests

## Purpose

Unit tests validate core behavior for mapping, planning, secret handling, error handling, and exit code contracts.

## Run

```text
dotnet test tests/unit/KeyVaultToAppConfig.UnitTests/KeyVaultToAppConfig.UnitTests.csproj
```

## Focus Areas

- Mapping rules and transforms (`Mapping/`)
- Diff and planning classification (`Planning/`)
- Secret handling, redaction, and copy-value guardrails (`Secrets/`)
- Error classification and exit code mapping (`ErrorHandling/`)
- Observability outputs and report structures (`Observability/`)
- Write planning and apply behavior (`Writes/`)

## Guidance

- Keep tests deterministic and isolated.
- Do not log secret values or include them in assertions.
- Prefer table-driven cases for mapping and diff behaviors.
