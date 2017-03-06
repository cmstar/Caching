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

        [SetUp]
        [TearDown]
        public void SetUp()
        {
            Console.WriteLine("Cleanup...");
            CacheProvider.Remove(Key);
            AssertCacheNotExist();
        }

        [Test]
        public void TestOnInt64()
        {
            Console.WriteLine("Test INCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.Increase<long>(Key, 1));
            Assert.AreEqual(0, CacheProvider.Increase<long>(Key, 100));
            AssertCacheNotExist();

            Console.WriteLine("Test expiration control for INCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.IncreaseOrCreate<long>(Key, 1, ExpiryShort));
            AssertCacheValue(1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Console.WriteLine("Test INCREASEORCREATE...");
            Func<long, long> increaseOrCreate = x => CacheProvider.IncreaseOrCreate(Key, x, ExpiryLong);
            ActionAndAssert<long>(increaseOrCreate, 1, 1);
            ActionAndAssert<long>(increaseOrCreate, -5, -4);
            ActionAndAssert<long>(increaseOrCreate, 10004, 10000);
            ActionAndAssert<long>(increaseOrCreate, -10000, 0);

            Console.WriteLine("Test INCREASE...");
            Func<long, long> increase = x => CacheProvider.Increase(Key, x);
            ActionAndAssert<long>(increase, 1, 1);
            ActionAndAssert<long>(increase, 1, 2);
            ActionAndAssert<long>(increase, 12356, 12358);
            ActionAndAssert<long>(increase, -5, 12353);
            ActionAndAssert<long>(increase, -33333, -20980);
            ActionAndAssert<long>(increase, 0, -20980);
        }

        [Test]
        public void TestOnInt32()
        {
            Console.WriteLine("Test INCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.Increase<int>(Key, 1));
            Assert.AreEqual(0, CacheProvider.Increase<int>(Key, 100));
            AssertCacheNotExist();

            Console.WriteLine("Test expiration control for INCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.IncreaseOrCreate<int>(Key, 1, ExpiryShort));
            AssertCacheValue(1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Console.WriteLine("Test INCREASEORCREATE...");
            Func<int, int> increaseOrCreate = x => CacheProvider.IncreaseOrCreate(Key, x, ExpiryLong);
            ActionAndAssert<int>(increaseOrCreate, 1, 1);
            ActionAndAssert<int>(increaseOrCreate, -5, -4);
            ActionAndAssert<int>(increaseOrCreate, 10004, 10000);
            ActionAndAssert<int>(increaseOrCreate, -10000, 0);

            Console.WriteLine("Test INCREASE...");
            Func<int, int> increase = x => CacheProvider.Increase(Key, x);
            ActionAndAssert<int>(increase, 1, 1);
            ActionAndAssert<int>(increase, 1, 2);
            ActionAndAssert<int>(increase, 12356, 12358);
            ActionAndAssert<int>(increase, -5, 12353);
            ActionAndAssert<int>(increase, -33333, -20980);
            ActionAndAssert<int>(increase, 0, -20980);
        }

        [Test]
        public void TestOnInt16()
        {
            Console.WriteLine("Test INCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.Increase<short>(Key, 1));
            Assert.AreEqual(0, CacheProvider.Increase<short>(Key, 100));
            AssertCacheNotExist();

            Console.WriteLine("Test expiration control for INCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.IncreaseOrCreate<short>(Key, 1, ExpiryShort));
            AssertCacheValue(1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Console.WriteLine("Test INCREASEORCREATE...");
            Func<short, short> increaseOrCreate = x => CacheProvider.IncreaseOrCreate(Key, x, ExpiryLong);
            ActionAndAssert<short>(increaseOrCreate, 1, 1);
            ActionAndAssert<short>(increaseOrCreate, -5, -4);
            ActionAndAssert<short>(increaseOrCreate, 10004, 10000);
            ActionAndAssert<short>(increaseOrCreate, -10000, 0);

            Console.WriteLine("Test INCREASE...");
            Func<short, short> increase = x => CacheProvider.Increase(Key, x);
            ActionAndAssert<short>(increase, 1, 1);
            ActionAndAssert<short>(increase, 1, 2);
            ActionAndAssert<short>(increase, 12356, 12358);
            ActionAndAssert<short>(increase, -5, 12353);
            ActionAndAssert<short>(increase, -12353, 0);
            ActionAndAssert<short>(increase, 0, 0);
        }

        [Test]
        public void TestOnByte()
        {
            Console.WriteLine("Test INCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.Increase<byte>(Key, 1));
            Assert.AreEqual(0, CacheProvider.Increase<byte>(Key, 100));
            AssertCacheNotExist();

            Console.WriteLine("Test expiration control for INCREASEORCREATE...");
            CacheProvider.IncreaseOrCreate<byte>(Key, 1, ExpiryShort);
            AssertCacheValue(1);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Console.WriteLine("Test INCREASEORCREATE...");
            Func<byte, byte> increaseOrCreate = x => CacheProvider.IncreaseOrCreate(Key, x, ExpiryLong);
            ActionAndAssert<byte>(increaseOrCreate, 1, 1);
            ActionAndAssert<byte>(increaseOrCreate, 3, 4);

            Console.WriteLine("Test INCREASE...");
            Func<byte, byte> increase = x => CacheProvider.Increase(Key, x);
            ActionAndAssert<byte>(increase, 1, 5);
            ActionAndAssert<byte>(increase, 1, 6);
            ActionAndAssert<byte>(increase, 33, 39);
            ActionAndAssert<byte>(increase, 0, 39);
        }

        private void AssertCacheNotExist()
        {
            Assert.IsFalse(CacheProvider.Remove(Key));
        }

        private void AssertCacheValue(long expected)
        {
            var cachedValue = CacheProvider.Get<long>(Key);
            Assert.AreEqual(expected, cachedValue);
        }

        private void ActionAndAssert<T>(Func<T, T> funcIncr, T increment, T expected)
        {
            var res = funcIncr(increment);
            Assert.AreEqual(expected, res);

            res = CacheProvider.Get<T>(Key);
            Assert.AreEqual(expected, res);
        }
    }
}