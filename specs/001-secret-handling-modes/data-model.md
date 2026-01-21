# Data Model: Secret Handling Modes

## Entities

### SecretHandlingMode
- Purpose: Selected mode for handling secrets.
- Fields:
  - Mode (reference, copyValue)
  - GuardrailsEnabled

### SecretUriResolution
- Purpose: Validated and resolved secret URI details.
- Fields:
  - OriginalUri
  - ResolvedUri
  - Version
  - IsValid
  - FailureReason

### CopyValueGuardrail
- Purpose: Explicit confirmations required to enable copy-value.
- Fields:
  - FlagProvided
  - SecondaryConfirmation
  - AllowedKeys (list)

### RedactionPolicy
- Purpose: Guarantees redaction behavior across outputs.
- Fields:
  - Enabled
  - SensitivePatterns
  - RedactedFields

### ModeExecutionResult
- Purpose: Outcome of mode selection and guardrails.
- Fields:
  - Mode
  - GuardrailsSatisfied
  - RedactionApplied
  - Messages (non-secret)

## Relationships

- SecretHandlingMode 1..* SecretUriResolution
- SecretHandlingMode 0..1 CopyValueGuardrail
- RedactionPolicy applies to ModeExecutionResult

## State Transitions

### ModeExecutionResult
- Pending -> Succeeded
- Pending -> Blocked
- Pending -> Failed
