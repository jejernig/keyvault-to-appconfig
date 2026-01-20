# Feature List for /specify

This document is intentionally **separate from the PRD** and is designed to be consumed directly by **/specify** workflows. Each section can be treated as an **Epic**, with the bullets underneath becoming individual **Features**.

---

## Epic A — CLI & Execution Control

* Parse and validate CLI arguments
* Enforce required arguments (Key Vault URI, App Config endpoint)
* Support optional flags (environment, dry-run, diff, parallelism)
* Validate incompatible flag combinations
* Provide deterministic exit codes for CI/CD usage
* Support non-interactive execution (no prompts by default)

---

## Epic B — Authentication & Identity

* Integrate `DefaultAzureCredential`
* Support Managed Identity authentication
* Support Workload Identity (OIDC) authentication
* Support local developer authentication (Azure CLI / VS)
* Fail fast on authentication or authorization errors

---

## Epic C — Key Vault Enumeration

* Enumerate secret names from Key Vault
* Filter secrets by prefix include list
* Filter secrets by prefix exclude list
* Filter secrets by regex include/exclude
* Filter secrets by tags (e.g., `export=true`)
* Exclude disabled or expired secrets
* Select secret version strategy (latest, pinned)

---

## Epic D — Mapping & Normalization Engine

* Load mapping configuration from YAML or JSON
* Validate mapping configuration schema
* Support direct secret-name-to-key mappings
* Support regex-based mappings with capture groups
* Apply string transforms (trim, replace, casing)
* Apply default key prefixes
* Resolve environment to App Configuration label
* Apply ContentType mappings
* Enforce denylist rules
* Detect mapping collisions

---

## Epic E — Planning & Diff Engine

* Build desired-state plan from mapped secrets
* Retrieve existing App Configuration settings
* Detect create vs update vs unchanged states
* Generate before/after diff output
* Detect and report key+label collisions
* Support managed-keys-only enforcement via tags

---

## Epic F — Azure App Configuration Writer

* Write new configuration settings
* Update existing configuration settings idempotently
* Apply labels correctly per environment
* Apply metadata and management tags
* Support configurable parallel execution
* Handle throttling and transient failures

---

## Epic G — Secret Handling Modes

* Implement Key Vault reference mode (default)
* Generate Key Vault reference payloads
* Resolve correct secret URIs (versioned/unversioned)
* Implement copy-value mode
* Require explicit allow-secret-values override
* Guarantee secret values are never logged

---

## Epic H — Observability & Reporting

* Emit human-readable console summaries
* Support structured JSON logging
* Generate run identifiers for correlation
* Produce JSON report artifacts
* Report counts for scanned, mapped, skipped, changed, failed
* List changed keys without exposing values

---

## Epic I — Error Handling & Resilience

* Isolate errors per secret
* Support fail-fast vs continue-on-error strategies
* Handle graceful shutdown and cancellation
* Map failure scenarios to deterministic exit codes

---

## Epic J — Testing & Quality Gates

* Unit tests for mapping engine
* Unit tests for diff and planning logic
* CLI argument validation tests
* Log redaction and secret leakage tests
* Optional integration test harness

---

## Epic K — Packaging & Distribution

* Package as a dotnet tool
* Support single-file binary publishing
* Provide CI/CD usage examples
* Generate versioned release artifacts

---

## Recommended /specify Order

1. CLI & Execution Control
2. Authentication & Identity
3. Key Vault Enumeration
4. Mapping & Normalization Engine
5. Planning & Diff Engine
6. App Configuration Writer
7. Secret Handling Modes
8. Observability & Reporting
9. Error Handling & Resilience
10. Testing & Quality Gates
11. Packaging & Distribution
