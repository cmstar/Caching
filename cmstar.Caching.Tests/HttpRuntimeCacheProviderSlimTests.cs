using NUnit.Framework;

namespace cmstar.Caching
{
    public class HttpRuntimeCacheProviderSlimTests
    {
        [TestFixture]
        public class HttpRuntimeCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => HttpRuntimeCacheProviderSlim.Instance;
        }

        [TestFixture]
        public class HttpRuntimeCacheProvideraAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(HttpRuntimeCacheProviderSlim.Instance);
        }
    }
}
