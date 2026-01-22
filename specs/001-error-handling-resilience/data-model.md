# Data Model: Error Handling & Resilience

## Run

Represents a single execution of the tool.

**Fields**:
- RunId
- CorrelationId
- StartedAt
- EndedAt
- Outcome (Success, RecoverableFailures, FatalFailure, Canceled)
- ExitCode
- TotalSecrets
- SuccessfulSecrets
- RecoverableFailures
- UnprocessedSecrets
- FatalErrorId (optional)
- Cancellation (optional)

## SecretOperation

Represents the result of processing a single secret.

**Fields**:
- SecretKey (redacted or normalized identifier)
- Status (Success, RecoverableFailure, Unprocessed)
- ErrorId (optional)
- StartedAt
- EndedAt

## ErrorRecord

Represents a structured error without secret values.

**Fields**:
- ErrorId
- Classification (Fatal, Recoverable)
- Scope (Run, Secret)
- Stage (e.g., Startup, Read, Write, Validation)
- Summary (redacted)
- OccurredAt

## CancellationEvent

Captures a cancellation or shutdown request.

**Fields**:
- RequestedAt
- AcknowledgedAt
- Reason (UserRequested, SystemShutdown)

## Relationships

- Run has many SecretOperations.
- Run has zero or more ErrorRecords.
- SecretOperation references ErrorRecord when failed.
- Run has zero or one CancellationEvent.

## State Transitions

- Run: InProgress -> Success | RecoverableFailures | FatalFailure | Canceled
- SecretOperation: Pending -> Success | RecoverableFailure | Unprocessed
