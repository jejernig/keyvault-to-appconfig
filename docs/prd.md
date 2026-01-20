# Product Requirements Document (PRD)

## Key Vault → Azure App Configuration Standardizer (Console)

---

## 1. Overview

### 1.1 Purpose

Build a **.NET (Core) C# console application** that:

1. Connects to a specified **Azure Key Vault**.
2. Enumerates secret names (and selected versions).
3. Applies a **mapping and normalization layer** to convert Key Vault secret identifiers into a **standardized Azure App Configuration key structure**.
4. Writes results into a target **Azure App Configuration** store as KeyValues or Key Vault references.
5. Produces an auditable, repeatable, and safe process for configuration standardization.

### 1.2 Goals

* Standardize configuration keys across applications and environments.
* Enforce consistent naming conventions, labels, and metadata.
* Support dry-run, diff, and idempotent apply modes.
* Follow Azure best practices: managed identity, least privilege RBAC, safe logging, retries, and strong error handling.

### 1.3 Non-Goals

* Editing or rotating secrets in Key Vault.
* Provisioning Azure resources (handled via IaC).
* Running as a long-lived service.
* Full secrets lifecycle management.

---

## 2. Stakeholders & Users

* **Platform / DevOps Engineers**: operate the tool locally or in CI/CD.
* **Application Teams**: consume standardized configuration keys.
* **Security / Compliance**: review access scopes and audit output.

---

## 3. Definitions & Concepts

### 3.1 Source: Azure Key Vault

* Secret attributes:

  * Name
  * Version (optional)
  * Value (only when explicitly enabled)
  * Tags
  * ContentType
  * Enabled / NotBefore / ExpiresOn

### 3.2 Target: Azure App Configuration

* ConfigurationSetting attributes:

  * Key
  * Value
  * Label
  * ContentType
  * Tags / metadata

### 3.3 Operation Modes

* **Key Vault Reference Mode (default, recommended)**

  * Writes App Configuration Key Vault references pointing to secret URIs.
* **Copy Value Mode (opt-in, restricted)**

  * Writes secret values directly into App Configuration.
  * Requires explicit override flag.

---

## 4. Functional Requirements

### FR1 — Secure Authentication

* Use `DefaultAzureCredential`.
* Support Managed Identity, Workload Identity (OIDC), and local developer auth.

### FR2 — Enumerate Secrets

* List secrets from Key Vault.
* Filters:

  * Prefix include/exclude
  * Regex include/exclude
  * Tag-based filters (e.g., `export=true`)
  * Enabled-only secrets
* Version selection:

  * Latest version (default)
  * Explicit version pinning

### FR3 — Mapping & Normalization

* Transform Key Vault secret names to App Configuration keys.
* Mapping sources:

  * YAML or JSON mapping file (primary)
  * Convention-based fallback
* Support:

  * Direct mappings
  * Regex-based mappings with capture groups
  * Prefix trimming and transforms
  * Default label assignment
  * ContentType mapping

### FR4 — Write to Azure App Configuration

* Idempotent upsert behavior.
* Compare existing values and metadata before updating.
* Support labels (environment-based preferred).
* Apply metadata tags:

  * source=keyvault
  * keyvaultUri=<uri>
  * managedBy=<tool>
  * managedAt=<timestamp>

### FR5 — Dry Run, Diff, and Apply

* Dry-run mode with no writes.
* Diff output (before/after summary).
* Apply mode executes changes.
* Output human-readable console summary and optional JSON report.

### FR6 — Safety & Compliance

* Never log secret values.
* Optional redaction of secret names.
* Denylist support for sensitive secrets.
* Copy-value mode gated behind explicit flag and confirmation.

### FR7 — Failure Handling

* Per-secret error handling with configurable fail-fast.
* Exit codes:

  * 0: success
  * 1: success with changes
  * 2: partial failures
  * 3: fatal error

### FR8 — Observability

* Structured logging option.
* Report includes counts, changed keys, skipped items, and failures (no values).

---

## 5. Non-Functional Requirements

### NFR1 — Security

* Least-privilege RBAC:

  * Key Vault: secrets list/get
  * App Config: configuration data write
* Support private endpoints.

### NFR2 — Reliability

* Azure SDK retry policies.
* Graceful cancellation (Ctrl+C).

### NFR3 — Performance

* Handle hundreds/thousands of secrets.
* Configurable parallelism.

### NFR4 — Maintainability

* Layered architecture:

  * CLI / Host
  * Domain (mapping rules)
  * Infrastructure (Azure clients)
  * Reporting

### NFR5 — Portability

* Cross-platform (Windows/Linux).
* Optional single-file publish.

---

## 6. CLI User Experience

### Required Arguments

* `--keyvault-uri`
* `--appconfig-endpoint`

### Optional Arguments

* `--environment`
* `--mapping-file`
* `--mode kvref|copyvalue`
* `--dry-run`
* `--diff`
* `--parallelism`
* `--include-prefix`
* `--exclude-regex`
* `--only-tag`
* `--report-json`

---

## 7. Mapping Rules

### Standard Key Format (Example)

* `<app>/<component>/<setting>`

### Labels

* Environment as label (`dev`, `test`, `prod`).
* Keys remain environment-agnostic.

### Mapping File Requirements

* Defaults section
* Rules section (direct + regex)
* Denylist section

---

## 8. Data Handling Rules

### Key Vault Reference Mode

* Write App Config KV reference JSON with secret URI.

### Copy Value Mode

* Write secret value directly.
* Never emit value to logs.

### Collision Handling

* Default: fail run with collision report.
* Optional override: last-write-wins.

---

## 9. Acceptance Criteria

1. Secrets enumerated and summarized.
2. Deterministic mapping output.
3. Dry-run performs zero writes.
4. Idempotent updates only when needed.
5. No secret values logged.
6. Managed Identity authentication verified.
7. Collision detection enforced.
8. JSON report generated when requested.

---

## 10. Testing Strategy

### Unit Tests

* Mapping engine
* Diff detection
* CLI validation

### Integration Tests

* Test subscription with seeded Key Vault and App Config.

### Security Tests

* Log scanning to ensure no secret leakage.

---

## 11. Deployment & Operations

### Distribution

* dotnet tool, single binary, or container image.

### CI/CD

* Build, test, lint, security checks.

### RBAC Documentation

* Required roles documented and validated.

---

## 12. Architecture

### Modules

* CliHost
* KeyVaultClientAdapter
* AppConfigClientAdapter
* MappingEngine
* Planner
* Executor
* Reporter

### Dependencies

* Azure.Identity
* Azure.Security.KeyVault.Secrets
* Azure.Data.AppConfiguration
* Microsoft.Extensions.Hosting
* Microsoft.Extensions.Logging

---

## 13. Risks & Mitigations

* Secret leakage → strict logging rules.
* Throttling → retry and concurrency limits.
* Overwrites → managed-key tagging.

---

## 14. Milestones

1. MVP: enumeration, mapping, dry-run, KV references.
2. Hardening: diff, reports, collision handling.
3. Enterprise polish: packaging, integration tests.

---

## 15. Feature Lists for /specify

### Feature Group A — CLI & Execution Control

* CLI argument parsing and validation
* Required argument enforcement (Key Vault URI, App C
