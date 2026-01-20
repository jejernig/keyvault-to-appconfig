namespace KeyVaultToAppConfig.Cli;

public static class HelpText
{
    public const string Usage = """
Key Vault to App Configuration Standardizer

Required:
  --keyvault-uri <uri>          Source Key Vault URI
  --appconfig-endpoint <uri>    Target App Configuration endpoint

Execution:
  --dry-run                     Preview without writes (default)
  --diff                        Show planned changes
  --apply                       Apply changes

Options:
  --mode <kvref|copyvalue>      Secret handling mode (default: kvref)
  --confirm-copy-value          Required when --mode copyvalue
  --environment <label>
  --mapping-file <path>
  --parallelism <n>
  --include-prefix <prefix>
  --exclude-regex <pattern>
  --only-tag <key=value>
  --report-json <path>
""";
}
