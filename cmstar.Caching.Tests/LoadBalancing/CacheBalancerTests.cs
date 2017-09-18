using NUnit.Framework;

namespace cmstar.Caching.LoadBalancing
{
    [TestFixture]
    public class CacheBalancerTests : CacheProviderTestBase
    {
        public CacheBalancerTests()
        {
            var cacheBalancer = new CacheBalancer();
            cacheBalancer.AddNode(new MemoryCacheProvider("name"));
            cacheBalancer.AddNode(HttpRuntimeCacheProvider.Instance);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheProvider CacheProvider { get; }

        [Test]
        public void TestBalancingOf2()
        {
            var apiCounter = new BalancerApiCounter(new CacheBalancer(), new[] { 1, 1 });
            DoTest(apiCounter);
        }

        [Test]
        public void TestBalancingOf4()
        {
            var apiCounter = new BalancerApiCounter(new CacheBalancer(), new[] { 4, 3, 2, 1 });
            DoTest(apiCounter);
        }

        private static void DoTest(BalancerApiCounter apiCounter)
        {
            apiCounter.Test("Set", x => x.SetCount,
                (provider, key) => provider.Set(key, 0, ExpiryShort));

            apiCounter.Test("Create", x => x.CreateCount,
                (provider, key) => provider.Create(key, 0, ExpiryShort));

            apiCounter.Test("Get", x => x.GetCount,
                (provider, key) => provider.Get<int>(key));

            apiCounter.Test("TryGet", x => x.TryGetCount,
                (provider, key) =>
                {
                    int _;
                    provider.TryGet(key, out _);
                });

            apiCounter.Test("Remove", x => x.RemoveCount,
                (provider, key) => provider.Remove(key));
        }
    }
}