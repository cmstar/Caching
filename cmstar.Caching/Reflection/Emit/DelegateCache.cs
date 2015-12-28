using System;
using System.Collections.Concurrent;

namespace cmstar.Caching.Reflection.Emit
{
    /// <summary>
    /// Provides a thread-safe cache for storing the delegates.
    /// </summary>
    internal static class DelegateCache
    {
        private static readonly ConcurrentDictionary<object, Delegate> Cache
            = new ConcurrentDictionary<object, Delegate>();

        public static void Add(object identity, Delegate d)
        {
            Cache[identity] = d;
        }

        public static Delegate Get(object identity)
        {
            Delegate value;
            return Cache.TryGetValue(identity, out value) ? value : null;
        }

        public static Delegate GetOrAdd(object identity, Func<object, Delegate> valueFactory)
        {
            return Cache.GetOrAdd(identity, valueFactory);
        }
    }
}
