using NUnit.Framework;

namespace cmstar.Caching
{
    public class HttpRuntimeCacheProviderTests
    {
        [TestFixture]
        public class HttpRuntimeCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => HttpRuntimeCacheProvider.Instance;
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider => HttpRuntimeCacheProvider.Instance;
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider => HttpRuntimeCacheProvider.Instance;
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderFieldIncreasableTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider => HttpRuntimeCacheProvider.Instance;
        }

#if !NET35
        [TestFixture]
        public class HttpRuntimeCacheProvideraAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(HttpRuntimeCacheProvider.Instance);
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderFieldAccessableAsyncTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider { get; }
                = new CacheFieldAccessableAdapter(HttpRuntimeCacheProvider.Instance);
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderIncreasableAsyncTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider { get; }
                = new CacheIncreasableAdapter(HttpRuntimeCacheProvider.Instance);
        }

        [TestFixture]
        public class HttpRuntimeCacheProviderFieldIncreasableAsyncTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider { get; }
                = new CacheFieldIncreasableAdapter(HttpRuntimeCacheProvider.Instance);
        }
#endif
    }
}
