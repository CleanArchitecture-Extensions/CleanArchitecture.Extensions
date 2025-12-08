namespace CleanArchitecture.Extensions.Core.Options;

/// <summary>
/// Controls which CleanArchitecture.Extensions.Core MediatR pipeline components are registered.
/// </summary>
public sealed class CorePipelineOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to register the logging pre-processor.
    /// </summary>
    public bool UseLoggingPreProcessor { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to register the correlation behavior.
    /// </summary>
    public bool UseCorrelationBehavior { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to register the logging behavior.
    /// </summary>
    public bool UseLoggingBehavior { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to register the performance behavior.
    /// </summary>
    public bool UsePerformanceBehavior { get; set; } = true;
}
