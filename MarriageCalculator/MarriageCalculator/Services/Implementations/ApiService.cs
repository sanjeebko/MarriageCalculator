using MarriageCalculator.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public string Token { get; private set; }
    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7294";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)); // Increased timeout
            
            System.Diagnostics.Debug.WriteLine($"=== API CONNECTION TEST ===");
            System.Diagnostics.Debug.WriteLine($"Testing connection to: {_baseUrl}");
            System.Diagnostics.Debug.WriteLine($"Current auth token present: {!string.IsNullOrEmpty(Token)}");
            System.Diagnostics.Debug.WriteLine($"Auth header present: {_httpClient.DefaultRequestHeaders.Authorization != null}");
            
            if (_httpClient.DefaultRequestHeaders.Authorization != null)
            {
                System.Diagnostics.Debug.WriteLine($"Auth header scheme: {_httpClient.DefaultRequestHeaders.Authorization.Scheme}");
                var paramLength = _httpClient.DefaultRequestHeaders.Authorization.Parameter?.Length ?? 0;
                System.Diagnostics.Debug.WriteLine($"Auth header parameter length: {paramLength}");
            }
            
            // First, try a simple ping to check if the server is reachable
            try
            {
                System.Diagnostics.Debug.WriteLine("Testing basic server reachability...");
                var pingResponse = await _httpClient.GetAsync("", cts.Token).ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine($"Server ping response: {pingResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine("✓ Server is reachable");
            }
            catch (Exception pingEx)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Server ping failed: {pingEx.Message}");
                return false; // If we can't reach the server at all, return false
            }
            
            // Now test the authenticated endpoint
            System.Diagnostics.Debug.WriteLine("Testing authenticated endpoint /api/UserAuth/me...");
            var response = await _httpClient.GetAsync("api/UserAuth/me", cts.Token).ConfigureAwait(false);
            
            System.Diagnostics.Debug.WriteLine($"Auth endpoint response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // 200 OK => API reachable and token valid
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"✓ Auth endpoint success: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response length: {content?.Length ?? 0} characters");
                return true;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                System.Diagnostics.Debug.WriteLine("✗ 401 Unauthorized - Token might be expired or missing");
                
                // Try to get more details about the auth failure
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response: {errorContent}");
                
                // Check if we have a token at all
                if (string.IsNullOrEmpty(Token))
                {
                    System.Diagnostics.Debug.WriteLine("No token available - user needs to login");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Token present but authentication failed - token might be expired");
                }
                
                // Return false for 401 - authentication issue
                return false;
            }
            
            // For any other response code, log it but consider server reachable
            System.Diagnostics.Debug.WriteLine($"Unexpected response status: {response.StatusCode}");
            var unexpectedContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response content: {unexpectedContent}");
            
            // Server responded, so it's reachable, even if the specific endpoint had issues
            System.Diagnostics.Debug.WriteLine("Server is responding but with unexpected status");
            return true;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ HTTP request failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Target URL: {_baseUrl}");
            
            // Check specific error types
            if (ex.Message.Contains("Name or service not known") || 
                ex.Message.Contains("No such host is known") ||
                ex.Message.Contains("getaddrinfo failed"))
            {
                System.Diagnostics.Debug.WriteLine("DNS resolution failed - check server hostname");
            }
            else if (ex.Message.Contains("Connection refused") || ex.Message.Contains("refused"))
            {
                System.Diagnostics.Debug.WriteLine("Connection refused - server might not be running on expected port");
            }
            else if (ex.Message.Contains("timeout") || ex.Message.Contains("timed out"))
            {
                System.Diagnostics.Debug.WriteLine("Request timed out - server might be slow or unreachable");
            }
            else if (ex.Message.Contains("SSL") || ex.Message.Contains("certificate"))
            {
                System.Diagnostics.Debug.WriteLine("SSL/Certificate issue - check HTTPS configuration");
            }
            
            return false;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ Request timed out: {ex.Message}");
            System.Diagnostics.Debug.WriteLine("Connection test timed out after 15 seconds");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ Unexpected exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine($"=== END CONNECTION TEST ===");
        }
    }

    public async Task SetAuthTokenAsync(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            Token = token;
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