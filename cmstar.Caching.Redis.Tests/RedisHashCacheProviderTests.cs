namespace cmstar.Caching.Redis
{
    public class RedisHashCacheProviderTests
    {
        private static readonly RedisHashCacheProvider Cache
            = new RedisHashCacheProvider(RedisTestEnv.GetRedisServerConfiguration());

        public class RedisHashCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return Cache; }
            }
        }

        public class RedisHashCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider
            {
                get { return Cache; }
            }
        }
    }
}
