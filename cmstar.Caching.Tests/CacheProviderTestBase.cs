using System;
using System.Threading;
using NUnit.Framework;

namespace cmstar.Caching
{
    [TestFixture]
    public abstract class CacheProviderTestBase
    {
        private const string Key = "TEST_12740E08F86E42A4A8B0638E653DCF3A";
        private static readonly TimeSpan ExpiryShort = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan ExpiryLong = TimeSpan.FromMinutes(2);

        protected abstract ICacheProvider CacheProvider { get; }

        [Test]
        public void TestExpirationControl()
        {
            CacheProvider.Set(Key, string.Empty, ExpiryShort);
            Thread.Sleep(ExpiryShort.Add(TimeSpan.FromMilliseconds(50)));
            Assert.IsNull(CacheProvider.Get<string>(Key));
            Assert.IsFalse(CacheProvider.Remove(Key));
        }

        [Test]
        public void TestImplicicConvert()
        {
            CacheProvider.Set(Key, 2.0D, ExpiryLong);
            Assert.AreEqual(2.0D, CacheProvider.Get<double>(Key));
            Assert.AreEqual(2.0F, CacheProvider.Get<float>(Key));
            Assert.AreEqual(2, CacheProvider.Get<short>(Key));
            Assert.AreEqual(2, CacheProvider.Get<int>(Key));
            Assert.AreEqual(2L, CacheProvider.Get<long>(Key));
            Assert.AreEqual(2, CacheProvider.Get<byte>(Key));
        }

        [Test]
        public void TestOnBool()
        {
            Console.WriteLine("Test true...");
            PerformTestCacheProvider(true);

            Console.WriteLine("Test false...");
            PerformTestCacheProvider(false);
        }

        [Test]
        public void TestOnChar()
        {
            PerformTestCacheProvider('a');
            PerformTestCacheProvider('\0');
            PerformTestCacheProvider('\t');
            PerformTestCacheProvider(' ');
            PerformTestCacheProvider(char.MaxValue);
            PerformTestCacheProvider(char.MinValue);
        }

        [Test]
        public void TestOnString()
        {
            Console.WriteLine("Test ASCII...");
            PerformTestCacheProvider("this is a string");

            Console.WriteLine("Test complex chars...");
            PerformTestCacheProvider("中文`日本語");
        }

        [Test]
        public void TestOnByte()
        {
            PerformTestCacheProvider((byte)1);
            PerformTestCacheProvider((byte)33);
            PerformTestCacheProvider(byte.MaxValue);
            PerformTestCacheProvider(byte.MinValue);
        }

        [Test]
        public void TestOnSByte()
        {
            PerformTestCacheProvider((sbyte)0);
            PerformTestCacheProvider((sbyte)33);
            PerformTestCacheProvider((sbyte)-1);
            PerformTestCacheProvider((sbyte)-58);
            PerformTestCacheProvider(sbyte.MaxValue);
            PerformTestCacheProvider(sbyte.MinValue);
        }

        [Test]
        public void TestOnInt16()
        {
            PerformTestCacheProvider((short)0);
            PerformTestCacheProvider((short)1);
            PerformTestCacheProvider((short)-1);
            PerformTestCacheProvider((short)7765);
            PerformTestCacheProvider((short)-11565);
            PerformTestCacheProvider(short.MaxValue);
            PerformTestCacheProvider(short.MinValue);
        }

        [Test]
        public void TestOnUInt16()
        {
            PerformTestCacheProvider((ushort)0);
            PerformTestCacheProvider((ushort)1);
            PerformTestCacheProvider((ushort)15880);
            PerformTestCacheProvider(ushort.MinValue);
            PerformTestCacheProvider(ushort.MinValue);
        }

        [Test]
        public void TestOnInt32()
        {
            PerformTestCacheProvider(0);
            PerformTestCacheProvider(1);
            PerformTestCacheProvider(123);
            PerformTestCacheProvider(-1);
            PerformTestCacheProvider(-99999);
            PerformTestCacheProvider(int.MinValue);
            PerformTestCacheProvider(int.MinValue);
        }

        [Test]
        public void TestOnUInt32()
        {
            PerformTestCacheProvider((uint)0);
            PerformTestCacheProvider((uint)1);
            PerformTestCacheProvider((uint)8899);
            PerformTestCacheProvider((uint)123456789);
            PerformTestCacheProvider(uint.MinValue);
            PerformTestCacheProvider(uint.MinValue);
        }

        [Test]
        public void TestOnInt64()
        {
            PerformTestCacheProvider(0L);
            PerformTestCacheProvider(1L);
            PerformTestCacheProvider(-1L);
            PerformTestCacheProvider(15419158948L);
            PerformTestCacheProvider(-558936L);
            PerformTestCacheProvider(long.MaxValue);
            PerformTestCacheProvider(long.MinValue);
        }

        [Test]
        public void TestOnUInt64()
        {
            PerformTestCacheProvider((ulong)0);
            PerformTestCacheProvider((ulong)1);
            PerformTestCacheProvider((ulong)15419158948);
            PerformTestCacheProvider(ulong.MaxValue);
            PerformTestCacheProvider(ulong.MinValue);
        }

        [Test]
        public void TestOnSingle()
        {
            PerformTestCacheProvider(0F);
            PerformTestCacheProvider(-123.456F);
            PerformTestCacheProvider(-0.001F);
            PerformTestCacheProvider(55F);
            PerformTestCacheProvider(float.MaxValue);
            PerformTestCacheProvider(float.MinValue);
            PerformTestCacheProvider(float.Epsilon);
        }

        [Test]
        public void TestOnDouble()
        {
            PerformTestCacheProvider(0D);
            PerformTestCacheProvider(-123.456D);
            PerformTestCacheProvider(-0.001D);
            PerformTestCacheProvider(55D);
            PerformTestCacheProvider(1.25e12);
            PerformTestCacheProvider(double.MaxValue);
            PerformTestCacheProvider(double.MinValue);
            PerformTestCacheProvider(double.Epsilon);
        }

        [Test]
        public void TestOnGuid()
        {
            PerformTestCacheProvider(Guid.NewGuid());
        }

        [Test]
        public void TestOnDateTime()
        {
            PerformTestCacheProvider(DateTime.Now);
        }

        [Test]
        public void TestOnDateTimeOffset()
        {
            PerformTestCacheProvider(DateTimeOffset.Now);
        }

        [Test]
        public void TestOnDecimal()
        {
            PerformTestCacheProvider(0M);
            PerformTestCacheProvider(99M);
            PerformTestCacheProvider(-1M);
            PerformTestCacheProvider(15419158.113948M);
            PerformTestCacheProvider(decimal.MinValue);
            PerformTestCacheProvider(decimal.MaxValue);
        }

        [Test]
        public void TestOnNull()
        {
            PerformTestCacheProvider((CacheValueClass)null);
        }

        [Test]
        public void TestOnEmptyStruct()
        {
            PerformTestCacheProvider(new CacheValueStruct());
        }

        [Test]
        public void TestOnCustomStruct()
        {
            PerformTestCacheProvider(CacheValueStruct.CloneSample());
        }

        [Test]
        public void TestOnCustomClass()
        {
            PerformTestCacheProvider(CacheValueClass.CloneSample());
        }

        [Test]
        public void TestOnEmptyObject()
        {
            PerformTestCacheProvider(new NoMemberClass());
        }

        private void PerformTestCacheProvider<T>(T valueForTest) where T : IEquatable<T>
        {
            var type = typeof(T);
            var msg = Type.GetTypeCode(type) == TypeCode.Object
                ? string.Format("Perform test on complex type {0} ...", type)
                : string.Format("Perform test on type {0} value '{1}' ...", type, valueForTest);
            Console.WriteLine(msg);

            Console.WriteLine("Remove the old key...");
            CacheProvider.Remove(Key);

            Console.WriteLine("Test GET on non-existing key...");
            var nullValue = default(T);
            var value = CacheProvider.Get<T>(Key);
            Assert.AreEqual(nullValue, value);

            Console.WriteLine("Test TRYGET on non-existing key...");
            Assert.IsFalse(CacheProvider.TryGet(Key, out value));

            Console.WriteLine("Test SET on non-existing key...");
            CacheProvider.Set(Key, valueForTest, ExpiryLong);

            Console.WriteLine("Test GET on existing key...");
            value = CacheProvider.Get<T>(Key);
            Assert.AreEqual(valueForTest, value);

            Console.WriteLine("Test TRYGET on existing key...");
            Assert.IsTrue(CacheProvider.TryGet(Key, out value));
            Assert.AreEqual(valueForTest, value);

            Console.WriteLine("Test REMOVE...");
            Assert.IsTrue(CacheProvider.Remove(Key));
            Assert.IsFalse(CacheProvider.Remove(Key));

            Console.WriteLine("Test SET on existing key...");
            CacheProvider.Set(Key, Guid.NewGuid(), ExpiryLong);
            CacheProvider.Set(Key, valueForTest, ExpiryLong); // replace the old key

            value = CacheProvider.Get<T>(Key);
            Assert.AreEqual(valueForTest, value);
            Assert.IsTrue(CacheProvider.TryGet(Key, out value));
            Assert.AreEqual(valueForTest, value);
        }

        private class NoMemberClass : IEquatable<NoMemberClass>
        {
            public bool Equals(NoMemberClass other)
            {
                return true;
            }
        }
    }
}