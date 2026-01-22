# Quickstart: Testing & Quality Gates

## Goal

Validate required test suites and CI gating expectations for high-risk paths.

## Scenario

1. Run unit tests: `dotnet test tests/unit/KeyVaultToAppConfig.UnitTests/KeyVaultToAppConfig.UnitTests.csproj`
2. Run contract tests: `dotnet test tests/contract/KeyVaultToAppConfig.ContractTests/KeyVaultToAppConfig.ContractTests.csproj`
3. Run integration tests in a configured environment.
4. Verify logs and reports contain no secret values.

## Expected Results

- Unit tests pass within 5 minutes.
- Integration tests run successfully in release validation.
- Security/redaction tests detect any unredacted secret values.

## Validation Status

- Unit tests: Passed on January 22, 2026 (78 passed, 1 skipped; NU1900 warning due to private feed vulnerability data).
- Contract tests: Passed on January 22, 2026 (1 passed; NU1900 warning due to private feed vulnerability data).
- Integration tests: Executed on January 22, 2026 (2 skipped; tests require live Azure resources/identity; NU1900 warning due to private feed vulnerability data).
