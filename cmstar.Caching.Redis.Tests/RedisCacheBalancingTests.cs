using cmstar.Caching.LoadBalancing;
using NUnit.Framework;

namespace cmstar.Caching.Redis
{
    public class RedisCacheBalancingTests
    {
        [TestFixture]
        public class MixedCacheBalancerTests : CacheProviderTestBase
        {
            public MixedCacheBalancerTests()
            {
                // 这里混合不同的缓存提供器组成负载均衡。
                var cache1 = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration());
                var cache2 = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration2());
                var cache3 = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration3());
                var cache4 = new MemoryCacheProvider(nameof(RedisCacheBalancingTests));

                var cacheBalancer = new CacheBalancer();
                cacheBalancer.AddNode(cache1);
                cacheBalancer.AddNode(cache2);
                cacheBalancer.AddNode(cache3);
                cacheBalancer.AddNode(cache4);

                CacheProvider = cacheBalancer;
            }

            protected override ICacheProvider CacheProvider { get; }
        }
    }

    [TestFixture]
    public class RedisCacheBalancerBasicTests : CacheProviderTestBase
    {
        public RedisCacheBalancerBasicTests()
        {
            var cache1 = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration());
            var cache2 = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration2());
            var cacheBalancer = new CacheBalancer();
            cacheBalancer.AddNode(cache1);
            cacheBalancer.AddNode(cache2);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheProvider CacheProvider { get; }
    }

    [TestFixture]
    public class RedisCacheIncreasableBalancerTests : CacheIncreasableTestBase
    {
        public RedisCacheIncreasableBalancerTests()
        {
            var cache1 = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration());
            var cache2 = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration2());
            var cacheBalancer = new CacheIncreasableBalancer();
            cacheBalancer.AddNode(cache1);
            cacheBalancer.AddNode(cache2);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheIncreasable CacheProvider { get; }
    }

    [TestFixture]
    public class RedisHashCacheFieldAccessableBalancerTests : CacheFieldAccessableTestBase
    {
        public RedisHashCacheFieldAccessableBalancerTests()
        {
            var cache1 = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration());
            var cache2 = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration2());
            var cacheBalancer = new CacheFieldAccessableBalancer();
            cacheBalancer.AddNode(cache1);
            cacheBalancer.AddNode(cache2);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheFieldAccessable CacheProvider { get; }
    }

    [TestFixture]
    public class RedisHashCacheFieldIncreasableBalancerTests : CacheFieldIncreasableTestBase
    {
        public RedisHashCacheFieldIncreasableBalancerTests()
        {
            var cache1 = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration());
            var cache2 = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration2());
            var cacheBalancer = new CacheFieldIncreasableBalancer();
            cacheBalancer.AddNode(cache1);
            cacheBalancer.AddNode(cache2);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheFieldIncreasable CacheProvider { get; }
    }
}
