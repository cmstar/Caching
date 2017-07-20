using NUnit.Framework;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    public class RedisCacheProviderTests
    {
        private static readonly RedisCacheProvider Cache
            = new RedisCacheProvider(RedisTestEnv.GetRedisServerConfiguration());

        [TestFixture]
        public class RedisCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => Cache;

            // 验证二进制数据确实是以二进制方式存储在redis上的，没有被错误的以JSON或其他方式序列化。
            [Test]
            public void TestBinaryLength()
            {
                var data = new byte[] { 1, 2, 3, 4, 5 };
                CacheProvider.Set(Key, data, ExpiryLong);

                var mul = ConnectionMultiplexer.Connect(RedisTestEnv.GetRedisServerConfiguration());
                var db = mul.GetDatabase();
                var cacheValue = db.StringGet(Key);
                var binary = (byte[])cacheValue;
                Assert.NotNull(binary);
                Assert.AreEqual(data.Length, binary.Length);
            }
        }

        [TestFixture]
        public class RedisCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider => Cache;
        }
    }
}
