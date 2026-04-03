using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SmartHealthcare.API.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }

    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(data))
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };

                var data = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, data, options);
                _logger.LogDebug("Cached data for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Removed cache for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(data);
            }
            catch
            {
                return false;
            }
        }
    }
}
