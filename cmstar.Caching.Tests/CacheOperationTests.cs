using System;
using System.Threading;
using NUnit.Framework;

namespace cmstar.Caching
{
    [TestFixture]
    public class CacheOperationTests
    {
        [Test]
        public void TestIncreasableCache()
        {
            Action<ICacheIncreasable> test = cache =>
            {
                var keyRoot = Guid.NewGuid().ToString();
                var expiry = CacheExpiration.FromSeconds(5);
                var cacheOp = new CacheOperation<int>("ns", keyRoot, cache, expiry);
                var keyOp = cacheOp.Key();

                Console.WriteLine("Test GET on non-existing key...");
                var value = keyOp.Get();
                Assert.AreEqual(default(int), value);

                Console.WriteLine("Test TRYGET on non-existing key...");
                Assert.IsFalse(keyOp.TryGet(out value));

                Console.WriteLine("Test SET on non-existing key...");
                keyOp.Set(1);

                Console.WriteLine("Test GET on existing key...");
                value = keyOp.Get();
                Assert.AreEqual(1, value);

                Console.WriteLine("Test TRYGET on existing key...");
                Assert.IsTrue(keyOp.TryGet(out value));
                Assert.AreEqual(1, value);

                Console.WriteLine("Test REMOVE...");
                Assert.IsTrue(keyOp.Remove());
                Assert.IsFalse(keyOp.Remove());

                Console.WriteLine("Test INCREASE/INCREASEORCREATE...");
                Assert.AreEqual(0, keyOp.Increase(1)); // key does not exist
                Assert.AreEqual(4, keyOp.IncreaseOrCreate(4)); // key created
                Assert.AreEqual(-96, keyOp.Increase(-100));
            };

            Console.WriteLine("Testing synchronized cache...");
            test(HttpRuntimeCacheProvider.Instance);

#if !NET35
            Console.WriteLine("Testing asynchronized cache...");
            test(new CacheIncreasableAdapter(HttpRuntimeCacheProvider.Instance));
#endif
        }

        [Test]
        public void TestFieldIncreasableCache()
        {
            Action<ICacheFieldIncreasable> test = cache =>
            {
                var keyRoot = Guid.NewGuid().ToString();
                var expiry = CacheExpiration.FromSeconds(5);
                var cacheOp = new CacheOperation<CacheValueClass>("ns", keyRoot, cache, expiry);
                var keyOp = cacheOp.Key();

                Console.WriteLine("Test FIELDINCREASE on non-existing key...");
                Assert.AreEqual(0, keyOp.FieldIncrease(x => x.LongField, 1));
                Assert.IsNull(keyOp.Get());

                Console.WriteLine("Test FIELDINCREASE on non-existing field...");
                keyOp.Create(new CacheValueClass());
                Assert.AreEqual(0, keyOp.FieldIncrease("nx", 1));
                Assert.IsTrue(keyOp.Remove());
                Assert.IsNull(keyOp.Get());

                Console.WriteLine("Test FIELDINCREASEORCREATE...");
                Assert.AreEqual(1, keyOp.FieldIncreaseOrCreate(x => x.LongField, 1));

                var instance = keyOp.Get();
                Assert.IsNotNull(instance);
                Assert.AreEqual(1, instance.LongField);

                Console.WriteLine("Test FIELDINCREASE on existing key...");
                Assert.AreEqual(2, keyOp.FieldIncrease(x => x.LongField, 1));

                instance = keyOp.Get();
                Assert.IsNotNull(instance);
                Assert.AreEqual(2, instance.LongField);
            };

            Console.WriteLine("Testing synchronized cache...");
            test(HttpRuntimeCacheProvider.Instance);

#if !NET35
            Console.WriteLine("Testing asynchronized cache...");
            test(new CacheFieldIncreasableAdapter(HttpRuntimeCacheProvider.Instance));
#endif
        }

        [Test]
        public void TestObserverOnIncreasableCache()
        {
            Action<ICacheIncreasable> test = cache =>
            {
                var ob = new InternalObserver();
                var keyRoot = Guid.NewGuid().ToString().Substring(0, 8);
                var cacheOp = new CacheOperation<int>("ns", keyRoot, cache, CacheExpiration.FromSeconds(1), ob);

                // miss
                DoTestObserver(cacheOp, x => x.Get(), 1, KeyOpType.Miss);
                DoTestObserver(cacheOp, x =>
                {
                    int _;
                    x.TryGet(out _);
                }, 2, KeyOpType.Miss);

                // set the value
                DoTestObserver(cacheOp, x => x.Set(1), 4);
                DoTestObserver(cacheOp, x => x.Create(1), 5);

                // then, hit
                DoTestObserver(cacheOp, x => x.Get(), 1, KeyOpType.Hit);
                DoTestObserver(cacheOp, x =>
                {
                    int _;
                    x.TryGet(out _);
                }, 2, KeyOpType.Hit);

                // remove the existing key
                DoTestObserver(cacheOp, x => x.Remove(), 1, KeyOpType.Remove);

                // other methods
                DoTestObserver(cacheOp, x => x.Remove(), 6);
                DoTestObserver(cacheOp, x => x.Increase(1), 7);
                DoTestObserver(cacheOp, x => x.IncreaseOrCreate(1), 8);
            };

            Console.WriteLine("Testing synchronized cache...");
            test(HttpRuntimeCacheProvider.Instance);

#if !NET35
            Console.WriteLine("Testing asynchronized cache...");
            test(new CacheIncreasableAdapter(HttpRuntimeCacheProvider.Instance));
#endif
        }

        [Test]
        public void TestObserverOnFieldIncreasableCache()
        {
            Action<ICacheFieldIncreasable> test = cache =>
            {
                var ob = new InternalObserver();
                var keyRoot = Guid.NewGuid().ToString().Substring(0, 8);
                var expiry = CacheExpiration.FromSeconds(1);
                var cacheOp = new CacheOperation<CacheValueClass>("ns", keyRoot, cache, expiry, ob);

                // miss
                DoTestObserver(cacheOp, x => x.FieldGet(v => v.IntField), 1, KeyOpType.Miss);
                DoTestObserver(cacheOp, x =>
                {
                    int _;
                    x.FieldTryGet(v => v.IntField, out _);
                }, 2, KeyOpType.Miss);

                // set the value
                DoTestObserver(cacheOp, x => x.FieldSet(v => v.IntField, 1), 4);
                DoTestObserver(cacheOp, x => x.FieldSetOrCreate(v => v.IntField, 1), 5);

                // then, hit
                DoTestObserver(cacheOp, x => x.FieldGet(v => v.IntField), 1, KeyOpType.Hit);
                DoTestObserver(cacheOp, x =>
                {
                    int _;
                    x.FieldTryGet(v => v.IntField, out _);
                }, 2, KeyOpType.Hit);

                // other methods
                DoTestObserver(cacheOp, x => x.FieldIncrease(v => v.IntField, 1), 6);
                DoTestObserver(cacheOp, x => x.FieldIncreaseOrCreate(v => v.IntField, 1), 7);
            };

            Console.WriteLine("Testing synchronized cache...");
            test(HttpRuntimeCacheProvider.Instance);

#if !NET35
            Console.WriteLine("Testing asynchronized cache...");
            test(new CacheFieldIncreasableAdapter(HttpRuntimeCacheProvider.Instance));
#endif
        }

        [Test]
        public void TestObserverOnRemoving()
        {
            Action<ICacheFieldIncreasable> test = cache =>
            {
                var ob = new InternalObserver();
                var keyRoot = Guid.NewGuid().ToString().Substring(0, 8);
                var expiry = CacheExpiration.FromSeconds(1);
                var cacheOp = new CacheOperation<int>("ns", keyRoot, cache, expiry, ob);
                var keyOp = cacheOp.Key();

                keyOp.Remove();
                Assert.AreEqual(1, ob.Accesses);
                Assert.AreEqual(0, ob.Removes);

                var accesses = ob.Accesses;
                var removes = ob.Removes;
                for (int i = 0; i < 10; i++)
                {
                    keyOp.Set(1);
                    Assert.AreEqual(++accesses, ob.Accesses);

                    keyOp.Remove(); // hit
                    Assert.AreEqual(accesses, ob.Accesses); // not increased
                    Assert.AreEqual(++removes, ob.Removes);

                    keyOp.Remove(); // miss
                    Assert.AreEqual(++accesses, ob.Accesses);
                    Assert.AreEqual(removes, ob.Removes); // not increased
                }
            };

            Console.WriteLine("Testing synchronized cache...");
            test(HttpRuntimeCacheProvider.Instance);

#if !NET35
            Console.WriteLine("Testing asynchronized cache...");
            test(new CacheFieldIncreasableAdapter(HttpRuntimeCacheProvider.Instance));
#endif
        }

        private static void DoTestObserver<T>(
            CacheOperation<T> cacheOp, Action<KeyOperation<T>> act, int times, KeyOpType type = KeyOpType.Access)
        {
            var ob = (InternalObserver)cacheOp.Observer;
            var expectedAccesses = ob.Accesses + times;
            var expectedHits = ob.Hits + times;
            var expectedMisses = ob.Misses + times;
            var expectedRemoves = ob.Removes + times;

            var keyOp = cacheOp.Key();
            for (int i = 0; i < times; i++)
            {
                act(keyOp);
            }

            switch (type)
            {
                case KeyOpType.Access:
                    Assert.AreEqual(expectedAccesses, ob.Accesses, $"unexpeced Accesses, times: {times}");
                    break;

                case KeyOpType.Hit:
                    Assert.AreEqual(expectedHits, ob.Hits, $"unexpeced Hits, times: {times}");
                    break;

                case KeyOpType.Miss:
                    Assert.AreEqual(expectedMisses, ob.Misses, $"unexpeced Misses, times: {times}");
                    break;

                case KeyOpType.Remove:
                    Assert.AreEqual(expectedRemoves, ob.Removes, $"unexpeced Removes, times: {times}");
                    break;
            }
        }

        private enum KeyOpType
        {
            Access,
            Hit,
            Miss,
            Remove
        }

        private class InternalObserver : ICacheOperationObserver
        {
            public int Accesses;
            public int Hits;
            public int Misses;
            public int Removes;

            public void Accessed(string keyBase)
            {
                Interlocked.Increment(ref Accesses);
            }

            public void Hit(string keyBase)
            {
                Interlocked.Increment(ref Hits);
            }

            public void Missed(string keyBase)
            {
                Interlocked.Increment(ref Misses);
            }

            public void Removed(string keyBase)
            {
                Interlocked.Increment(ref Removes);
            }
        }
    }
}