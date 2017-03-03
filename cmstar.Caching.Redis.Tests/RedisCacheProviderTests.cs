using NUnit.Framework;

namespace cmstar.Caching.Redis
{
    public class RedisCacheProviderTests
    {
        private static readonly RedisCacheProvider Cache
            = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration());

        [TestFixture]
        public class RedisCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return Cache; }
            }
        }

        [TestFixture]
        public class RedisCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider
            {
                get { return Cache; }
            }
        }
    }
}
