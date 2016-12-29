using System;
using System.Threading;
using NUnit.Framework;

namespace cmstar.Caching
{
    [TestFixture]
    public abstract class CacheFieldAccessableTestBase
    {
        protected readonly TimeSpan TestCacheKeyExpiry = TimeSpan.FromMinutes(5);

        protected abstract ICacheFieldAccessable CacheProvider { get; }

        private string GetTempKey()
        {
            var key = "TEST_" + Guid.NewGuid().ToString("N");
            return key;
        }

        [Test]
        public void TestFieldGetOnNonExistingKey()
        {
            var key = GetTempKey();
            var i = CacheProvider.FieldGet<CacheValueClass, int>(key, "IntField");
            Assert.AreEqual(0, i);

            var s = CacheProvider.FieldGet<CacheValueClass, string>(key, "StringField");
            Assert.AreEqual(null, s);

            var st = CacheProvider.FieldGet<CacheValueClass, CacheValueStruct>(key, "StructField");
            Assert.AreEqual(new CacheValueStruct(), st);
        }

        [Test]
        public void TestFieldGetOnNonExistingField()
        {
            Console.WriteLine("Testing for classes...");
            var key = GetTempKey();
            var c = CacheValueClass.CloneSample();
            PrepareValue(key, c);

            var st = CacheProvider.FieldGet<CacheValueClass, CacheValueStruct>(key, "nx");
            Assert.AreEqual(new CacheValueStruct(), st);

            Console.WriteLine("Testing for structs...");
            st = CacheValueStruct.CloneSample();
            PrepareValue(key, st);

            var f = CacheProvider.FieldGet<CacheValueStruct, float>(key, "nx");
            Assert.AreEqual(0F, f);

            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldTryGetOnNonExistingKey()
        {
            var key = GetTempKey();

            int i;
            Assert.IsFalse(CacheProvider.FieldTryGet<CacheValueClass, int>(key, "IntField", out i));

            string s;
            Assert.IsFalse(CacheProvider.FieldTryGet<CacheValueClass, string>(key, "StringField", out s));

            CacheValueStruct st;
            Assert.IsFalse(CacheProvider.FieldTryGet<CacheValueClass, CacheValueStruct>(key, "StructField", out st));
        }

        [Test]
        public void TestFieldTryGetOnNonExistingField()
        {
            Console.WriteLine("Testing for classes...");
            var key = GetTempKey();
            var c = CacheValueClass.CloneSample();
            PrepareValue(key, c);

            string s;
            Assert.IsFalse(CacheProvider.FieldTryGet<CacheValueClass, string>(key, "nx", out s));

            Console.WriteLine("Testing for structs...");
            var st = CacheValueStruct.CloneSample();
            PrepareValue(key, st);

            var f = CacheProvider.FieldGet<CacheValueStruct, float>(key, "nx");
            Assert.AreEqual(0F, f);

            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetOnNonExistingKey()
        {
            var key = GetTempKey();
            Assert.IsFalse(CacheProvider.FieldSet<CacheValueClass, int>(key, "IntField", 123));
        }

        [Test]
        public void TestFieldGetForClass()
        {
            var key = GetTempKey();
            var c = CacheValueClass.CloneSample();
            PrepareValue(key, c);

            PerformTestFieldGetForClass(key, c);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldGetForStruct()
        {
            var key = GetTempKey();
            var st = CacheValueStruct.CloneSample();
            PrepareValue(key, st);

            PerformTestFieldGetForStruct(key, st);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldTryGetForClass()
        {
            var key = GetTempKey();
            var c = CacheValueClass.CloneSample();
            PrepareValue(key, c);

            PerformTestFieldTryGetForClass(key, c);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldTryGetForStruct()
        {
            var key = GetTempKey();
            var st = CacheValueStruct.CloneSample();
            PrepareValue(key, st);

            PerformTestFieldTryGetForStruct(key, st);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetForClass()
        {
            var key = GetTempKey();
            var expected = CacheValueClass.CloneSample();
            PrepareValue(key, expected);

            expected.IntField = 33;
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueClass, int>(key, "IntField", expected.IntField));

            expected.StringField = "gga";
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueClass, string>(key, "StringField", expected.StringField));

            expected.DateTimeField = DateTime.Now;
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueClass, DateTime>(key, "DateTimeField", expected.DateTimeField));

            expected.StructField = new CacheValueStruct { D = 345M, S = "abcc" };
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueClass, CacheValueStruct>(key, "StructField", expected.StructField));

            var val = CacheProvider.Get<CacheValueClass>(key);
            Assert.AreEqual(expected, val);

            PerformTestFieldGetForClass(key, expected);
            PerformTestFieldTryGetForClass(key, expected);

            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetForStruct()
        {
            var key = GetTempKey();
            var expected = CacheValueStruct.CloneSample();
            PrepareValue(key, expected);

            expected.D = 88731.134M;
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueStruct, decimal>(key, "D", expected.D));

            expected.S = "gga";
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueStruct, string>(key, "S", expected.S));

            expected.G = Guid.NewGuid();
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueStruct, Guid>(key, "G", expected.G));

            expected.F = -3.141F;
            Assert.IsTrue(CacheProvider.FieldSet<CacheValueStruct, float>(key, "F", expected.F));

            var val = CacheProvider.Get<CacheValueStruct>(key);
            Assert.AreEqual(expected, val);

            PerformTestFieldGetForStruct(key, expected);
            PerformTestFieldTryGetForStruct(key, expected);

            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetCxForClassOnNonExistingKey()
        {
            var key = GetTempKey();
            var expiry = TimeSpan.FromMinutes(5);

            Action<CacheValueClass> assert = e =>
            {
                var val = CacheProvider.Get<CacheValueClass>(key);
                Assert.AreEqual(e, val);
            };

            var expected = new CacheValueClass { IntField = -9866 };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, int>(
                key, "IntField", expected.IntField, expiry));
            assert(expected);
            CacheProvider.Remove(key);

            expected = new CacheValueClass { StringField = "jhfiw中文？" };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, string>(
                key, "StringField", expected.StringField, expiry));
            assert(expected);
            CacheProvider.Remove(key);

            expected = new CacheValueClass { StructField = CacheValueStruct.CloneSample() };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, CacheValueStruct>(
                key, "StructField", expected.StructField, expiry));
            assert(expected);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetCxForClassOnExistingKey()
        {
            var key = GetTempKey();
            var expiry = TimeSpan.FromMinutes(5);
            var expected = CacheValueClass.CloneSample();
            PrepareValue(key, expected);

            Action<CacheValueClass> assert = e =>
            {
                var val = CacheProvider.Get<CacheValueClass>(key);
                Assert.AreEqual(e, val);
            };

            expected.IntField = -9866;
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, int>(
                key, "IntField", expected.IntField, expiry));
            assert(expected);

            expected.StringField = "jhfiw中文？";
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, string>(
                key, "StringField", expected.StringField, expiry));
            assert(expected);

            expected.StructField = CacheValueStruct.CloneSample();
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, CacheValueStruct>(
                key, "StructField", expected.StructField, expiry));
            assert(expected);

            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetCxForStructOnNonExistingKey()
        {
            var key = GetTempKey();
            var expiry = TimeSpan.FromMinutes(5);

            Action<CacheValueStruct> assert = e =>
            {
                var val = CacheProvider.Get<CacheValueStruct>(key);
                Assert.AreEqual(e, val);
            };

            var expected = new CacheValueStruct { D = 0.99M };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, decimal>(key, "D", expected.D, expiry));
            assert(expected);
            CacheProvider.Remove(key);

            expected = new CacheValueStruct { G = Guid.NewGuid() };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, Guid>(key, "G", expected.G, expiry));
            assert(expected);
            CacheProvider.Remove(key);

            expected = new CacheValueStruct { S = "wfef日本語" };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, string>(key, "S", expected.S, expiry));
            assert(expected);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestFieldSetCxForStructOnExistingKey()
        {
            var key = GetTempKey();
            var expiry = TimeSpan.FromMinutes(5);
            var expected = CacheValueStruct.CloneSample();
            PrepareValue(key, expected);

            Action<CacheValueStruct> assert = e =>
            {
                var val = CacheProvider.Get<CacheValueStruct>(key);
                Assert.AreEqual(e, val);
            };

            expected.D = 0.99M;
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, decimal>(key, "D", expected.D, expiry));
            assert(expected);

            expected.G = Guid.NewGuid();
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, Guid>(key, "G", expected.G, expiry));
            assert(expected);

            expected.S = "wfef日本語";
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, string>(key, "S", expected.S, expiry));
            assert(expected);

            CacheProvider.Remove(key);
        }

        [Test]
        public void TestExpirationOnFieldSetCxForClass()
        {
            var key = GetTempKey();
            var expiry = TimeSpan.FromSeconds(1);

            var expected = new CacheValueClass { IntField = 996 };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueClass, int>(key, "IntField", expected.IntField, expiry));
            var val = CacheProvider.Get<CacheValueClass>(key);
            Assert.AreEqual(expected, val);

            Thread.Sleep(1100);
            val = CacheProvider.Get<CacheValueClass>(key);
            Assert.IsNull(val);
            CacheProvider.Remove(key);
        }

        [Test]
        public void TestExpirationOnFieldSetCxForStruct()
        {
            var key = GetTempKey();
            var expiry = TimeSpan.FromSeconds(1);

            var expected = new CacheValueStruct { G = Guid.NewGuid() };
            Assert.IsTrue(CacheProvider.FieldSetCx<CacheValueStruct, Guid>(key, "G", expected.G, expiry));

            var val = CacheProvider.Get<CacheValueStruct>(key);
            Assert.AreEqual(expected, val);

            Thread.Sleep(1100);
            val = CacheProvider.Get<CacheValueStruct>(key);
            Assert.AreEqual(new CacheValueStruct(), val);
            CacheProvider.Remove(key);
        }

        private void PrepareValue(string key, object value)
        {
            Console.WriteLine("Remove the old value...");
            CacheProvider.Remove(key);

            Console.WriteLine("Set value...");
            CacheProvider.Set(key, value, TestCacheKeyExpiry);
        }

        private void PerformTestFieldGetForClass(string key, CacheValueClass c)
        {
            var i = CacheProvider.FieldGet<CacheValueClass, int>(key, "IntField");
            Assert.AreEqual(c.IntField, i);

            var s = CacheProvider.FieldGet<CacheValueClass, string>(key, "StringField");
            Assert.AreEqual(c.StringField, s);

            var st = CacheProvider.FieldGet<CacheValueClass, CacheValueStruct>(key, "StructField");
            Assert.AreEqual(c.StructField, st);
        }

        private void PerformTestFieldGetForStruct(string key, CacheValueStruct st)
        {
            var f = CacheProvider.FieldGet<CacheValueStruct, float>(key, "F");
            Assert.AreEqual(st.F, f);

            var s = CacheProvider.FieldGet<CacheValueStruct, string>(key, "S");
            Assert.AreEqual(st.S, s);

            var g = CacheProvider.FieldGet<CacheValueStruct, Guid>(key, "G");
            Assert.AreEqual(st.G, g);
        }

        private void PerformTestFieldTryGetForClass(string key, CacheValueClass c)
        {
            int i;
            Assert.IsTrue(CacheProvider.FieldTryGet<CacheValueClass, int>(key, "IntField", out i));
            Assert.AreEqual(c.IntField, i);

            string s;
            Assert.IsTrue(CacheProvider.FieldTryGet<CacheValueClass, string>(key, "StringField", out s));
            Assert.AreEqual(c.StringField, s);

            CacheValueStruct st;
            Assert.IsTrue(CacheProvider.FieldTryGet<CacheValueClass, CacheValueStruct>(key, "StructField", out st));
            Assert.AreEqual(c.StructField, st);
        }

        private void PerformTestFieldTryGetForStruct(string key, CacheValueStruct st)
        {
            float f;
            Assert.IsTrue(CacheProvider.FieldTryGet<CacheValueStruct, float>(key, "F", out f));
            Assert.AreEqual(st.F, f);

            string s;
            Assert.IsTrue(CacheProvider.FieldTryGet<CacheValueStruct, string>(key, "S", out s));
            Assert.AreEqual(st.S, s);

            Guid g;
            Assert.IsTrue(CacheProvider.FieldTryGet<CacheValueStruct, Guid>(key, "G", out g));
            Assert.AreEqual(st.G, g);
        }
    }
}
