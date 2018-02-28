using NUnit.Framework;

namespace cmstar.Caching
{
    public class SimpleObjectCacheProviderTests
    {
        [TestFixture]
        public class SimpleObjectCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => SimpleObjectCacheProvider.Instance;
        }

        [TestFixture]
        public class SimpleObjectCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider => SimpleObjectCacheProvider.Instance;
        }

        [TestFixture]
        public class SimpleObjectCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider => SimpleObjectCacheProvider.Instance;
        }

        [TestFixture]
        public class SimpleObjectCacheProviderFieldIncreasableTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider => SimpleObjectCacheProvider.Instance;
        }

#if !NET35
        [TestFixture]
        public class SimpleObjectCacheProvideraAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(SimpleObjectCacheProvider.Instance);
        }

        [TestFixture]
        public class SimpleObjectCacheProviderFieldAccessableAsyncTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider { get; }
                = new CacheFieldAccessableAdapter(SimpleObjectCacheProvider.Instance);
        }

        [TestFixture]
        public class SimpleObjectCacheProviderIncreasableAsyncTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider { get; }
                = new CacheIncreasableAdapter(SimpleObjectCacheProvider.Instance);
        }

        [TestFixture]
        public class SimpleObjectCacheProviderFieldIncreasableAsyncTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider { get; }
                = new CacheFieldIncreasableAdapter(SimpleObjectCacheProvider.Instance);
        }
#endif
    }
}
