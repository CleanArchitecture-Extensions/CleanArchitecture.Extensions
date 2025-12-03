using CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Models;
using ApplicationResult = CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Models.Result;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(ApplicationResult Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<ApplicationResult> DeleteUserAsync(string userId);
}
