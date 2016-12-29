using NUnit.Framework;

namespace cmstar.Caching
{
    public class HttpRuntimeCacheProviderTests
    {
        [TestFixture]
        public class HttpRuntimeCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return HttpRuntimeCacheProvider.Instance; }
            }
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider
            {
                get { return HttpRuntimeCacheProvider.Instance; }
            }
        }
    }
}
