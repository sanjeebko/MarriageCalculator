using System.Text.Json;
using System.Text;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories;

/// <summary>
/// Service for communicating with the MarriageCalculator API
/// </summary>
public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
    Task<bool> TestConnectionAsync();
}

/// <summary>
/// Implementation of API service for HTTP communication with MarriageCalculator.API
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
            
            var response = await _httpClient.GetAsync("api/database/info", cts.Token).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
                return false;
            
            var responseStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
            var databaseInfo = await JsonSerializer.DeserializeAsync<DatabaseInfoDto>(responseStream, _jsonOptions, cts.Token).ConfigureAwait(false);
            
            return databaseInfo?.CanConnect == true;
        }
        catch
        {
            return false;
        }
    }
}