using NUnit.Framework;

namespace cmstar.Caching.LoadBalancing
{
    [TestFixture]
    public class CacheIncreasableBalancerTests : CacheIncreasableTestBase
    {
        public CacheIncreasableBalancerTests()
        {
            var cacheBalancer = new CacheIncreasableBalancer();
            cacheBalancer.AddNode(new MemoryCacheProvider("name"));
            cacheBalancer.AddNode(SimpleObjectCacheProvider.Instance);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheIncreasable CacheProvider { get; }

        [Test]
        public void TestBalancing()
        {
            var apiCounter = new BalancerApiCounter(new CacheIncreasableBalancer(), new[] { 1, 3, 7, 1, 3 });

            apiCounter.Test("Increase", x => x.IncreaseCount,
                (provider, key) => ((ICacheIncreasable)provider).Increase(key, 1));

            apiCounter.Test("IncreaseOrCreate", x => x.IncreaseOrCreateCount,
                (provider, key) => ((ICacheIncreasable)provider).IncreaseOrCreate(key, 1, CacheProviderTestBase.ExpiryShort));
        }
    }
}