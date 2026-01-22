namespace KeyVaultToAppConfig.Core.Auth;

public sealed class AuthResult
{
    public bool IsSuccess { get; init; }
    public CredentialSource? SelectedSource { get; init; }
    public AuthErrorCategory ErrorCategory { get; init; } = AuthErrorCategory.None;
    public string ErrorMessage { get; init; } = string.Empty;
    public string ErrorDetail { get; init; } = string.Empty;
    public int RetryCount { get; init; }
}

public enum AuthErrorCategory
{
    None,
    Transient,
    Fatal,
    Rbac
}
