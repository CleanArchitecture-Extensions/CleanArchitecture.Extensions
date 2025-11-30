using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Guards;

/// <summary>
/// Configures guard clause behavior and error handling strategy.
/// </summary>
public sealed class GuardOptions
{
    /// <summary>
    /// Gets or sets the strategy used when a guard fails.
    /// </summary>
    public GuardStrategy Strategy { get; init; } = GuardStrategy.ReturnFailure;

    /// <summary>
    /// Gets or sets the collection that accumulates errors when using the accumulate strategy.
    /// </summary>
    public ICollection<Error>? ErrorSink { get; init; }

    /// <summary>
    /// Gets or sets the factory used to create exceptions when the throw strategy is selected.
    /// </summary>
    public Func<Error, Exception>? ExceptionFactory { get; init; }

    /// <summary>
    /// Gets or sets the trace identifier to attach to errors produced by guards.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Creates a <see cref="GuardOptions"/> instance from shared <see cref="CoreExtensionsOptions"/>.
    /// </summary>
    /// <param name="options">Shared core options.</param>
    /// <param name="errorSink">Optional error sink to use for accumulation.</param>
    /// <returns>A configured <see cref="GuardOptions"/> instance.</returns>
    public static GuardOptions FromOptions(CoreExtensionsOptions options, ICollection<Error>? errorSink = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new GuardOptions
        {
            Strategy = options.GuardStrategy,
            ErrorSink = errorSink,
            TraceId = options.TraceId
        };
    }
}
