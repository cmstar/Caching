using System;
using cmstar.Caching.Reflection;
using cmstar.Caching.Reflection.Emit;

namespace cmstar.Caching
{
    /// <summary>
    /// 提供基于当前程序域使用的内存的缓存的基本读写操作，并基于这些操作进行的扩展及提供线程安全控制。
    /// 这是一个抽象类。
    /// </summary>
    public abstract class MemoryBasedCacheProvider : ICacheFieldAccessable
    {
        private readonly SpinKeyLock<string> _lock = new SpinKeyLock<string>();

        public T Get<T>(string key)
        {
            ArgAssert.NotNullOrEmpty(key, "key");

            using (_lock.Enter(key))
            {
                var v = DoGet(key);
                return v == null || ReferenceEquals(CacheEnv.NullValue, v)
                    ? default(T)
                    : (T)v;
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            ArgAssert.NotNullOrEmpty(key, "key");

            using (_lock.Enter(key))
            {
                object v;
                if (!InternalTryGet(key, out v))
                {
                    value = default(T);
                    return false;
                }

                value = v == null || ReferenceEquals(CacheEnv.NullValue, v)
                    ? default(T)
                    : (T)v;
                return true;
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, "key");

            using (_lock.Enter(key))
            {
                InternalSet(key, value, expiration);
            }
        }

        public bool Remove(string key)
        {
            ArgAssert.NotNullOrEmpty(key, "key");

            using (_lock.Enter(key))
            {
                return DoRemove(key);
            }
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            ArgAssert.NotNullOrEmpty(key, "key");
            ArgAssert.NotNullOrEmpty(field, "field");

            TField value;
            FieldTryGet<T, TField>(key, field, out value);
            return value;
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            ArgAssert.NotNullOrEmpty(key, "key");
            ArgAssert.NotNullOrEmpty(field, "field");

            var getter = TypeMemberAccessorUtils.GetGetAccessor(typeof(T), field);
            if (getter == null)
            {
                value = default(TField);
                return false;
            }

            using (_lock.Enter(key))
            {
                object tar;
                if (!InternalTryGet(key, out tar) || tar == null)
                {
                    value = default(TField);
                    return false;
                }

                value = (TField)getter(tar);
                return true;
            }
        }

        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            ArgAssert.NotNullOrEmpty(key, "key");
            ArgAssert.NotNullOrEmpty(field, "field");

            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return false;

            using (_lock.Enter(key))
            {
                object tar;
                if (!InternalTryGet(key, out tar) || tar == null)
                    return false;

                setter(tar, value);
                return true;
            }
        }

        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, "key");
            ArgAssert.NotNullOrEmpty(field, "field");

            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return false;

            using (_lock.Enter(key))
            {
                object tar;
                if (!InternalTryGet(key, out tar))
                {
                    var constructor = ConstructorInvokerGenerator.CreateDelegate(typeof(T));
                    tar = constructor();
                    setter(tar, value);
                    InternalSet(key, tar, expiration);
                    return true;
                }

                if (tar == null)
                    return false;

                setter(tar, value);
                return true;
            }
        }

        /// <summary>
        /// 从缓存中获取具有指定的键的值。若缓存键不存在，返回null。
        /// </summary>
        protected abstract object DoGet(string key);

        /// <summary>
        /// 将指定的键值存入缓存中，并设置其过期时间。<paramref name="value"/>不会为null。
        /// </summary>
        protected abstract void DoSet(string key, object value, TimeSpan expiration);

        /// <summary>
        /// 从缓存中移除指定的键。
        /// </summary>
        protected abstract bool DoRemove(string key);

        private bool InternalTryGet(string key, out object value)
        {
            var v = DoGet(key);

            if (v == null || ReferenceEquals(CacheEnv.NullValue, v))
            {
                value = null;
                return v != null;
            }

            value = v;
            return true;
        }

        private void InternalSet(string key, object value, TimeSpan expiration)
        {
            DoSet(key, value ?? CacheEnv.NullValue, expiration);
        }
    }
}

