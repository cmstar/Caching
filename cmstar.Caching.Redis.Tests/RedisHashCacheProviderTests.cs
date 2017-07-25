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
            protected override ICacheProvider CacheProvider => Cache;
        }

        [TestFixture]
        public class RedisHashCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider => Cache;
        }

        [TestFixture]
        public class RedisHashCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider => Cache;
        }

        [TestFixture]
        public class RedisHashCacheProviderFieldIncreasableTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider => Cache;
        }

        [TestFixture]
        public class RedisHashCacheProviderAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(Cache);
        }

        [TestFixture]
        public class RedisHashCacheProviderFieldAccessableAsyncTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider { get; }
                = new CacheFieldAccessableAdapter(Cache);
        }

        [TestFixture]
        public class RedisHashCacheProviderIncreasableAsyncTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider { get; }
                = new CacheIncreasableAdapter(Cache);
        }

        [TestFixture]
        public class RedisHashCacheProviderFieldIncreasableAsyncTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider { get; }
                = new CacheFieldIncreasableAdapter(Cache);
        }
    }
}
