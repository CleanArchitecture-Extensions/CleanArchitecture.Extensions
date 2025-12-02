using FluentValidation.Results;

namespace CleanArchitecture.Extensions.Validation.Exceptions;

/// <summary>
/// Represents validation failures, maintaining compatibility with the Clean Architecture template shape.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class from validation failures.
    /// </summary>
    /// <param name="failures">Validation failures to surface.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        if (failures is null)
        {
            throw new ArgumentNullException(nameof(failures));
        }

        Errors = failures
            .GroupBy(failure => failure.PropertyName ?? string.Empty, failure => failure.ErrorMessage ?? string.Empty)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the grouped validation errors (property name to messages).
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}
