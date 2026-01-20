# Prompts by Epic

This document contains **ready-to-run prompts** for the `/prompts:speckit.specify` workflow. Each prompt is scoped to **one Epic** and is designed to produce **individual, testable feature specifications** aligned with the PRD and feature list.

Use one prompt at a time.

---

## Epic A — CLI & Execution Control

**/prompts:speckit.specify**

Act as a Principal Software Engineer.

Using the PRD and Feature List, specify all features required for **CLI & Execution Control**. For each feature:

* Define purpose and scope
* Define required inputs and validation rules
* Define CLI flags and defaults
* Define failure cases and exit codes
* Define acceptance criteria

Ensure all features are CI/CD safe, non-interactive by default, and deterministic.

---

## Epic B — Authentication & Identity

**/prompts:speckit.specify**

Act as a Principal Cloud Engineer.

Specify all features required for **Authentication & Identity** using Azure best practices. For each feature:

* Define supported identity types (Managed Identity, Workload Identity, local dev)
* Define credential resolution order
* Define failure and retry behavior
* Define security boundaries and RBAC assumptions
* Define acceptance criteria

Avoid any hard-coded secrets or credentials.

---

## Epic C — Key Vault Enumeration

**/prompts:speckit.specify**

Act as a Senior Platform Engineer.

Specify features for **Key Vault secret enumeration**. For each feature:

* Define how secrets are discovered and paged
* Define filtering rules (prefix, regex, tags, enabled state)
* Define version selection strategy
* Define performance and throttling considerations
* Define acceptance criteria

Ensure enumeration is safe, scalable, and deterministic.

---

## Epic D — Mapping & Normalization Engine

**/prompts:speckit.specify**

Act as a Domain-Driven Design expert.

Specify features for the **Mapping & Normalization Engine**. For each feature:

* Define mapping inputs (YAML/JSON)
* Define schema validation rules
* Define supported mapping strategies (direct, regex)
* Define transform operations
* Define collision detection behavior
* Define acceptance criteria

Ensure mappings are deterministic and reproducible.

---

## Epic E — Planning & Diff Engine

**/prompts:speckit.specify**

Act as a Staff Software Engineer.

Specify features for the **Planning & Diff Engine**. For each feature:

* Define how desired state is constructed
* Define how existing App Configuration state is read
* Define diff rules (create/update/unchanged)
* Define collision and conflict handling
* Define dry-run behavior
* Define acceptance criteria

Ensure no writes occur during planning or dry-run modes.

---

## Epic F — Azure App Configuration Writer

**/prompts:speckit.specify**

Act as a Cloud Platform Engineer.

Specify features for **writing to Azure App Configuration**. For each feature:

* Define idempotent write behavior
* Define label handling rules
* Define metadata/tag application
* Define concurrency and retry strategy
* Define rollback or partial-failure behavior
* Define acceptance criteria

Ensure safe updates and minimal churn.

---

## Epic G — Secret Handling Modes

**/prompts:speckit.specify**

Act as a Security-Focused Engineer.

Specify features for **Secret Handling Modes**. For each feature:

* Define Key Vault reference mode behavior
* Define secret URI resolution rules
* Define copy-value mode behavior
* Define explicit guardrails for secret value handling
* Define logging redaction guarantees
* Define acceptance criteria

Default to least-risk behavior.

---

## Epic H — Observability & Reporting

**/prompts:speckit.specify**

Act as an Observability Engineer.

Specify features for **Observability & Reporting**. For each feature:

* Define logging structure and verbosity levels
* Define run correlation identifiers
* Define console summary output
* Define JSON report schema
* Define acceptance criteria

Ensure reports are audit-safe and machine-readable.

---

## Epic I — Error Handling & Resilience

**/prompts:speckit.specify**

Act as a Reliability Engineer.

Specify features for **Error Handling & Resilience**. For each feature:

* Define error classification (fatal vs recoverable)
* Define per-secret isolation behavior
* Define cancellation and shutdown handling
* Define exit code mapping
* Define acceptance criteria

Favor correctness and predictability over silent success.

---

## Epic J — Testing & Quality Gates

**/prompts:speckit.specify**

Act as a Test Architect.

Specify features for **Testing & Quality Gates**. For each feature:

* Define unit test scope
* Define integration test strategy
* Define security and log-redaction tests
* Define CI gating requirements
* Define acceptance criteria

Ensure coverage of high-risk paths.

---

## Epic K — Packaging & Distribution

**/prompts:speckit.specify**

Act as a DevOps Engineer.

Specify features for **Packaging & Distribution**. For each feature:

* Define supported packaging formats
* Define versioning strategy
* Define CI/CD integration patterns
* Define release artifact requirements
* Define acceptance criteria

Ensure repeatable, deterministic builds.

---

## Usage Guidance

* Run `/prompts:speckit.specify` once per Epic
* Do not combine Epics in a single run
* Lock feature acceptance criteria before proceeding to `/plan`
