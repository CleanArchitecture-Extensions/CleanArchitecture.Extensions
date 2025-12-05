using CleanArchitecture.Extensions.Core.DomainEvents;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Common;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Data.Interceptors;

public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly DomainEventTracker _tracker;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogContext _logContext;
    private readonly CoreExtensionsOptions _options;

    public DispatchDomainEventsInterceptor(
        DomainEventTracker tracker,
        IDomainEventDispatcher dispatcher,
        ILogContext logContext,
        IOptions<CoreExtensionsOptions> options)
    {
        _tracker = tracker;
        _dispatcher = dispatcher;
        _logContext = logContext;
        _options = options.Value;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureDomainEvents(eventData.Context);
        DispatchTrackedEventsAsync(CancellationToken.None).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureDomainEvents(eventData.Context);
        await DispatchTrackedEventsAsync(cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureDomainEvents(DbContext? context)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity);

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                _tracker.Add(domainEvent);
            }

            entity.ClearDomainEvents();
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

        await _dispatcher.DispatchAsync(correlatedEvents, cancellationToken);
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

        return domainEvent switch
        {
            BaseEvent baseEvent => baseEvent with { CorrelationId = correlationId },
            _ => domainEvent with { CorrelationId = correlationId }
        };
    }
}
