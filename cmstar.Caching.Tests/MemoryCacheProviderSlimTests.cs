using System;
using NUnit.Framework;

namespace cmstar.Caching
{
    public class MemoryCacheProviderSlimTests
    {
        private static readonly MemoryCacheProviderSlim CacheInstance = new MemoryCacheProviderSlim(Environment.MachineName);

        [TestFixture]
        public class MemoryCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider => CacheInstance;
        }
        
        [TestFixture]
        public class MemoryCacheProvideraAsyncBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider { get; }
                = new CacheProviderAdapter(CacheInstance);
        }
    }
}
