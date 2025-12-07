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
            [typeof(NotFoundException)] = new ExceptionDescriptor(
                typeof(NotFoundException),
                ExceptionCodes.NotFound,
                "The specified resource was not found.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.NotFound),
            [typeof(ConflictException)] = new ExceptionDescriptor(
                typeof(ConflictException),
                ExceptionCodes.Conflict,
                "The request conflicts with the current state of the resource.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.Conflict),
            [typeof(ForbiddenException)] = new ExceptionDescriptor(
                typeof(ForbiddenException),
                ExceptionCodes.Forbidden,
                "Forbidden.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.Forbidden),
            [typeof(ForbiddenAccessException)] = new ExceptionDescriptor(
                typeof(ForbiddenAccessException),
                ExceptionCodes.Forbidden,
                "Forbidden.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.Forbidden),
            [typeof(UnauthorizedException)] = new ExceptionDescriptor(
                typeof(UnauthorizedException),
                ExceptionCodes.Unauthorized,
                "Unauthorized.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.Unauthorized),
            [typeof(ConcurrencyException)] = new ExceptionDescriptor(
                typeof(ConcurrencyException),
                ExceptionCodes.Concurrency,
                "A concurrency conflict occurred.",
                ExceptionSeverity.Warning,
                true,
                HttpStatusCode.Conflict),
            [typeof(TransientException)] = new ExceptionDescriptor(
                typeof(TransientException),
                ExceptionCodes.Transient,
                "A transient failure occurred.",
                ExceptionSeverity.Warning,
                true,
                HttpStatusCode.ServiceUnavailable),
            [typeof(DomainException)] = new ExceptionDescriptor(
                typeof(DomainException),
                ExceptionCodes.Domain,
                "A domain rule was violated.",
                ExceptionSeverity.Error),
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
            [typeof(ArgumentException)] = new ExceptionDescriptor(
                typeof(ArgumentException),
                ExceptionCodes.Validation,
                "One or more arguments were invalid.",
                ExceptionSeverity.Warning,
                false,
                HttpStatusCode.BadRequest),
            [typeof(KeyNotFoundException)] = new ExceptionDescriptor(
                typeof(KeyNotFoundException),
                ExceptionCodes.NotFound,
                "The specified resource was not found.",
                ExceptionSeverity.Error,
                false,
                HttpStatusCode.NotFound),
        };

        TryAddValidationDescriptor(descriptors);
        TryAddTimeoutDescriptor(descriptors);
        TryAddEfConcurrencyDescriptor(descriptors);
        TryAddHttpRequestDescriptor(descriptors);

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

    private static void TryAddEfConcurrencyDescriptor(IDictionary<Type, ExceptionDescriptor> descriptors)
    {
        const string typeName = "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException, Microsoft.EntityFrameworkCore";
        var type = Type.GetType(typeName, throwOnError: false);
        if (type is not null)
        {
            descriptors[type] = new ExceptionDescriptor(
                type,
                ExceptionCodes.Concurrency,
                "A concurrency conflict occurred.",
                ExceptionSeverity.Warning,
                true,
                HttpStatusCode.Conflict);
        }
    }

    private static void TryAddHttpRequestDescriptor(IDictionary<Type, ExceptionDescriptor> descriptors)
    {
        const string typeName = "System.Net.Http.HttpRequestException, System.Net.Http";
        var type = Type.GetType(typeName, throwOnError: false);
        if (type is not null)
        {
            descriptors[type] = new ExceptionDescriptor(
                type,
                ExceptionCodes.Transient,
                "A downstream HTTP request failed.",
                ExceptionSeverity.Warning,
                true,
                HttpStatusCode.BadGateway);
        }
    }
}
