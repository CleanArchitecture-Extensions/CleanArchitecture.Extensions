using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Guards;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Application.Diagnostics.Queries.GetCoreOptions;

public sealed record CoreOptionsDto(string CorrelationHeaderName, GuardStrategy GuardStrategy, bool EnablePerformanceLogging, TimeSpan PerformanceWarningThreshold);

public sealed record GetCoreOptionsQuery : IRequest<CoreOptionsDto>;

public sealed class GetCoreOptionsQueryHandler : IRequestHandler<GetCoreOptionsQuery, CoreOptionsDto>
{
    private readonly IOptions<CoreExtensionsOptions> _options;

    public GetCoreOptionsQueryHandler(IOptions<CoreExtensionsOptions> options)
    {
        _options = options;
    }

    public Task<CoreOptionsDto> Handle(GetCoreOptionsQuery request, CancellationToken cancellationToken)
    {
        var value = _options.Value;
        var dto = new CoreOptionsDto(
            value.CorrelationHeaderName,
            value.GuardStrategy,
            value.EnablePerformanceLogging,
            value.PerformanceWarningThreshold);

        return Task.FromResult(dto);
    }
}
