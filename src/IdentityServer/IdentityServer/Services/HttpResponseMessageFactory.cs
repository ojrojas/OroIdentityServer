using System.Net;
using BuildingBlocks.Kernel.Results;

namespace IdentityServer.Services;

/// <summary>
/// IAdminXxxService write methods return HttpResponseMessage so the same interface works for both
/// the HTTP-based client implementation and this CQRS-based server implementation. This builds that
/// HttpResponseMessage from a command Result, without an actual HTTP round-trip.
/// </summary>
internal static class HttpResponseMessageFactory
{
    public static HttpResponseMessage FromResult(Result result, HttpStatusCode successStatusCode)
        => new(result.IsSuccess ? successStatusCode : ToStatusCode(result.Error.Type));

    private static HttpStatusCode ToStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => HttpStatusCode.NotFound,
        ErrorType.Validation => HttpStatusCode.BadRequest,
        ErrorType.Conflict => HttpStatusCode.Conflict,
        ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
        _ => HttpStatusCode.BadRequest
    };
}
