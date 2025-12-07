using System.Reflection;

namespace CleanArchitecture.Extensions.Core.Results;

/// <summary>
/// Reflection helpers for creating <see cref="Result{T}"/> instances when the value type is resolved at runtime.
/// </summary>
internal static class ResultFailureFactory
{
    private static readonly MethodInfo GenericFailureMethod = typeof(Result)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(method =>
            method.IsGenericMethod &&
            method.Name == nameof(Result.Failure) &&
            method.GetParameters() is { Length: 2 } parameters &&
            parameters[0].ParameterType == typeof(IEnumerable<Error>));

    internal static object CreateGenericFailure(Type valueType, IEnumerable<Error> errors, string? traceId)
    {
        if (valueType is null)
        {
            throw new ArgumentNullException(nameof(valueType));
        }

        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        var closed = GenericFailureMethod.MakeGenericMethod(valueType);
        return closed.Invoke(null, new object?[] { errors, traceId })!;
    }
}
