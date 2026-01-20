# Quickstart: Mapping & Normalization Engine

## Goal
Create a mapping specification, validate it, and run a deterministic normalization that reports collisions.

## Step 1: Create a mapping spec (YAML)

```yaml
name: StandardizeSecrets
version: v1
collisionPolicy: error
rules:
  - ruleId: db-conn-direct
    strategyType: direct
    sourceSelector: DbConnectionString
    targetKey: database.connection
    priority: 100
    transforms:
      - transformType: trim
      - transformType: lower
  - ruleId: service-regex
    strategyType: regex
    sourceSelector: ^Service_(.+)$
    targetKey: service.$1
    priority: 50
    transforms:
      - transformType: regex-replace
        parameters:
          pattern: _
          replacement: .
```

Store the spec for reuse:

```bash
dotnet run --project src/cli/KeyVaultToAppConfig.Cli -- mapping-spec --spec-path ./mapping-spec.yaml --store-path ./.mapping-specs
```

## Step 2: Validate the spec

- Submit the spec to the validation command and confirm no unknown fields are present.

```bash
dotnet run --project src/cli/KeyVaultToAppConfig.Cli -- mapping-validate --spec-path ./mapping-spec.yaml
```

## Step 3: Run a mapping

- Provide a source set with keys to normalize.
- Execute a mapping run using the validated spec.
- Review the normalized output and any collision report.

```bash
dotnet run --project src/cli/KeyVaultToAppConfig.Cli -- mapping-run --spec-path ./mapping-spec.yaml --source-path ./source-keys.txt
```

## Expected Outcomes

- Output keys are deterministic and stable across repeat runs.
- Collisions are reported based on the declared policy.
- Transform order is honored exactly as defined in the spec.
