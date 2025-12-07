using System.Linq;
using System.Text.RegularExpressions;

namespace CleanArchitecture.Extensions.Exceptions.Redaction;

/// <summary>
/// Provides lightweight redaction for exception messages and metadata to avoid leaking secrets.
/// </summary>
public sealed class ExceptionRedactor
{
    private static readonly string[] SensitiveKeys =
    {
        "password",
        "passphrase",
        "pwd",
        "token",
        "auth",
        "auth-token",
        "access-token",
        "refresh-token",
        "secret",
        "client-secret",
        "authorization",
        "cookie",
        "session",
        "sessionid",
        "apikey",
        "api-key",
        "api_key",
        "credential",
        "jwt",
        "bearer"
    };

    private static readonly Regex EmailRegex = new(
        @"(?ix)(?<![\w@])[\p{L}\p{N}._%+-]+@(?:[\p{L}\p{N}-]+\.)+[\p{L}\p{N}]{2,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex BearerTokenRegex = new(
        @"(?ix)\bBearer\s+(?<value>[A-Za-z0-9\-\._~+/]+=*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SensitiveKeyValueRegex = new(
        @"(?ix)
            (?<key>
                password|passphrase|pwd|
                token|auth[_-]?token|access[_-]?token|refresh[_-]?token|
                secret|client[_-]?secret|
                authorization|auth|
                cookie|session(?:id)?|
                api(?:key|[-_ ]?key|[-_ ]?secret)|
                credential|jwt|bearer
            )
            (?<separator>\s*[:=]?\s+)
            (?<value>""[^""]*""|'[^']*'|[^\s;,]+)
        ",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Redacts sensitive values from a string.
    /// </summary>
    /// <param name="value">Value to redact.</param>
    /// <returns>Redacted value.</returns>
    public string Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var redacted = EmailRegex.Replace(value, "[redacted-email]");
        redacted = BearerTokenRegex.Replace(redacted, "Bearer [redacted]");
        redacted = SensitiveKeyValueRegex.Replace(
            redacted,
            match => $"{match.Groups["key"].Value}{match.Groups["separator"].Value}[redacted]");
        return redacted;
    }

    /// <summary>
    /// Redacts sensitive entries from metadata.
    /// </summary>
    /// <param name="metadata">Metadata to scrub.</param>
    /// <returns>Redacted metadata.</returns>
    public IReadOnlyDictionary<string, string> RedactMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return metadata ?? new Dictionary<string, string>();
        }

        var result = new Dictionary<string, string>(metadata.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var pair in metadata)
        {
            if (SensitiveKeys.Any(key => pair.Key.Contains(key, StringComparison.OrdinalIgnoreCase)))
            {
                result[pair.Key] = "[redacted]";
                continue;
            }

            result[pair.Key] = Redact(pair.Value);
        }

        return result;
    }
}
