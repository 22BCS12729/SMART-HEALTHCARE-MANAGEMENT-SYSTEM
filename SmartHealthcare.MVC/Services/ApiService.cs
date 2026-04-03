using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.MVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiService> _logger;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            // Set base address from configuration
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7002/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
        }

        private void SetAuthorizationHeader()
        {
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/{endpoint}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to {Endpoint}", endpoint);
                    return default;
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API GET {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/{endpoint}", content);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to {Endpoint}", endpoint);
                    return default;
                }

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API POST {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/{endpoint}", content);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to {Endpoint}", endpoint);
                    return default;
                }

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API PUT {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"api/{endpoint}", content);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to {Endpoint}", endpoint);
                    return default;
                }

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API PATCH {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/{endpoint}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to {Endpoint}", endpoint);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API DELETE {Endpoint}", endpoint);
                return false;
            }
        }

        // Auth specific methods
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Auth/login", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed: {Error}", errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public void SetToken(string token)
        {
            _httpContextAccessor.HttpContext?.Session.SetString("JWTToken", token);
        }

        public void ClearToken()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("JWTToken");
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetToken());
        }
    }
}
