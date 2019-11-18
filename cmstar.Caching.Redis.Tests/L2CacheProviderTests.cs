using System;
using NUnit.Framework;

namespace cmstar.Caching
{
    [TestFixture]
    public class L2CacheProviderTests
    {
        private static readonly ICacheProvider L1Cache = SimpleObjectCacheProvider.Instance;
        private static readonly ICacheProvider L2Cache = new MemoryCacheProvider("test");

        // L1的超时，需要足够的短，以便验证缓存超时。
        private static readonly CacheExpiration L1Expiry 
            = CacheExpiration.FromTimeSpan(CacheProviderTestBase.ExpiryShort);

        private static readonly ICacheProvider TargetCache = new L2CacheProvider(L2Cache, L1Cache, L1Expiry);

        [TestFixture]
        public class L2CacheProviderTestsBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => TargetCache;
        }

#if !NET35
        [TestFixture]
        public class L2CacheProviderTestsaAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(TargetCache);
        }
#endif
    }
}