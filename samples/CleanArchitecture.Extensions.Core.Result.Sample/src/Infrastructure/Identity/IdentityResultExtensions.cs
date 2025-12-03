using CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using ApplicationResult = CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Models.Result;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static ApplicationResult ToApplicationResult(this IdentityResult result)
    {
        return result.Succeeded
            ? ApplicationResult.Success()
            : ApplicationResult.Failure(result.Errors.Select(e => e.Description));
    }
}
