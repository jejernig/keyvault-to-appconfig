# Contracts: Testing & Quality Gates

This feature does not define external APIs. The contract is the required CI gates and test suites.

## CI Gate Contract

- Merge gate: Unit tests required, no failures.
- Release gate: Unit + integration + security/redaction suites required.
- Waiver: Allowed only with documented approval and scope.

## Required Test Suites

- Unit: mapping, diff, redaction, error handling, exit codes, deterministic ordering.
- Integration: Key Vault reference resolution, App Configuration write path.
- Security: redaction coverage, copy-value guardrails.
