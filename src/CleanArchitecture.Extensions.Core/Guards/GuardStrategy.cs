namespace CleanArchitecture.Extensions.Core.Guards;

/// <summary>
/// Defines how guard clause failures are handled.
/// </summary>
public enum GuardStrategy
{
    /// <summary>
    /// Guards return a failure result on validation errors.
    /// </summary>
    ReturnFailure = 0,

    /// <summary>
    /// Guards throw an exception on validation errors.
    /// </summary>
    Throw = 1,

    /// <summary>
    /// Guards add errors to a provided sink for later inspection.
    /// </summary>
    Accumulate = 2
}
