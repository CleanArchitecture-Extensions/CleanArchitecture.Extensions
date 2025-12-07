using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// EF Core interceptor that captures domain events from aggregates implementing <see cref="IHasDomainEvents"/> and dispatches them via <see cref="IDomainEventDispatcher"/>.
/// </summary>
public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly DomainEventTracker _tracker;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogContext _logContext;
    private readonly CoreExtensionsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatchDomainEventsInterceptor"/> class.
    /// </summary>
    public DispatchDomainEventsInterceptor(
        DomainEventTracker tracker,
        IDomainEventDispatcher dispatcher,
        ILogContext logContext,
        IOptions<CoreExtensionsOptions> options)
    {
        _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureDomainEvents(eventData.Context);
        DispatchTrackedEventsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureDomainEvents(eventData.Context);
        await DispatchTrackedEventsAsync(cancellationToken).ConfigureAwait(false);
        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private void CaptureDomainEvents(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var aggregates = context.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Select(entity => entity.DequeueDomainEvents())
            .Where(events => events.Count != 0);

        foreach (var events in aggregates)
        {
            foreach (var domainEvent in events)
            {
                _tracker.Add(domainEvent);
            }
        }
    }

    private async Task DispatchTrackedEventsAsync(CancellationToken cancellationToken)
    {
        if (!_tracker.HasEvents)
        {
            return;
        }

        var correlationId = EnsureCorrelationId();
        var correlatedEvents = _tracker
            .Drain()
            .Select(domainEvent => EnsureCorrelation(domainEvent, correlationId))
            .ToArray();

        if (correlatedEvents.Length == 0)
        {
            return;
        }

        await _dispatcher.DispatchAsync(correlatedEvents, cancellationToken).ConfigureAwait(false);
    }

    private string EnsureCorrelationId()
    {
        if (!string.IsNullOrWhiteSpace(_logContext.CorrelationId))
        {
            return _logContext.CorrelationId!;
        }

        var correlationId = _options.CorrelationIdFactory();
        _logContext.CorrelationId = correlationId;
        return correlationId;
    }

    private static DomainEvent EnsureCorrelation(DomainEvent domainEvent, string correlationId)
    {
        if (!string.IsNullOrWhiteSpace(domainEvent.CorrelationId))
        {
            return domainEvent;
        }

        return domainEvent with { CorrelationId = correlationId };
    }
}
