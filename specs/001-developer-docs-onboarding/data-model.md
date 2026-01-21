# Data Model: Developer Documentation & Onboarding

## Entities

### DocumentationSection

- **Fields**: title, purpose, audience, location, format, requiredInputs, securityNotes
- **Validation**: purpose and audience required; location must be README.md or docs/ path.

### PrerequisitesChecklist

- **Fields**: resources, roles, identityOptions
- **Validation**: roles list must include minimum RBAC roles for Key Vault and App Configuration.

### CliReference

- **Fields**: command, requiredArgs, optionalArgs, defaults, examples
- **Validation**: required arguments and defaults documented for each command.

### TroubleshootingEntry

- **Fields**: symptom, cause, resolution, exampleError
- **Validation**: exampleError must be non-secret.

## Relationships

- DocumentationSection links to PrerequisitesChecklist and CliReference.
- TroubleshootingEntry references relevant documentation section.
