namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Marker used to initialize the domain event time provider during DI setup.
/// </summary>
internal sealed class DomainEventTimeMarker
{
    public static readonly DomainEventTimeMarker Instance = new();

    private DomainEventTimeMarker()
    {
    }
}
