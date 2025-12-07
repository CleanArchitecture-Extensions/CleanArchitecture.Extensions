using System.Linq;
using System.Text.RegularExpressions;

namespace CleanArchitecture.Extensions.Exceptions.Redaction;

/// <summary>
/// Provides lightweight redaction for exception messages and metadata to avoid leaking secrets.
/// </summary>
public sealed class ExceptionRedactor
{
    private static readonly string[] SensitiveKeys = { "password", "pwd", "token", "secret", "authorization", "cookie", "apikey", "api-key", "credential" };
    private static readonly Regex SensitiveValueRegex = new(@"(?i)(password|pwd|token|secret|authorization|cookie|apikey|api-key)[=:]\s*([^\s;]+)", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);

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
        redacted = SensitiveValueRegex.Replace(redacted, "$1=[redacted]");
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
