using CleanArchitecture.Extensions.Validation.Models;

namespace CleanArchitecture.Extensions.Validation.Notifications;

/// <summary>
/// Publishes validation failures to downstream observers (logging, metrics, or UI notifications).
/// </summary>
public interface IValidationNotificationPublisher
{
    /// <summary>
    /// Publishes validation errors.
    /// </summary>
    /// <param name="errors">Errors to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the publish operation.</returns>
    Task PublishAsync(IReadOnlyList<ValidationError> errors, CancellationToken cancellationToken = default);
}
