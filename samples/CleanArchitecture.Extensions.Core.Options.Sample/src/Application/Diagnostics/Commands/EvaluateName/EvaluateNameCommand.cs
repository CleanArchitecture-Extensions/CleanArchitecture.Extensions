using CleanArchitecture.Extensions.Core.Guards;
using CleanArchitecture.Extensions.Core.Options;
using Microsoft.Extensions.Options;
using CoreResults = CleanArchitecture.Extensions.Core.Results;
using CoreGuard = CleanArchitecture.Extensions.Core.Guards.Guard;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Application.Diagnostics.Commands.EvaluateName;

public sealed record EvaluateNameCommand(string? Name) : IRequest<CoreResults.Result<string>>;

public sealed class EvaluateNameCommandHandler : IRequestHandler<EvaluateNameCommand, CoreResults.Result<string>>
{
    private readonly IOptions<CoreExtensionsOptions> _options;

    public EvaluateNameCommandHandler(IOptions<CoreExtensionsOptions> options)
    {
        _options = options;
    }

    public Task<CoreResults.Result<string>> Handle(EvaluateNameCommand request, CancellationToken cancellationToken)
    {
        var guardOptions = GuardOptions.FromOptions(_options.Value);

        var result = CoreGuard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name), guardOptions)
            .Ensure(n => n.Length <= 50, new CoreResults.Error("diagnostics.name.length", "Name must be 50 characters or fewer.", guardOptions.TraceId));

        return Task.FromResult(result);
    }
}
