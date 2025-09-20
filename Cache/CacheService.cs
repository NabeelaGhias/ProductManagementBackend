using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ProductManagementSystem.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetData<T>(string key)
        {
            var value = _memoryCache.Get(key);
            if (value != null)
            {
                return Task.FromResult(JsonConvert.DeserializeObject<T>(value.ToString()!));
            }
            return Task.FromResult(default(T));
        }

        public Task<bool> SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            _memoryCache.Set(key, serializedValue, expirationTime);
            return Task.FromResult(true);
        }

        public Task<object> RemoveData(string key)
        {
            _memoryCache.Remove(key);
            return Task.FromResult((object)true);
        }
    }
}
