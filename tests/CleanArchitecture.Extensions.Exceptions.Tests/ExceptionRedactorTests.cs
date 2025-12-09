using CleanArchitecture.Extensions.Exceptions.Redaction;

namespace CleanArchitecture.Extensions.Exceptions.Tests;

/// <summary>
/// Tests covering sensitive data redaction helpers.
/// </summary>
public class ExceptionRedactorTests
{
    [Fact]
    public void Redact_ScrubsEmailsTokensAndKeyValuePairs()
    {
        var redactor = new ExceptionRedactor();
        const string message = "user test@example.com sent Bearer abc123 and password secret";

        var redacted = redactor.Redact(message);

        Assert.DoesNotContain("test@example.com", redacted);
        Assert.DoesNotContain("abc123", redacted);
        Assert.DoesNotContain("secret", redacted);
        Assert.Contains("[redacted-email]", redacted);
        Assert.Contains("Bearer [redacted]", redacted);
        Assert.Contains("[redacted]", redacted);
    }

    [Fact]
    public void RedactMetadata_ScrubsSensitiveKeysAndValues()
    {
        var redactor = new ExceptionRedactor();
        var metadata = new Dictionary<string, string>
        {
            ["access-token"] = "abc",
            ["note"] = "Email foo@bar.com",
            ["status"] = "500"
        };

        var redacted = redactor.RedactMetadata(metadata);

        Assert.Equal("[redacted]", redacted["access-token"]);
        Assert.Equal("Email [redacted-email]", redacted["note"]);
        Assert.Equal("500", redacted["status"]);
    }
}
