namespace cmstar.Caching.Redis
{
    public class RedisCacheProviderTests
    {
        private static readonly RedisCacheProvider Cache
            = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration());

        public class RedisCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return Cache; }
            }
        }
    }
}
