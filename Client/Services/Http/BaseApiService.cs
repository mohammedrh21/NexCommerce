using System.Net.Http.Json;
using System.Text.Json;
using Client.Infrastructure.Http;
using Client.Models.Common;

namespace Client.Services.Http;

public abstract class BaseApiService
{
    protected readonly HttpClient HttpClient;
    protected static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected BaseApiService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await HttpClient.GetAsync(url, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    protected async Task<T> PostAsync<TRequest, T>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    protected async Task<T> PutAsync<TRequest, T>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await HttpClient.PutAsJsonAsync(url, request, JsonOptions, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    protected async Task<T> PatchAsync<TRequest, T>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await HttpClient.PatchAsJsonAsync(url, request, JsonOptions, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    protected async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var response = await HttpClient.DeleteAsync(url, ct);
        await HandleResponseAsync<object>(response, ct);
    }

    protected async Task<T> DeleteAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await HttpClient.DeleteAsync(url, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    protected static async Task<T> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            if (result != null)
            {
                if (result.Success)
                {
                    // For endpoints returning object or empty Data payloads
                    if (result.Data == null)
                    {
                        return default!;
                    }
                    return result.Data;
                }
                
                throw new ApiException(response.StatusCode, result.Message, ConvertToGenericResponse(result));
            }
            throw new ApiException(response.StatusCode, "Empty or invalid response from server.");
        }
        else
        {
            ApiResponse<object>? errorResponse = null;
            string message = $"API call failed with status: {response.StatusCode}";

            try
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, JsonOptions);
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                    {
                        message = errorResponse.Message;
                    }
                }
            }
            catch
            {
                // Fallback for non-JSON responses
            }

            throw new ApiException(response.StatusCode, message, errorResponse);
        }
    }

    private static ApiResponse<object> ConvertToGenericResponse<T>(ApiResponse<T> response)
    {
        return new ApiResponse<object>
        {
            Success = response.Success,
            Message = response.Message,
            Errors = response.Errors,
            Data = response.Data
        };
    }
}
