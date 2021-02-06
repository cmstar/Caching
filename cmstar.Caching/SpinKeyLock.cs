using System;
using System.Threading;

namespace cmstar.Caching
{
    /// <summary>
    /// 提供一组方法，允许获取或释放基于特定关键字的锁。
    /// 此实现具有较高性能，但有一定几率发生关键字碰撞（不同的关键字使用相同的锁）。
    /// 此类型允许锁递归，不要在异步上下文中使用。
    /// </summary>
    /// <typeparam name="TKey">用于锁定的关键字的类型。</typeparam>
    internal class SpinKeyLock<TKey>
    {
        /*
         * 最初此类型使用 SpinWait 实现，但在业务场景中长时间的锁定 CPU 开销较大。
         * 之后改用 Thread.Sleep(1) ，其在 Windows 上开销极小，但在 Linux 系统导致
         * 频繁的用户态和内核态切换， CPU 占用很高，遂改用信号量 AutoResetEvent 实现。
         * 目前的版本已经和 SpinWait 没有关系了，只是类名还保留着。
         */

        /// <summary>
        /// 默认的碰撞区间大小。
        /// </summary>
        public const int DefaultBulkSize = 193;

        /// <summary>
        /// 所允许的最大碰撞区间大小。
        /// </summary>
        public const int MaxBulkSize = 524287;

        private readonly SpinKeyLockHelper _helper = new SpinKeyLockHelper();
        private readonly int _bulkSize;

        // 封装一组 EventWaitHandler ，但信号为 true 表示当前槽位没有被占用；为 false 表示已被占用。
        private readonly SpinKeyLockHolder[] _bulks;

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
        /// <param name="bulkSize">数值越大则发生关键字碰撞的几率越小，但占用内存越大。</param>
        public SpinKeyLock(int bulkSize)
        {
            if (bulkSize <= 0 || bulkSize > MaxBulkSize)
                throw new ArgumentOutOfRangeException(
                    nameof(bulkSize), $"The bulk size should be positive and lesser than {MaxBulkSize}.");
            
            _bulkSize = bulkSize;
            _bulks = new SpinKeyLockHolder[bulkSize];
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
        public virtual IDisposable Enter(TKey key)
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
        public virtual IDisposable TryEnter(TKey key)
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
        public virtual IDisposable TryEnter(TKey key, int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

            return PerformTryEnter(key, millisecondsTimeout);
        }

        private IDisposable PerformTryEnter(TKey key, int millisecondsTimeout)
        {
            var index = GetIndex(key);
            var contextMark = _helper.GetCurrentContextMark();
            GetLockHolder(index, contextMark, out var holder, out var shouldWait);

            if (!shouldWait)
                return new LockUnit<TKey>(this, index);

            if (!holder.WaitHandle.WaitOne(millisecondsTimeout))
                return null;

            holder.Level++;
            holder.ContextMark = contextMark;
            return new LockUnit<TKey>(this, index);
        }

        // holder： 输出当前槽位上的 LockHolder 对象。
        // shouldWait：若为 true ，调用者需等待信号量；否则说明已经获取到锁，可以直接返回了。
        private void GetLockHolder(int keyIndex, int contextMark, out SpinKeyLockHolder holder, out bool shouldWait)
        {
            holder = _bulks[keyIndex];

            var isHolderCreator = false;

            if (holder == null)
            {
                holder = new SpinKeyLockHolder
                {
                    WaitHandle = new AutoResetEvent(false),
                    ContextMark = contextMark,
                    Level = 1
                };

                var oldHolder = Interlocked.CompareExchange(ref _bulks[keyIndex], holder, null);
                if (oldHolder == null)
                {
                    isHolderCreator = true;
                }
                else
                {
                    holder = oldHolder;
                }
            }

            if (isHolderCreator)
            {
                shouldWait = false;
            }
            else if (holder.ContextMark == contextMark)
            {
                // 若当前上下文即为锁的持有者，增加递归层级即可。此操只会由当前线程执行，不会并发。
                holder.Level++;
                shouldWait = false;
            }
            else
            {
                shouldWait = true;
            }
        }

        /// <summary>
        /// 释放指定关键字锁对应的锁。
        /// </summary>
        /// <param name="index">关键字对应的索引值。</param>
        private void Exit(int index)
        {
            var contextMark = _helper.GetCurrentContextMark();
            var holder = _bulks[index];

            if (holder.ContextMark != contextMark)
                throw new SynchronizationLockException("The current context does not own the lock.");

            if (holder.Level <= 0)
                throw new SynchronizationLockException("The lock had been released.");

            holder.Level--;

            // 仍大于 0 说明只是递归层级下降；等于 0 时则是最后一层，可以释放锁了。
            if (holder.Level == 0)
            {
                // ContextMark 清零必须在 WaitHandle.Set() 之前。
                // 一旦 WaitHandle.Set() ，别的线程就可能会获取锁并设置 ContextMark 。
                holder.ContextMark = 0;
                holder.WaitHandle.Set();
            }
        }

        private int GetIndex(TKey key)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var index = (key.GetHashCode() & int.MaxValue) % _bulkSize;
            return index;
        }

        private class LockUnit<T> : IDisposable
        {
            private readonly SpinKeyLock<T> _locker;
            private readonly int _index;

            public LockUnit(SpinKeyLock<T> locker, int index)
            {
                _locker = locker;
                _index = index;
            }

            public void Dispose()
            {
                _locker.Exit(_index);
            }
        }
    }

    internal class SpinKeyLockHolder
    {
        public AutoResetEvent WaitHandle = new AutoResetEvent(false);
        public int ContextMark;

        // 首次获取锁后为 1 ，递归锁没层级+1 。释放锁后为 0 。
        public short Level;
    }

    /// <summary>
    /// <see cref="SpinKeyLock{TKey}"/>的辅助类。
    /// 为每个上下文分配一个ID，取代 Thread.CurrentThread.ManagedThreadId ，以便在异步上下文中使用。
    /// </summary>
    internal class SpinKeyLockHelper
    {
        // 存储当前上下文的ID， 0 表示未初始化。
        private readonly AsyncLocal<int> _currentContextMark = new AsyncLocal<int>();

        private int _nextContextMark;

        /// <summary>
        /// 获取当前线程的标记。模拟类似 ThreadId 的效果。
        /// 不会返回 0 。 0 可用于表示未初始化的值。
        /// </summary>
        public int GetCurrentContextMark()
        {
            var value = _currentContextMark.Value;
            if (value != 0)
                return value;

            value = Interlocked.Increment(ref _nextContextMark);

            // 跳过 0，其表示未初始化。
            if (value == 0)
            {
                value = Interlocked.Increment(ref _nextContextMark);
            }

            _currentContextMark.Value = value;
            return value;
        }
    }
}
