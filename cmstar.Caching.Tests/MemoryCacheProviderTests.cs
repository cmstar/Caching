﻿using System;
using NUnit.Framework;

namespace cmstar.Caching
{
    public class MemoryCacheProviderTests
    {
        private static readonly MemoryCacheProvider CacheInstance = new MemoryCacheProvider(Environment.MachineName);

        [TestFixture]
        public class MemoryCacheProviderBasicTests : CacheProviderTestBase
        {
            protected override ICacheProvider CacheProvider
            {
                get { return CacheInstance; }
            }
        }

        [TestFixture]
        public class MemoryCacheProviderFieldAccessableTests : CacheFieldAccessableTestBase
        {
            protected override ICacheFieldAccessable CacheProvider
            {
                get { return CacheInstance; }
            }
        }

        [TestFixture]
        public class MemoryCacheProviderIncreasableTests : CacheIncreasableTestBase
        {
            protected override ICacheIncreasable CacheProvider
            {
                get { return CacheInstance; }
            }
        }

        [TestFixture]
        public class MemoryCacheProviderFieldIncreasableTests : CacheFieldIncreasableTestBase
        {
            protected override ICacheFieldIncreasable CacheProvider
            {
                get { return CacheInstance; }
            }
        }
    }
}
