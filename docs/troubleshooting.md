# Troubleshooting

| Symptom | Likely Cause | Resolution | Example Error |
|---------|--------------|------------|---------------|
| Authentication failure | Missing or incorrect RBAC role | Assign Key Vault Secrets User and App Configuration role | auth: forbidden |
| Dry-run shows zero secrets | Filters exclude all secrets | Remove include/exclude filters or tags | enumeration: no results |
| Copy-value blocked | Missing confirmation flag | Add --confirm-copy-value | guardrail: confirmation required |
| Report not written | Invalid path or permissions | Ensure output directory exists and is writable | report-json: access denied |
| Invalid version mode | Missing or bad version map | Provide --version-map for explicit mode | version-mode: explicit requires version map |

## Notes

- Error messages are redacted to avoid secret exposure.
- Include correlation identifier in support requests.
