using System;
using System.Threading.Tasks;

namespace cmstar.Caching.LoadBalancing
{
    /// <summary>
    /// 派生自<see cref="CacheBalancer"/>，并增加<see cref="ICacheIncreasable"/>的相关方法。
    /// </summary>
    public class CacheIncreasableBalancer : CacheBalancer, ICacheIncreasable
    {
        /// <inheritdoc />
        public T Increase<T>(string key, T increment)
        {
            var provider = DetermineIncreasableProvider(key);
            return provider.Increase(key, increment);
        }

        /// <inheritdoc />
        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            var provider = DetermineIncreasableProvider(key);
            return provider.IncreaseOrCreate(key, increment, expiration);
        }

#if !NET35
        /// <inheritdoc />
        public Task<T> IncreaseAsync<T>(string key, T increment)
        {
            var provider = DetermineIncreasableProvider(key);
            return provider.IncreaseAsync(key, increment);
        }

        /// <inheritdoc />
        public Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            var provider = DetermineIncreasableProvider(key);
            return provider.IncreaseOrCreateAsync(key, increment, expiration);
        }
#endif

        /// <summary>
        /// 添加一个用于负载均衡的缓存提供器。
        /// </summary>
        /// <param name="provider">添加的缓存提供器，其必须实现<see cref="ICacheIncreasable"/>。</param>
        /// <param name="weight">
        /// 指定该节点所占的比重。必须是正数。节点被使用的概率 = 节点比重 / （所有节点的总比重）。
        /// </param>
        public override void AddNode(ICacheProvider provider, int weight = 1)
        {
            if (!(provider is ICacheIncreasable))
                throw new ArgumentException("The cache provider must implement ICacheIncreasable.", nameof(provider));

            base.AddNode(provider, weight);
        }

        private ICacheIncreasable DetermineIncreasableProvider(string key)
        {
            var provider = DetermineProvider(key);
            return (ICacheIncreasable)provider;
        }
    }
}
