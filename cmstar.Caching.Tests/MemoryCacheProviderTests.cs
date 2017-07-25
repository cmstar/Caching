using System;
using NUnit.Framework;

namespace cmstar.Caching
{
    public class MemoryCacheProviderTests
    {
        private static readonly MemoryCacheProvider CacheInstance = new MemoryCacheProvider(Environment.MachineName);

        [TestFixture]
        public class MemoryCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => CacheInstance;
        }

        [TestFixture]
        public class MemoryCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider => CacheInstance;
        }

        [TestFixture]
        public class MemoryCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider => CacheInstance;
        }

        [TestFixture]
        public class MemoryCacheProviderFieldIncreasableTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider => CacheInstance;
        }

#if !NET35
        [TestFixture]
        public class MemoryCacheProvideraAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(CacheInstance);
        }

        [TestFixture]
        public class MemoryCacheProviderFieldAccessableAsyncTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider { get; }
                = new CacheFieldAccessableAdapter(CacheInstance);
        }

        [TestFixture]
        public class MemoryCacheProviderIncreasableAsyncTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider { get; }
                = new CacheIncreasableAdapter(CacheInstance);
        }

        [TestFixture]
        public class MemoryCacheProviderFieldIncreasableAsyncTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider { get; }
                = new CacheFieldIncreasableAdapter(CacheInstance);
        }
#endif
    }
}
