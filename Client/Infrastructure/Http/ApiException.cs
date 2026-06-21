using System.Net;
using Client.Models.Common;

namespace Client.Infrastructure.Http;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ApiResponse<object>? ApiResponse { get; }

    public ApiException(HttpStatusCode statusCode, string message, ApiResponse<object>? apiResponse = null)
        : base(message)
    {
        StatusCode = statusCode;
        ApiResponse = apiResponse;
    }
}
