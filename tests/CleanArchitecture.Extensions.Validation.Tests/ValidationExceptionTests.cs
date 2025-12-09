using CleanArchitecture.Extensions.Validation.Exceptions;
using FluentValidation.Results;

namespace CleanArchitecture.Extensions.Validation.Tests;

/// <summary>
/// Tests covering ValidationException shape and grouping.
/// </summary>
public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_GroupsFailuresByProperty()
    {
        var failures = new[]
        {
            new ValidationFailure("Name", "required"),
            new ValidationFailure("Name", "length"),
            new ValidationFailure("Email", "invalid")
        };

        var exception = new ValidationException(failures);

        Assert.Equal(2, exception.Errors.Count);
        Assert.Equal(new[] { "required", "length" }, exception.Errors["Name"]);
        Assert.Equal(new[] { "invalid" }, exception.Errors["Email"]);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyErrors()
    {
        var exception = new ValidationException();

        Assert.Empty(exception.Errors);
        Assert.Contains("validation failures", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
