# Data Model: Key Vault Secret Enumeration

## Entity: SecretDescriptor

**Purpose**: Represents a secret reference without its value.

**Fields**:
- `name` (string)
- `version` (string)
- `isEnabled` (bool)
- `tags` (map<string, string>)
- `lastUpdated` (datetime)

## Entity: EnumerationFilter

**Purpose**: Encapsulates filter rules for enumeration.

**Fields**:
- `prefix` (string, optional)
- `regex` (string, optional)
- `tags` (map<string, string>, optional)
- `enabledOnly` (bool)

**Validation Rules**:
- `regex` must be valid when provided.

## Entity: VersionSelection

**Purpose**: Defines version selection strategy.

**Fields**:
- `mode` (enum): `latest` or `explicit`
- `explicitVersions` (map<string, string>, optional)

## Entity: EnumerationPage

**Purpose**: Represents a paged set of results.

**Fields**:
- `items` (list of SecretDescriptor)
- `continuationToken` (string, optional)
