using Microsoft.Extensions.Options;
using CleanArchitecture.Extensions.Core.Time;

namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Ensures the domain event time source uses the configured clock when options are materialized.
/// </summary>
internal sealed class DomainEventTimePostConfigure : IPostConfigureOptions<Options.CoreExtensionsOptions>
{
    private readonly IClock _clock;

    public DomainEventTimePostConfigure(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void PostConfigure(string? name, Options.CoreExtensionsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        DomainEventTime.SetProvider(() => _clock.UtcNow);
    }
}
