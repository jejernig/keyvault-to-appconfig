# Contracts: Error Handling & Resilience

This feature does not introduce external HTTP APIs. The contract is defined by CLI outcomes and the run report content.

## Exit Code Contract

- 0: Success (no errors)
- 1: Recoverable failures (one or more secrets failed, run completed)
- 2: Fatal failure (run stopped due to fatal error)
- 3: Canceled (run stopped by cancellation)

## Outcome Reporting

The run report must include counts for success, recoverable failures, and unprocessed secrets, plus a clear run outcome and exit code.
