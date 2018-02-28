using NUnit.Framework;

namespace cmstar.Caching.LoadBalancing
{
    [TestFixture]
    public class CacheFieldAccessableBalancerTests : CacheFieldAccessableTestBase
    {
        public CacheFieldAccessableBalancerTests()
        {
            var cacheBalancer = new CacheFieldAccessableBalancer();
            cacheBalancer.AddNode(new MemoryCacheProvider("name"));
            cacheBalancer.AddNode(SimpleObjectCacheProvider.Instance);

            CacheProvider = cacheBalancer;
        }

        protected override ICacheFieldAccessable CacheProvider { get; }

        [Test]
        public void TestBalancing()
        {
            var apiCounter = new BalancerApiCounter(new CacheFieldAccessableBalancer(), new[] { 1, 6 });

            apiCounter.Test("FieldGet", x => x.FieldGetCount,
                (provider, key) => ((ICacheFieldAccessable)provider).FieldGet<CacheValueClass, int>(key, "IntField"));

            apiCounter.Test("FieldTryGet", x => x.FieldTryGetCount,
                (provider, key) =>
                {
                    int _;
                    ((ICacheFieldAccessable)provider).FieldTryGet<CacheValueClass, int>(key, "IntField", out _);
                });

            apiCounter.Test("FieldSet", x => x.FieldSetCount,
                (provider, key) => ((ICacheFieldAccessable)provider).FieldSet<CacheValueClass, int>(key, "InitField", 0));

            apiCounter.Test("FieldSetOrCreate", x => x.FieldSetCount,
                (provider, key) =>
                {
                    ((ICacheFieldAccessable)provider).FieldSetOrCreate<CacheValueClass, int>(
                        key, "InitField", 0, CacheProviderTestBase.ExpiryShort);
                });
        }
    }
}