# Quickstart: Planning & Diff Engine

## Goal
Generate a deterministic plan and diff without writing to App Configuration.

## Step 1: Prepare desired state input

Provide a desired state definition produced by the mapping engine.

## Step 2: Generate a plan

- Run the planning command with desired state and optional existing state inputs.
- Confirm the plan output lists create, update, and unchanged items.

Example:

```text
dotnet run -- plan --desired-path desired-state.json --existing-path existing-state.json
```

## Step 3: Execute a dry-run

- Run dry-run with the same inputs as planning.
- Verify the plan output matches planning and no writes occur.

Example:

```text
dotnet run -- dry-run --desired-path desired-state.json --existing-path existing-state.json
```

## Step 4: Compute a diff

- Run diff with desired and existing state inputs.
- Review classifications and conflict reporting.

Example:

```text
dotnet run -- diff --desired-path desired-state.json --existing-path existing-state.json
```

## Expected Outcomes

- Plan output is deterministic and ordered.
- Diff classification is accurate for create, update, and unchanged.
- Dry-run performs zero writes.
