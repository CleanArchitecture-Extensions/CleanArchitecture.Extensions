using System.Net;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Redaction;

namespace CleanArchitecture.Extensions.Exceptions.Tests;

/// <summary>
/// Tests covering exception descriptor mapping and redaction.
/// </summary>
public class ExceptionDescriptorTests
{
    [Fact]
    public void ToError_UsesDescriptorDefaults_WhenDetailsHidden()
    {
        var descriptor = new ExceptionDescriptor(
            typeof(NotFoundException),
            ExceptionCodes.NotFound,
            "fallback message",
            ExceptionSeverity.Error,
            isTransient: true,
            statusCode: HttpStatusCode.BadRequest,
            metadata: new Dictionary<string, string> { ["source"] = "catalog" });
        var exception = new NotFoundException("specific message");
        var redactor = new ExceptionRedactor();

        var error = descriptor.ToError(exception, "trace-1", includeExceptionDetails: false, redactSensitiveData: false, redactor);

        Assert.Equal(ExceptionCodes.NotFound, error.Code);
        Assert.Equal("fallback message", error.Message);
        Assert.Equal("trace-1", error.TraceId);
        Assert.Equal("catalog", error.Metadata["source"]);
        Assert.Equal(((int)HttpStatusCode.BadRequest).ToString(), error.Metadata["status"]);
        Assert.Equal(bool.TrueString, error.Metadata["transient"]);
        Assert.Equal(ExceptionSeverity.Error.ToString(), error.Metadata["severity"]);
        Assert.Equal(typeof(NotFoundException).FullName, error.Metadata["exceptionType"]);
    }

    [Fact]
    public void ToError_RedactsWhenConfigured()
    {
        var descriptor = new ExceptionDescriptor(typeof(Exception), "code", "message");
        var exception = new Exception("email test@example.com password=secret");
        var redactor = new ExceptionRedactor();

        var error = descriptor.ToError(exception, null, includeExceptionDetails: true, redactSensitiveData: true, redactor);

        Assert.DoesNotContain("test@example.com", error.Message);
        Assert.Contains("[redacted-email]", error.Message);
        Assert.DoesNotContain("secret", string.Join(' ', error.Metadata.Values));
    }

    [Fact]
    public void FromApplicationException_PreservesMetadataAndStatus()
    {
        var exception = new TestAppException();

        var descriptor = ExceptionDescriptor.FromApplicationException(exception);

        Assert.Equal("TEST.CODE", descriptor.Code);
        Assert.Equal(ExceptionSeverity.Warning, descriptor.Severity);
        Assert.Equal(HttpStatusCode.Accepted, descriptor.StatusCode);
        Assert.Equal("value", descriptor.Metadata["key"]);
    }

    private sealed class TestAppException : ApplicationExceptionBase
    {
        public TestAppException()
            : base("TEST.CODE", "message", ExceptionSeverity.Warning, true, HttpStatusCode.Accepted, metadata: new Dictionary<string, string> { ["key"] = "value" })
        {
        }
    }
}
