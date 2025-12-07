using System.Net;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;

namespace CleanArchitecture.Extensions.Exceptions.Catalog;

internal static class ExceptionCatalogDefaults
{
    public static ExceptionDescriptor Unknown { get; } =
        new ExceptionDescriptor(typeof(Exception), ExceptionCodes.Unknown, "An unexpected error occurred.", ExceptionSeverity.Error);

    public static IReadOnlyDictionary<Type, ExceptionDescriptor> Descriptors { get; } = BuildDescriptors();

    private static IReadOnlyDictionary<Type, ExceptionDescriptor> BuildDescriptors()
    {
        var descriptors = new Dictionary<Type, ExceptionDescriptor>
        {
            [typeof(UnauthorizedAccessException)] = new ExceptionDescriptor(
                typeof(UnauthorizedAccessException),
                ExceptionCodes.Unauthorized,
                "Unauthorized.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.Unauthorized),
            [typeof(OperationCanceledException)] = new ExceptionDescriptor(
                typeof(OperationCanceledException),
                ExceptionCodes.Cancelled,
                "The operation was canceled.",
                ExceptionSeverity.Info),
        };

        TryAddValidationDescriptor(descriptors);
        TryAddTimeoutDescriptor(descriptors);

        return descriptors;
    }

    private static void TryAddValidationDescriptor(IDictionary<Type, ExceptionDescriptor> descriptors)
    {
        var validationType = Type.GetType("CleanArchitecture.Extensions.Validation.Exceptions.ValidationException, CleanArchitecture.Extensions.Validation", throwOnError: false);
        if (validationType is not null)
        {
            descriptors[validationType] = new ExceptionDescriptor(
                validationType,
                ExceptionCodes.Validation,
                "One or more validation failures have occurred.",
                ExceptionSeverity.Warning,
                false,
                HttpStatusCode.BadRequest);
        }

        var fluentValidationType = Type.GetType("FluentValidation.ValidationException, FluentValidation", throwOnError: false);
        if (fluentValidationType is not null)
        {
            descriptors[fluentValidationType] = new ExceptionDescriptor(
                fluentValidationType,
                ExceptionCodes.Validation,
                "One or more validation failures have occurred.",
                ExceptionSeverity.Warning,
                false,
                HttpStatusCode.BadRequest);
        }
    }

    private static void TryAddTimeoutDescriptor(IDictionary<Type, ExceptionDescriptor> descriptors)
    {
        descriptors[typeof(TimeoutException)] = new ExceptionDescriptor(
            typeof(TimeoutException),
            ExceptionCodes.Transient,
            "The operation timed out.",
            ExceptionSeverity.Warning,
            true,
            HttpStatusCode.RequestTimeout);
    }
}
