# Research Notes: Authentication and Identity

## Decision 1: Credential resolution order
- **Decision**: Resolve credentials in order: managed identity, workload identity,
  then local developer credentials.
- **Rationale**: Prioritizes production-safe identities and matches Azure best
  practices.
- **Alternatives considered**: Environment-specific overrides; rejected for
  added complexity and non-determinism.

## Decision 2: Failure and retry behavior
- **Decision**: Use bounded retry attempts for transient token acquisition
  failures and fail fast on non-retryable errors.
- **Rationale**: Avoids infinite retries while improving resilience.
- **Alternatives considered**: No retries or unbounded retries; rejected for
  either fragility or risk of long hangs.

## Decision 3: Security boundaries and RBAC
- **Decision**: Require least-privilege RBAC roles for Key Vault secrets list/get
  and App Configuration write operations.
- **Rationale**: Minimizes blast radius while enabling required access.
- **Alternatives considered**: Broad roles (Owner/Contributor); rejected for
  excessive privilege.

## Decision 4: Secret handling
- **Decision**: Reject any configuration that embeds static secrets; only use
  platform-provided identities.
- **Rationale**: Eliminates hard-coded secrets and aligns with constitution.
- **Alternatives considered**: Allowing client secrets in local config; rejected
  for security risks.
