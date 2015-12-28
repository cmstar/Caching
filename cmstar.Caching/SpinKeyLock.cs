using System;
using System.Threading;

namespace cmstar.Caching
{
    /// <summary>
    /// 提供一组方法，允许获取或释放基于特定关键字的锁。
    /// 此实现具有高性能，但有一定几率发生关键字碰撞（不同的关键字使用相同的锁）。
    /// </summary>
    /// <typeparam name="TKey">用于锁定的关键字的类型。</typeparam>
    public class SpinKeyLock<TKey> 
    {
        /// <summary>
        /// 默认的碰撞区间大小。
        /// </summary>
        public const int DefaultBulkSize = 193;

        private readonly int _bulkSize;
        private readonly long[] _bulks;

        /// <summary>
        /// 使用默认的碰撞区间大小（<see cref="DefaultBulkSize"/>）初始化类型的新实例。
        /// </summary>
        public SpinKeyLock()
            : this(DefaultBulkSize)
        {
        }

        /// <summary>
        /// 初始化类型的新实例，并指定一个数，该数值越大则发生关键字碰撞的几率越小，但占用内存越大。
        /// 每单位需要占用8字节内存。建议该数字为一个素数。
        /// </summary>
        /// <param name="lockerSize">数值越大则发生关键字碰撞的几率越小，但占用内存越大。</param>
        public SpinKeyLock(int lockerSize)
        {
            if (lockerSize <= 0)
                throw new ArgumentOutOfRangeException(
                    "lockerSize", "lockerSize must be greater than zero.");

            _bulkSize = lockerSize;
            _bulks = new long[lockerSize];
        }

        /// <summary>
        /// 获取指定关键字的锁，并返回一个<see cref="IDisposable"/>的实现，
        /// 其<see cref="IDisposable.Dispose"/>方法可释放锁。
        /// </summary>
        /// <param name="key">关键字。</param>
        /// <returns>
        /// <see cref="IDisposable"/>的实现，其<see cref="IDisposable.Dispose"/>方法可释放锁，
        /// 方法调用需在拥有该锁的线程上进行。
        /// </returns>
        public IDisposable Enter(TKey key)
        {
            return PerformTryEnter(key, Timeout.Infinite);
        }

        /// <summary>
        /// 尝试获取指定关键字的锁。
        /// 若成功获取锁，返回一个<see cref="IDisposable"/>的实现，其<see cref="IDisposable.Dispose"/>方法可释放锁；
        /// 否则返回null。
        /// </summary>
        /// <param name="key">关键字。</param>
        /// <returns>
        /// 若成功获取锁，返回一个<see cref="IDisposable"/>的实现，其<see cref="IDisposable.Dispose"/>方法可释放锁，
        /// 方法调用需在拥有该锁的线程上进行；未能获取锁时返回null。
        /// </returns>
        public IDisposable TryEnter(TKey key)
        {
            return PerformTryEnter(key, 0);
        }

        /// <summary>
        /// 尝试在指定的时间内获取指定关键字的锁。
        /// 若成功获取锁，返回一个<see cref="IDisposable"/>的实现，其<see cref="IDisposable.Dispose"/>方法可释放锁；
        /// 否则返回null。
        /// </summary>
        /// <param name="key">关键字。</param>
        /// <param name="millisecondsTimeout">超时时间，单位为毫秒。</param>
        /// <returns>
        /// 若成功获取锁，返回一个<see cref="IDisposable"/>的实现，其<see cref="IDisposable.Dispose"/>方法可释放锁，
        /// 方法调用需在拥有该锁的线程上进行；未能获取锁时返回null。
        /// </returns>
        public IDisposable TryEnter(TKey key, int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            return PerformTryEnter(key, millisecondsTimeout);
        }

        private IDisposable PerformTryEnter(TKey key, int millisecondsTimeout)
        {
            var index = GetIndex(key);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var keeper = BuildLockKeeper(threadId, 0);

            long origKeeper;

            // try acquiring the lock with a CAS operation
            if ((origKeeper = Interlocked.CompareExchange(ref _bulks[index], keeper, 0)) == 0)
                return new LockUnit<TKey>(this, key);

            // CAS failed, check if the current thread owns the lock, if yes increase the recursive level
            if (threadId == GetThreadIdFromKeeper(origKeeper))
            {
                _bulks[index]++;
                return new LockUnit<TKey>(this, key);
            }

            // the thread own does not own the lock, acquire the lock with CAS operations in a loop
            if (millisecondsTimeout > 0)
            {
                var waited = 0;

                while (Interlocked.CompareExchange(ref _bulks[index], keeper, 0) != 0)
                {
                    Thread.Sleep(1);

                    if (++waited == millisecondsTimeout) // timeout
                        return null;
                }
            }
            else
            {
                while (Interlocked.CompareExchange(ref _bulks[index], keeper, 0) != 0)
                {
                    if (millisecondsTimeout == 0)
                        return null;

                    Thread.Sleep(1);
                }
            }

            return new LockUnit<TKey>(this, key);
        }

        /// <summary>
        /// 释放指定关键字锁对应的锁。
        /// </summary>
        /// <param name="key">关键字。</param>
        public void Exit(TKey key)
        {
            var index = GetIndex(key);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var keeper = BuildLockKeeper(threadId, 0);

            long origKeeper;
            if ((origKeeper = Interlocked.CompareExchange(ref _bulks[index], 0, keeper)) == keeper)
                return;

            if (threadId != GetThreadIdFromKeeper(origKeeper))
                throw new SynchronizationLockException("The current thread does not own the lock.");

            // if there's a recursive lock, decrease the recursive level
            _bulks[index]--;
        }

        private int GetIndex(TKey key)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (key == null)
                throw new ArgumentNullException("key");

            var index = (key.GetHashCode() & int.MaxValue) % _bulkSize;
            return index;
        }

        private static long BuildLockKeeper(long threadId, long recursiveLevel)
        {
            // a keeper (which is a 64bit number) has two parts - a 32bit header and a 32bit tail,
            // ther former is threadId and the latter is recursiveLevel
            var keeper = (threadId << 32) + recursiveLevel;
            return keeper;
        }

        private static int GetThreadIdFromKeeper(long keeper)
        {
            var threadId = keeper >> 32;
            return (int)threadId;
        }

        private class LockUnit<T> : IDisposable
        {
            private readonly SpinKeyLock<T> _locker;
            private readonly T _key;

            public LockUnit(SpinKeyLock<T> locker, T key)
            {
                _locker = locker;
                _key = key;
            }

            public void Dispose()
            {
                _locker.Exit(_key);
            }
        }
    }
}
