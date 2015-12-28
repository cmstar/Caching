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
            using (_lock.Enter(key))
            {
                return DoGet<T>(key);
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            using (_lock.Enter(key))
            {
                return DoTryGet(key, out value);
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            using (_lock.Enter(key))
            {
                DoSet(key, value, expiration);
            }
        }

        public bool Remove(string key)
        {
            using (_lock.Enter(key))
            {
                return DoRemove(key);
            }
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            TField value;
            FieldTryGet<T, TField>(key, field, out value);
            return value;
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            var getter = TypeMemberAccessorUtils.GetGetAccessor(typeof(T), field);
            if (getter == null)
            {
                value = default(TField);
                return false;
            }

            using (_lock.Enter(key))
            {
                T tar;
                if (!DoTryGet(key, out tar) || tar == null)
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
            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return false;

            using (_lock.Enter(key))
            {
                T tar;
                if (!DoTryGet(key, out tar) || tar == null)
                    return false;

                setter(tar, value);
                return true;
            }
        }

        public bool FieldSetCx<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return false;

            using (_lock.Enter(key))
            {
                T tar;
                if (!DoTryGet(key, out tar))
                {
                    var constructor = ConstructorInvokerGenerator.CreateDelegate(typeof(T));
                    tar = (T)constructor();
                    setter(tar, value);
                    DoSet(key, tar, expiration);
                    return false;
                }

                if (tar == null)
                    return false;

                setter(tar, value);
                return true;
            }
        }

        /// <summary>
        /// 提供<see cref="Get{T}"/>方法定义的对于缓存的操作的实际实现。
        /// </summary>
        protected abstract T DoGet<T>(string key);

        /// <summary>
        /// 提供<see cref="TryGet{T}"/>方法定义的对于缓存的操作的实际实现。
        /// </summary>
        protected abstract bool DoTryGet<T>(string key, out T value);

        /// <summary>
        /// 提供<see cref="Set{T}"/>方法定义的对于缓存的操作的实际实现。
        /// </summary>
        protected abstract void DoSet<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 提供<see cref="Remove"/>方法定义的对于缓存的操作的实际实现。
        /// </summary>
        protected abstract bool DoRemove(string key);
    }
}

