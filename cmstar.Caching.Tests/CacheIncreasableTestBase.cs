using System;
using System.Threading;
using NUnit.Framework;

namespace cmstar.Caching
{
    [TestFixture]
    public abstract class CacheIncreasableTestBase
    {
        // ReSharper disable RedundantTypeArgumentsOfMethod
        private const string Key = "TEST_12740E08F86E42A4A8B0638E653DCF3A";
        private static readonly TimeSpan ExpiryShort = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan ExpiryLong = TimeSpan.FromMinutes(5);

        protected abstract ICacheIncreasable CacheProvider { get; }

        [Test]
        public void TestOnInt64()
        {
            Console.WriteLine("Remove the old key...");
            CacheProvider.Remove(Key);

            Console.WriteLine("Test INCREASE on not-existing key...");
            CacheProvider.Increase<long>(Key, 1);
            CacheProvider.Increase<long>(Key, 100);
            NotExist(Key);

            Console.WriteLine("Test expiration control for INCREASECX...");
            CacheProvider.IncreaseCx<long>(Key, 1, ExpiryShort);
            EnsureCacheValue(Key, 1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            NotExist(Key);

            Console.WriteLine("Test INCREASECX...");
            CacheProvider.IncreaseCx<long>(Key, 1, ExpiryLong);
            EnsureCacheValue(Key, 1);

            CacheProvider.IncreaseCx<long>(Key, -5, ExpiryLong);
            EnsureCacheValue(Key, -4);

            CacheProvider.IncreaseCx<long>(Key, 10004, ExpiryLong);
            EnsureCacheValue(Key, 10000);

            CacheProvider.IncreaseCx<long>(Key, -10000, ExpiryLong);
            EnsureCacheValue(Key, 0);

            Console.WriteLine("Test INCREASE...");
            CacheProvider.Increase<long>(Key, 1);
            EnsureCacheValue(Key, 1);

            CacheProvider.Increase<long>(Key, 1);
            EnsureCacheValue(Key, 2);

            CacheProvider.Increase<long>(Key, 12356);
            EnsureCacheValue(Key, 12358);

            CacheProvider.Increase<long>(Key, -5);
            EnsureCacheValue(Key, 12353);

            CacheProvider.Increase<long>(Key, -33333);
            EnsureCacheValue(Key, -20980);

            CacheProvider.Increase<long>(Key, 0);
            EnsureCacheValue(Key, -20980);

            Console.WriteLine("Cleanup...");
            CacheProvider.Remove(Key);
            NotExist(Key);
        }

        [Test]
        public void TestOnInt32()
        {
            Console.WriteLine("Remove the old key...");
            CacheProvider.Remove(Key);

            Console.WriteLine("Test INCREASE on not-existing key...");
            CacheProvider.Increase<int>(Key, 1);
            CacheProvider.Increase<int>(Key, 100);
            NotExist(Key);

            Console.WriteLine("Test expiration control for INCREASECX...");
            CacheProvider.IncreaseCx<int>(Key, 1, ExpiryShort);
            EnsureCacheValue(Key, 1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            NotExist(Key);

            Console.WriteLine("Test INCREASECX...");
            CacheProvider.IncreaseCx<int>(Key, 1, ExpiryLong);
            EnsureCacheValue(Key, 1);

            CacheProvider.IncreaseCx<int>(Key, -5, ExpiryLong);
            EnsureCacheValue(Key, -4);

            CacheProvider.IncreaseCx<int>(Key, 10004, ExpiryLong);
            EnsureCacheValue(Key, 10000);

            CacheProvider.IncreaseCx<int>(Key, -10000, ExpiryLong);
            EnsureCacheValue(Key, 0);

            Console.WriteLine("Test INCREASE...");
            CacheProvider.Increase<int>(Key, 1);
            EnsureCacheValue(Key, 1);

            CacheProvider.Increase<int>(Key, 1);
            EnsureCacheValue(Key, 2);

            CacheProvider.Increase<int>(Key, 12356);
            EnsureCacheValue(Key, 12358);

            CacheProvider.Increase<int>(Key, -5);
            EnsureCacheValue(Key, 12353);

            CacheProvider.Increase<int>(Key, -33333);
            EnsureCacheValue(Key, -20980);

            CacheProvider.Increase<int>(Key, 0);
            EnsureCacheValue(Key, -20980);

            Console.WriteLine("Cleanup...");
            CacheProvider.Remove(Key);
            NotExist(Key);
        }

        [Test]
        public void TestOnInt16()
        {
            Console.WriteLine("Remove the old key...");
            CacheProvider.Remove(Key);

            Console.WriteLine("Test INCREASE on not-existing key...");
            CacheProvider.Increase<short>(Key, 1);
            CacheProvider.Increase<short>(Key, 100);
            NotExist(Key);

            Console.WriteLine("Test expiration control for INCREASECX...");
            CacheProvider.IncreaseCx<short>(Key, 1, ExpiryShort);
            EnsureCacheValue(Key, 1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            NotExist(Key);

            Console.WriteLine("Test INCREASECX...");
            CacheProvider.IncreaseCx<short>(Key, 1, ExpiryLong);
            EnsureCacheValue(Key, 1);

            CacheProvider.IncreaseCx<short>(Key, -5, ExpiryLong);
            EnsureCacheValue(Key, -4);

            CacheProvider.IncreaseCx<short>(Key, 10004, ExpiryLong);
            EnsureCacheValue(Key, 10000);

            CacheProvider.IncreaseCx<short>(Key, -10000, ExpiryLong);
            EnsureCacheValue(Key, 0);

            Console.WriteLine("Test INCREASE...");
            CacheProvider.Increase<short>(Key, 1);
            EnsureCacheValue(Key, 1);

            CacheProvider.Increase<short>(Key, 1);
            EnsureCacheValue(Key, 2);

            CacheProvider.Increase<short>(Key, 12356);
            EnsureCacheValue(Key, 12358);

            CacheProvider.Increase<short>(Key, -5);
            EnsureCacheValue(Key, 12353);

            CacheProvider.Increase<short>(Key, -12353);
            EnsureCacheValue(Key, 0);

            CacheProvider.Increase<short>(Key, 0);
            EnsureCacheValue(Key, 0);

            Console.WriteLine("Cleanup...");
            CacheProvider.Remove(Key);
            NotExist(Key);
        }

        [Test]
        public void TestOnByte()
        {
            Console.WriteLine("Remove the old key...");
            CacheProvider.Remove(Key);

            Console.WriteLine("Test INCREASE on not-existing key...");
            CacheProvider.Increase<byte>(Key, 1);
            CacheProvider.Increase<byte>(Key, 100);
            NotExist(Key);

            Console.WriteLine("Test expiration control for INCREASECX...");
            CacheProvider.IncreaseCx<byte>(Key, 1, ExpiryShort);
            EnsureCacheValue(Key, 1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            NotExist(Key);

            Console.WriteLine("Test INCREASECX...");
            CacheProvider.IncreaseCx<byte>(Key, 1, ExpiryLong);
            EnsureCacheValue(Key, 1);

            CacheProvider.IncreaseCx<byte>(Key, 3, ExpiryLong);
            EnsureCacheValue(Key, 4);

            Console.WriteLine("Test INCREASE...");
            CacheProvider.Increase<byte>(Key, 1);
            EnsureCacheValue(Key, 5);

            CacheProvider.Increase<byte>(Key, 1);
            EnsureCacheValue(Key, 6);

            CacheProvider.Increase<byte>(Key, 33);
            EnsureCacheValue(Key, 39);

            CacheProvider.Increase<byte>(Key, 0);
            EnsureCacheValue(Key, 39);

            Console.WriteLine("Cleanup...");
            CacheProvider.Remove(Key);
            NotExist(Key);
        }

        private void NotExist(string key)
        {
            Assert.IsFalse(CacheProvider.Remove(key));
        }

        private void EnsureCacheValue(string key, long expected)
        {
            var cachedValue = CacheProvider.Get<long>(key);
            Assert.AreEqual(expected, cachedValue);
        }
    }
}