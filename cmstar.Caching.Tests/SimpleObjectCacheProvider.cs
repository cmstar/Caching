using System;
using System.Collections.Generic;
using System.Threading;

namespace cmstar.Caching
{
    /// <summary>
    /// 一个利用<see cref="Dictionary{K,V}"/>简易实现的缓存提供器。
    /// </summary>
    public class SimpleObjectCacheProvider : MemoryBasedCacheProvider, IDisposable
    {
        public static readonly SimpleObjectCacheProvider Instance = new SimpleObjectCacheProvider();

        private readonly Dictionary<string, CacheObject> _cache = new Dictionary<string, CacheObject>();
        private readonly object _syncBlock = new object();

        private bool _disposed;

        public SimpleObjectCacheProvider()
        {
            var cleanerThread = new Thread(() =>
            {
                while (!_disposed)
                {
                    Thread.Sleep(1000);
                    CleanOnce();
                }
            });

            cleanerThread.Start();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;
        }

        /// <inheritdoc />
        protected override object DoGet(string key)
        {
            lock (_syncBlock)
            {
                if (!_cache.TryGetValue(key, out var cacheObject))
                    return null;

                if (cacheObject.Expiry > DateTime.Now)
                    return cacheObject.Value;

                _cache.Remove(key);
                return null;
            }
        }

        /// <inheritdoc />
        protected override void DoSet(string key, object value, TimeSpan expiration)
        {
            lock (_syncBlock)
            {
                InnerSet(key, value, expiration);
            }
        }

        /// <inheritdoc />
        protected override bool DoCreate(string key, object value, TimeSpan expiration)
        {
            lock (_syncBlock)
            {
                if (_cache.ContainsKey(key))
                    return false;

                InnerSet(key, value, expiration);
                return true;
            }
        }

        /// <inheritdoc />
        protected override bool DoRemove(string key)
        {
            lock (_syncBlock)
            {
                return _cache.Remove(key);
            }
        }

        private void InnerSet(string key, object value, TimeSpan expiration)
        {
            if (expiration.Ticks < 0)
                return;

            var expiry = expiration.Ticks == 0
                ? DateTime.MaxValue
                : DateTime.Now.Add(expiration);

            _cache[key] = new CacheObject
            {
                Value = value,
                Expiry = expiry
            };
        }

        private void CleanOnce()
        {
            lock (_syncBlock)
            {
                var expiredKeys = new List<string>();
                var now = DateTime.Now;

                foreach (var kv in _cache)
                {
                    if (kv.Value.Expiry > now)
                        continue;

                    expiredKeys.Add(kv.Key);
                }

                for (int i = 0; i < expiredKeys.Count; i++)
                {
                    _cache.Remove(expiredKeys[i]);
                }
            }
        }

        private struct CacheObject
        {
            public object Value;
            public DateTime Expiry;
        }
    }
}
