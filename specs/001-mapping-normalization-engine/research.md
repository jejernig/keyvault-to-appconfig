# Research Notes: Mapping & Normalization Engine

## Decision 1: YAML and JSON normalization
- Decision: Accept YAML and JSON inputs by normalizing YAML into a canonical JSON model before validation and execution.
- Rationale: A single canonical model reduces schema drift and ensures consistent validation and deterministic behavior.
- Alternatives considered: Separate validators for YAML and JSON; YAML-only support.

## Decision 2: Schema validation approach
- Decision: Validate specs against a strict JSON schema derived from the canonical model and reject unknown fields.
- Rationale: Ensures explicit, predictable inputs and prevents silent misconfigurations.
- Alternatives considered: Permissive validation with warnings; custom ad-hoc validation rules only.

## Decision 3: Rule evaluation and tie-breaking
- Decision: Apply highest-priority rule first; for ties, use the earliest rule in spec order.
- Rationale: Keeps outcomes deterministic and easy to reason about.
- Alternatives considered: Last-wins tie-breaking; non-deterministic evaluation based on hash ordering.

## Decision 4: Transform pipeline behavior
- Decision: Apply transforms in the declared order with locale-stable casing and deterministic regex behavior.
- Rationale: Preserves author intent and guarantees reproducibility.
- Alternatives considered: Fixed transform order; locale-sensitive casing.

## Decision 5: Collision handling default
- Decision: Default collision policy is error, with explicit options keep-first, keep-last, or report-only.
- Rationale: Fails safe and makes conflicts visible.
- Alternatives considered: Keep-last as default; ignore collisions.

## Decision 6: Output ordering
- Decision: Output ordering follows rule order and stable source key ordering defined by the spec input list.
- Rationale: Stable ordering is required for deterministic runs and diffability.
- Alternatives considered: Sort alphabetically; preserve input source discovery order only.
