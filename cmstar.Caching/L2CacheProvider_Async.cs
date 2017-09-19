using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    // L2CacheProvider 的异步方法部分。
    public partial class L2CacheProvider
    {
        /// <inheritdoc cref="ICacheProvider.GetAsync{T}"/>
        public async Task<T> GetAsync<T>(string key)
        {
            var res = await TryGetAsync<T>(key);
            return res.Value;
        }

        /// <inheritdoc cref="ICacheProvider.TryGetAsync{T}"/>
        public async Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            var res = await _level1.TryGetAsync<T>(key);
            if (res.HasValue)
                return res;

            res = await _level2.TryGetAsync<T>(key);
            if (res.HasValue)
            {
                await SetLevel1Async(key, res.Value);
                return res;
            }

            return new TryGetResult<T>(false, default(T));
        }

        /// <inheritdoc cref="ICacheProvider.CreateAsync{T}"/>
        public async Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            if (!(await _level2.CreateAsync(key, value, expiration)))
                return false;

            await SetLevel1Async(key, value);
            return true;
        }

        /// <inheritdoc cref="ICacheProvider.SetAsync{T}"/>
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            await SetLevel1Async(key, value);
            Log($"Set into level1 with key {key}");

            await _level2.SetAsync(key, value, expiration);
            Log($"Set into level2 with key {key}");
        }

        /// <inheritdoc cref="ICacheProvider.RemoveAsync"/>
        public async Task<bool> RemoveAsync(string key)
        {
            var res2 = await _level2.RemoveAsync(key);
            Log($"Remove from level2: {res2}");

            var res1 = await _level1.RemoveAsync(key);
            Log($"Remove from level1: {res1}");

            return res2;
        }

        private Task SetLevel1Async<T>(string key, T value)
        {
            var level1ExpirationSeconds = _level1Expiration.NewExpirationSeconds();
            return _level1.SetAsync(key, value, TimeSpan.FromSeconds(level1ExpirationSeconds));
        }
    }
}
