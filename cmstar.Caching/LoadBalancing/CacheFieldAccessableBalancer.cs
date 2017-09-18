using System;
using System.Threading.Tasks;

namespace cmstar.Caching.LoadBalancing
{
    /// <summary>
    /// 派生自<see cref="CacheBalancer"/>，并增加<see cref="ICacheFieldAccessable"/>的相关方法。
    /// </summary>
    public class CacheFieldAccessableBalancer : CacheBalancer, ICacheFieldAccessable
    {
        /// <inheritdoc />
        public TField FieldGet<T, TField>(string key, string field)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldGet<T, TField>(key, field);
        }

        /// <inheritdoc />
        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldTryGet<T, TField>(key, field, out value);
        }

        /// <inheritdoc />
        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldSet<T, TField>(key, field, value);
        }

        /// <inheritdoc />
        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldSetOrCreate<T, TField>(key, field, value, expiration);
        }

#if !NET35
        /// <inheritdoc />
        public Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldGetAsync<T, TField>(key, field);
        }

        /// <inheritdoc />
        public Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldTryGetAsync<T, TField>(key, field);
        }

        /// <inheritdoc />
        public Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldSetAsync<T, TField>(key, field, value);
        }

        /// <inheritdoc />
        public Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var provider = DetermineFieldAccessableProvider(key);
            return provider.FieldSetOrCreateAsync<T, TField>(key, field, value, expiration);
        }
#endif

        /// <summary>
        /// 添加一个用于负载均衡的缓存提供器。
        /// </summary>
        /// <param name="provider">添加的缓存提供器，其必须实现<see cref="ICacheFieldAccessable"/>。</param>
        /// <param name="weight">
        /// 指定该节点所占的比重。必须是正数。节点被使用的概率 = 节点比重 / （所有节点的总比重）。
        /// </param>
        public override void AddNode(ICacheProvider provider, int weight = 1)
        {
            if (!(provider is ICacheFieldAccessable))
                throw new ArgumentException("The cache provider must implement ICacheFieldAccessable.", nameof(provider));

            base.AddNode(provider, weight);
        }

        private ICacheFieldAccessable DetermineFieldAccessableProvider(string key)
        {
            var provider = DetermineProvider(key);
            return (ICacheFieldAccessable)provider;
        }
    }
}
