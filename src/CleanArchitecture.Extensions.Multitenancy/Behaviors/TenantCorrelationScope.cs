using System.Threading;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

internal static class TenantCorrelationScope
{
    private static readonly AsyncLocal<IDisposable?> CurrentScope = new();

    public static IDisposable? Current => CurrentScope.Value;

    public static void Set(IDisposable? scope)
    {
        CurrentScope.Value = scope;
    }

    public static IDisposable? Clear()
    {
        var scope = CurrentScope.Value;
        CurrentScope.Value = null;
        return scope;
    }
}
