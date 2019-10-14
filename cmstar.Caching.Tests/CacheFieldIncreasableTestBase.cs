using System;
using System.Threading;
using NUnit.Framework;

namespace cmstar.Caching
{
    [TestFixture]
    public abstract class CacheFieldIncreasableTestBase
    {
        private const string Key = "TEST_12740E08F86E42A4A8B0638E653DCF3A";
        private static readonly TimeSpan ExpiryShort = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan ExpiryLong = TimeSpan.FromMinutes(5);

        protected abstract ICacheFieldIncreasable CacheProvider { get; }

        [SetUp]
        [TearDown]
        public void Cleanup()
        {
            Console.WriteLine("Cleanup...");
            CacheProvider.Remove(Key);
            AssertCacheNotExist();
        }

        [Test]
        public void TestOnInt64ForClass()
        {
            Console.WriteLine("Test FIELDINCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, long>(Key, "LongField", 1));
            AssertCacheNotExist();

            Console.WriteLine("Test FIELDINCREASE on non-existing field...");
            CacheProvider.Create(Key, new CacheValueClass(), ExpiryLong);
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, long>(Key, "nx", 1));
            Cleanup();

            Console.WriteLine("Test expiration control for FIELDINCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.FieldIncreaseOrCreate<CacheValueClass, long>(Key, "LongField", 1, ExpiryShort));
            AssertCacheValue<CacheValueClass>(x => Assert.AreEqual(1, x.LongField));
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Func<CacheValueClass, long> fieldSelector = x => x.LongField;

            Console.WriteLine("Test FIELDINCREASEORCREATE...");
            Func<long, long> increaseOrCreate =
                x => CacheProvider.FieldIncreaseOrCreate<CacheValueClass, long>(Key, "LongField", x, ExpiryLong);
            ActionAndAssert(increaseOrCreate, fieldSelector, 1, 1);
            ActionAndAssert(increaseOrCreate, fieldSelector, -5, -4);
            ActionAndAssert(increaseOrCreate, fieldSelector, 10004, 10000);
            ActionAndAssert(increaseOrCreate, fieldSelector, -10000, 0);

            Console.WriteLine("Test FIELDINCREASE...");
            Func<long, long> increase =
                x => CacheProvider.FieldIncrease<CacheValueClass, long>(Key, "LongField", x);
            ActionAndAssert(increase, fieldSelector, 1, 1);
            ActionAndAssert(increase, fieldSelector, 1, 2);
            ActionAndAssert(increase, fieldSelector, 12356, 12358);
            ActionAndAssert(increase, fieldSelector, -5, 12353);
            ActionAndAssert(increase, fieldSelector, -33333, -20980);
            ActionAndAssert(increase, fieldSelector, 0, -20980);
        }

        [Test]
        public void TestOnInt32ForClass()
        {
            Console.WriteLine("Test FIELDINCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, int>(Key, "IntField", 1));
            AssertCacheNotExist();

            Console.WriteLine("Test FIELDINCREASE on non-existing field...");
            CacheProvider.Create(Key, new CacheValueClass(), ExpiryLong);
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, int>(Key, "nx", 1));
            Cleanup();

            Console.WriteLine("Test expiration control for FIELDINCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.FieldIncreaseOrCreate<CacheValueClass, int>(Key, "IntField", 1, ExpiryShort));
            AssertCacheValue<CacheValueClass>(x => Assert.AreEqual(1, x.IntField));
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Func<CacheValueClass, int> fieldSelector = x => x.IntField;

            Console.WriteLine("Test FIELDINCREASEORCREATE...");
            Func<int, int> increaseOrCreate =
                x => CacheProvider.FieldIncreaseOrCreate<CacheValueClass, int>(Key, "IntField", x, ExpiryLong);
            ActionAndAssert(increaseOrCreate, fieldSelector, 1, 1);
            ActionAndAssert(increaseOrCreate, fieldSelector, -5, -4);
            ActionAndAssert(increaseOrCreate, fieldSelector, 10004, 10000);
            ActionAndAssert(increaseOrCreate, fieldSelector, -10000, 0);

            Console.WriteLine("Test FIELDINCREASE...");
            Func<int, int> increase =
                x => CacheProvider.FieldIncrease<CacheValueClass, int>(Key, "IntField", x);
            ActionAndAssert(increase, fieldSelector, 1, 1);
            ActionAndAssert(increase, fieldSelector, 1, 2);
            ActionAndAssert(increase, fieldSelector, 12356, 12358);
            ActionAndAssert(increase, fieldSelector, -5, 12353);
            ActionAndAssert(increase, fieldSelector, -33333, -20980);
            ActionAndAssert(increase, fieldSelector, 0, -20980);
        }

        [Test]
        public void TestOnInt16ForClass()
        {
            Console.WriteLine("Test FIELDINCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, short>(Key, "ShortField", 1));
            AssertCacheNotExist();

            Console.WriteLine("Test FIELDINCREASE on non-existing field...");
            CacheProvider.Create(Key, new CacheValueClass(), ExpiryShort);
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, short>(Key, "nx", 1));
            Cleanup();

            Console.WriteLine("Test expiration control for FIELDINCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.FieldIncreaseOrCreate<CacheValueClass, short>(Key, "ShortField", 1, ExpiryShort));
            AssertCacheValue<CacheValueClass>(x => Assert.AreEqual(1, x.ShortField));
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Func<CacheValueClass, short> fieldSelector = x => x.ShortField;

            Console.WriteLine("Test FIELDINCREASEORCREATE...");
            Func<short, short> increaseOrCreate =
                x => CacheProvider.FieldIncreaseOrCreate<CacheValueClass, short>(Key, "ShortField", x, ExpiryLong);
            ActionAndAssert<CacheValueClass, short>(increaseOrCreate, fieldSelector, 1, 1);
            ActionAndAssert<CacheValueClass, short>(increaseOrCreate, fieldSelector, -5, -4);
            ActionAndAssert<CacheValueClass, short>(increaseOrCreate, fieldSelector, 10004, 10000);
            ActionAndAssert<CacheValueClass, short>(increaseOrCreate, fieldSelector, -10000, 0);

            Console.WriteLine("Test FIELDINCREASE...");
            Func<short, short> increase =
                x => CacheProvider.FieldIncrease<CacheValueClass, short>(Key, "ShortField", x);
            ActionAndAssert<CacheValueClass, short>(increase, fieldSelector, 1, 1);
            ActionAndAssert<CacheValueClass, short>(increase, fieldSelector, 1, 2);
            ActionAndAssert<CacheValueClass, short>(increase, fieldSelector, 12356, 12358);
            ActionAndAssert<CacheValueClass, short>(increase, fieldSelector, -5, 12353);
            ActionAndAssert<CacheValueClass, short>(increase, fieldSelector, -12353, 0);
            ActionAndAssert<CacheValueClass, short>(increase, fieldSelector, 0, 0);
        }

        [Test]
        public void TestOnByteForClass()
        {
            Console.WriteLine("Test FIELDINCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, byte>(Key, "ByteField", 1));
            AssertCacheNotExist();

            Console.WriteLine("Test FIELDINCREASE on non-existing field...");
            CacheProvider.Create(Key, new CacheValueClass(), ExpiryShort);
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueClass, byte>(Key, "nx", 1));
            Cleanup();

            Console.WriteLine("Test expiration control for FIELDINCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.FieldIncreaseOrCreate<CacheValueClass, byte>(Key, "ByteField", 1, ExpiryShort));
            AssertCacheValue<CacheValueClass>(x => Assert.AreEqual(1, x.ByteField));
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Func<CacheValueClass, byte> fieldSelector = x => x.ByteField;

            Console.WriteLine("Test FIELDINCREASEORCREATE...");
            Func<byte, byte> increaseOrCreate =
                x => CacheProvider.FieldIncreaseOrCreate<CacheValueClass, byte>(Key, "ByteField", x, ExpiryLong);
            ActionAndAssert<CacheValueClass, byte>(increaseOrCreate, fieldSelector, 1, 1);
            ActionAndAssert<CacheValueClass, byte>(increaseOrCreate, fieldSelector, 3, 4);

            Console.WriteLine("Test FIELDINCREASE...");
            Func<byte, byte> increase =
                x => CacheProvider.FieldIncrease<CacheValueClass, byte>(Key, "ByteField", x);
            ActionAndAssert<CacheValueClass, byte>(increase, fieldSelector, 1, 5);
            ActionAndAssert<CacheValueClass, byte>(increase, fieldSelector, 1, 6);
            ActionAndAssert<CacheValueClass, byte>(increase, fieldSelector, 33, 39);
            ActionAndAssert<CacheValueClass, byte>(increase, fieldSelector, 0, 39);
        }

        [Test]
        public void TestOnInt32ForStruct()
        {
            Console.WriteLine("Test FIELDINCREASE on non-existing key...");
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueStruct, int>(Key, "N", 1));
            AssertCacheNotExist();

            Console.WriteLine("Test FIELDINCREASE on non-existing field...");
            CacheProvider.Create(Key, new CacheValueClass(), ExpiryLong);
            Assert.AreEqual(0, CacheProvider.FieldIncrease<CacheValueStruct, int>(Key, "nx", 1));
            Cleanup();

            Console.WriteLine("Test expiration control for FIELDINCREASEORCREATE...");
            Assert.AreEqual(1, CacheProvider.FieldIncreaseOrCreate<CacheValueStruct, int>(Key, "N", 1, ExpiryShort));
            AssertCacheValue<CacheValueStruct>(x => Assert.AreEqual(1, x.N));
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            AssertCacheNotExist();

            Func<CacheValueStruct, int> fieldSelector = x => x.N;

            Console.WriteLine("Test FIELDINCREASEORCREATE...");
            Func<int, int> increaseOrCreate =
                x => CacheProvider.FieldIncreaseOrCreate<CacheValueStruct, int>(Key, "N", x, ExpiryLong);
            ActionAndAssert(increaseOrCreate, fieldSelector, 1, 1);
            ActionAndAssert(increaseOrCreate, fieldSelector, -5, -4);
            ActionAndAssert(increaseOrCreate, fieldSelector, 10004, 10000);
            ActionAndAssert(increaseOrCreate, fieldSelector, -10000, 0);

            Console.WriteLine("Test FIELDINCREASE...");
            Func<int, int> increase =
                x => CacheProvider.FieldIncrease<CacheValueStruct, int>(Key, "N", x);
            ActionAndAssert(increase, fieldSelector, 1, 1);
            ActionAndAssert(increase, fieldSelector, 1, 2);
            ActionAndAssert(increase, fieldSelector, 12356, 12358);
            ActionAndAssert(increase, fieldSelector, -5, 12353);
            ActionAndAssert(increase, fieldSelector, -33333, -20980);
            ActionAndAssert(increase, fieldSelector, 0, -20980);
        }

        [Test]
        public void TestOnNull()
        {
            CacheProvider.Create(Key, new CacheValueClass(), ExpiryLong);
            Assert.Throws<InvalidCastException>(() => CacheProvider.FieldIncrease<CacheValueClass, int>(Key, "NullableField", 1));
        }

        private void AssertCacheNotExist()
        {
            // 在一些缓存实现中（比如.net的MemoryCache和HttpRuntimeCache），缓存有两种过期方式，
            // 1 周期性的移除，周期可能比较久，在较短时间内未必会被清理；
            // 2 在访问缓存对象时，检查该对象是否过期，若过期，则立刻清理之。
            // 所以直接调用Remove方法，可能缓存清理周期还没到，虽然缓存过期了，但因为此时缓存对象
            // 还在，仍会返回true（清理成功）。我们先Get一下，确保该清的已经被清了（情况2），再调
            // 用Remove，此时才可以保证其返回false。
            Assert.IsNull(CacheProvider.Get<object>(Key));
            Assert.IsFalse(CacheProvider.Remove(Key));
        }

        private void AssertCacheValue<T>(Action<T> assert)
        {
            var cachedValue = CacheProvider.Get<T>(Key);
            Assert.NotNull(cachedValue);
            assert(cachedValue);
        }

        private void ActionAndAssert<T, TField>(
            Func<TField, TField> funcIncr, Func<T, TField> fieldSelector, TField increment, TField expected)
        {
            var res = funcIncr(increment);
            Assert.AreEqual(expected, res);

            var cachedValue = CacheProvider.Get<T>(Key);
            var cachedField = fieldSelector(cachedValue);
            Assert.AreEqual(expected, cachedField);
        }
    }
}