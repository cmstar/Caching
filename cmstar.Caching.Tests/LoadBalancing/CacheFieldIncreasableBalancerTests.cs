using NUnit.Framework;

namespace cmstar.Caching.LoadBalancing
{
    [TestFixture]
    public class CacheFieldIncreasableBalancerTests : CacheFieldIncreasableTestBase
    {
        public CacheFieldIncreasableBalancerTests()
        {
            var cacheBalancer = new CacheFieldIncreasableBalancer();
            cacheBalancer.AddNode(new MemoryCacheProvider("name"));
            cacheBalancer.AddNode(HttpRuntimeCacheProvider.Instance);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheFieldIncreasable CacheProvider { get; }

        [Test]
        public void TestBalancing()
        {
            var apiCounter = new BalancerApiCounter(new CacheFieldIncreasableBalancer(), new[] { 5, 3 });

            apiCounter.Test("FieldIncrease", x => x.FieldIncreaseCount,
                (provider, key) => ((ICacheFieldIncreasable)provider).FieldIncrease<CacheValueClass, int>(key, "IntField", 1));

            apiCounter.Test("FieldIncreaseOrCreate", x => x.FieldIncreaseOrCreateCount,
                (provider, key) =>
                {
                    ((ICacheFieldIncreasable)provider).FieldIncreaseOrCreate<CacheValueClass, int>(
                        key, "IntField", 1, CacheProviderTestBase.ExpiryShort);
                });
        }
    }
}