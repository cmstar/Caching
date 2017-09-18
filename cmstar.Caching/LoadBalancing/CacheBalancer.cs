using System;
using System.Threading.Tasks;

namespace cmstar.Caching.LoadBalancing
{
    /// <summary>
    /// 提供一个简单的缓存负载均衡器。
    /// </summary>
    public class CacheBalancer : ICacheProvider
    {
        private readonly CacheNodeSelector _nodeSelector = new CacheNodeSelector();

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var provider = DetermineProvider(key);
            return provider.Get<T>(key);
        }

        /// <inheritdoc />
        public bool TryGet<T>(string key, out T value)
        {
            var provider = DetermineProvider(key);
            return provider.TryGet(key, out value);
        }

        /// <inheritdoc />
        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            var provider = DetermineProvider(key);
            return provider.Create(key, value, expiration);
        }

        /// <inheritdoc />
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var provider = DetermineProvider(key);
            provider.Set(key, value, expiration);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            var provider = DetermineProvider(key);
            return provider.Remove(key);
        }

#if !NET35
        /// <inheritdoc />
        public Task<T> GetAsync<T>(string key)
        {
            var provider = DetermineProvider(key);
            return provider.GetAsync<T>(key);
        }

        /// <inheritdoc />
        public Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            var provider = DetermineProvider(key);
            return provider.TryGetAsync<T>(key);
        }

        /// <inheritdoc />
        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            var provider = DetermineProvider(key);
            return provider.CreateAsync(key, value, expiration);
        }

        /// <inheritdoc />
        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var provider = DetermineProvider(key);
            return provider.SetAsync(key, value, expiration);
        }

        /// <inheritdoc />
        public Task<bool> RemoveAsync(string key)
        {
            var provider = DetermineProvider(key);
            return provider.RemoveAsync(key);
        }
#endif

        /// <summary>
        /// 添加一个用于负载均衡的缓存提供器。
        /// </summary>
        /// <param name="provider">添加的缓存提供器。</param>
        /// <param name="weight">
        /// 指定该节点所占的比重。必须是正数。节点被使用的概率 = 节点比重 / （所有节点的总比重）。
        /// </param>
        public virtual void AddNode(ICacheProvider provider, int weight = 1)
        {
            _nodeSelector.AddCacheNode(provider, weight);
        }

        /// <summary>
        /// 根据给定的缓存键的哈希值，为其分配一个缓存提供器。
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <returns>分配给该缓存键的提供器。</returns>
        protected ICacheProvider DetermineProvider(string key)
        {
            return _nodeSelector.Determine(key);
        }
    }
}
