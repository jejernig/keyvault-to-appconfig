# Data Model: Authentication and Identity

## Entity: CredentialSource

**Purpose**: Represents a supported identity type and its availability.

**Fields**:
- `type` (enum): `managed-identity`, `workload-identity`, `local-dev`.
- `isAvailable` (bool): Whether the source is currently usable.
- `details` (string, optional): Safe diagnostic summary (no secrets).

## Entity: CredentialResolutionPolicy

**Purpose**: Defines the deterministic order used to select a credential source.

**Fields**:
- `orderedSources` (list of CredentialSource types)

**Validation Rules**:
- Must include all supported identity types in deterministic order.

## Entity: AuthResult

**Purpose**: Captures the outcome of authentication and token acquisition.

**Fields**:
- `isSuccess` (bool)
- `selectedSource` (CredentialSource type)
- `errorCategory` (enum): `none`, `transient`, `fatal`, `rbac`.
- `errorMessage` (string): User-safe diagnostics.
- `retryCount` (int)

**State Transitions**:
- `pending` -> `success` when token acquired.
- `pending` -> `retrying` on transient failure.
- `retrying` -> `success` or `fatal` when retries are exhausted.
