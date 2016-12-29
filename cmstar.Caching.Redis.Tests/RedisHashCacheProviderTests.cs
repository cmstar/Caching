using NUnit.Framework;

namespace cmstar.Caching.Redis
{
    public class RedisHashCacheProviderTests
    {
        private static readonly RedisHashCacheProvider Cache
            = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration());

        [TestFixture]
        public class RedisHashCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return Cache; }
            }
        }

        [TestFixture]
        public class RedisHashCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider
            {
                get { return Cache; }
            }
        }
    }
}
