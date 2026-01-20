# Quickstart: Key Vault Secret Enumeration

## Prerequisites

- Access to the target Key Vault with secrets list permissions.

## Common Runs

Enumerate latest versions (default):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --dry-run
```

Filter by prefix and enabled-only:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --dry-run --include-prefix app/ --enabled-only
```

Filter by regex and tags:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --dry-run --exclude-regex '^temp-' --only-tag env=prod
```

Explicit version selection:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --dry-run --version-mode explicit --version-map versions.json
```

Limit page size and resume from a continuation token:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --dry-run --page-size 100 --continuation-token <token>
```

## Throttling Behavior

- Transient throttling errors are retried with bounded backoff.
- Enumeration fails fast on non-retryable errors.

## Output Ordering

- Results are ordered by secret name and version for determinism.

## Validation Results (TODO)

- SC-001: TODO(run validation) - deterministic ordering across three runs.
- SC-002: TODO(run validation) - filter accuracy with zero false positives.
- SC-003: TODO(run validation) - latest-only returns one version per secret.
- SC-004: TODO(run validation) - 1,000+ secrets without timeouts in 95% runs.
