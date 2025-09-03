using MarriageCalculator.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace MarriageCalculator.Services.Implementations;

/// <summary>
/// Implementation of API service for HTTP communication with MarriageCalculator.API
/// Provides HTTP client functionality and handles authentication, serialization, and error handling
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _baseUrl;

    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7294";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            
            throw new HttpRequestException($"API request failed with status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling GET {endpoint}: {ex.Message}", ex);
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
            }
            
            // For authentication endpoints, also return error responses
            var errorJson = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(errorJson))
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<T>(errorJson, _jsonOptions);
                    return errorResponse;
                }
                catch
                {
                    // If can't deserialize error response, throw original error
                }
            }
            
            throw new HttpRequestException($"API request failed with status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling POST {endpoint}: {ex.Message}", ex);
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
            }
            
            throw new HttpRequestException($"API request failed with status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling PUT {endpoint}: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling DELETE {endpoint}: {ex.Message}", ex);
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            // Use a lightweight AUTHENTICATED endpoint that does not depend on DB connectivity
            // This checks that: (1) API is reachable, (2) token is present/valid
            var response = await _httpClient.GetAsync("api/UserAuth/me", cts.Token).ConfigureAwait(false);
            
            if (response.IsSuccessStatusCode)
            {
                // 200 OK => API reachable and token valid
                System.Diagnostics.Debug.WriteLine("Connection test: Auth endpoint responded 200 OK");
                return true;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("Connection test failed: 401 Unauthorized - Token might be expired or missing");
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine($"Connection test failed with status: {response.StatusCode}");
            return false;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection test failed with HTTP error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection test timed out: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection test failed with error: {ex.Message}");
            return false;
        }
    }

    public async Task SetAuthTokenAsync(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            System.Diagnostics.Debug.WriteLine($"ApiService: Auth token set. Token length: {token.Length}");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            System.Diagnostics.Debug.WriteLine("ApiService: Auth token cleared (empty token provided)");
        }
        await Task.CompletedTask;
    }

    public async Task ClearAuthTokenAsync()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        System.Diagnostics.Debug.WriteLine("ApiService: Auth token cleared");
        await Task.CompletedTask;
    }
}