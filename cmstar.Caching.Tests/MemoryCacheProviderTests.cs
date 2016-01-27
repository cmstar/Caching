using System;

namespace cmstar.Caching
{
    public class MemoryCacheProviderTests
    {
        private static readonly MemoryCacheProvider CacheInstance = new MemoryCacheProvider(Environment.MachineName);

        public class MemoryCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return CacheInstance; }
            }
        }

        public class MemoryCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider
            {
                get { return CacheInstance; }
            }
        }
    }
}
