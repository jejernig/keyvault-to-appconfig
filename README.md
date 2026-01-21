# Key Vault to App Configuration Standardizer

Minimal onboarding is in `docs/onboarding.md`.

## Quick Start

- Read `docs/onboarding.md` for local build/run, required inputs, and safe modes.
- Use `docs/cli-reference.md` for required CLI arguments and defaults.
- Use `docs/prerequisites.md` for Azure resources and RBAC roles.
- Use `docs/identity.md` for identity options and credential guardrails.
- Use `docs/troubleshooting.md` for common failures and fixes.

## Security Notes

- Do not place secret values in config files or command lines.
- Key Vault reference mode is the default and safest mode.
- Copy-value mode requires explicit confirmations.
