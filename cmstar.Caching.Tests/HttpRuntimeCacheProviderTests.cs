namespace cmstar.Caching
{
    public class HttpRuntimeCacheProviderTests
    {
        public class HttpRuntimeCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return HttpRuntimeCacheProvider.Instance; }
            }
        }

        public class HttpRuntimeCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider
            {
                get { return HttpRuntimeCacheProvider.Instance; }
            }
        }
    }
}
