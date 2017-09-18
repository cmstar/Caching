using System;
using System.Threading.Tasks;

namespace cmstar.Caching.LoadBalancing
{
    /// <summary>
    /// 派生自<see cref="CacheFieldAccessableBalancer"/>，并增加<see cref="ICacheFieldAccessable"/>的相关方法。
    /// </summary>
    public class CacheFieldIncreasableBalancer : CacheFieldAccessableBalancer, ICacheFieldIncreasable
    {
        /// <inheritdoc />
        public TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            var provider = DetermineFieldIncreasableProvider(key);
            return provider.FieldIncrease<T, TField>(key, field, increment);
        }

        /// <inheritdoc />
        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            var provider = DetermineFieldIncreasableProvider(key);
            return provider.FieldIncreaseOrCreate<T, TField>(key, field, increment, expiration);
        }

#if !NET35
        /// <inheritdoc />
        public Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            var provider = DetermineFieldIncreasableProvider(key);
            return provider.FieldIncreaseAsync<T, TField>(key, field, increment);
        }

        /// <inheritdoc />
        public Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            var provider = DetermineFieldIncreasableProvider(key);
            return provider.FieldIncreaseOrCreateAsync<T, TField>(key, field, increment, expiration);
        }
#endif

        /// <summary>
        /// 添加一个用于负载均衡的缓存提供器。
        /// </summary>
        /// <param name="provider">添加的缓存提供器，其必须实现<see cref="ICacheFieldIncreasable"/>。</param>
        /// <param name="weight">
        /// 指定该节点所占的比重。必须是正数。节点被使用的概率 = 节点比重 / （所有节点的总比重）。
        /// </param>
        public override void AddNode(ICacheProvider provider, int weight = 1)
        {
            if (!(provider is ICacheFieldIncreasable))
                throw new ArgumentException("The cache provider must implement ICacheFieldIncreasable.", nameof(provider));

            base.AddNode(provider, weight);
        }

        private ICacheFieldIncreasable DetermineFieldIncreasableProvider(string key)
        {
            var provider = DetermineProvider(key);
            return (ICacheFieldIncreasable)provider;
        }
    }
}
