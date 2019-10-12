using System;
using cmstar.Caching.Reflection;
using cmstar.Caching.Reflection.Emit;
using System.Collections.Concurrent;

namespace cmstar.Caching
{
    /// <summary>
    /// 提供基于当前程序域使用的内存的缓存的基本读写操作，并基于这些操作进行的扩展及提供线程安全控制。
    /// 这是一个抽象类。
    /// </summary>
    public abstract partial class MemoryBasedCacheProvider : ICacheFieldIncreasable, ICacheIncreasable
    {
        private readonly SpinKeyLock<string> _lock = new SpinKeyLock<string>();

        public T Get<T>(string key)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                var v = InternalGet(key);
                if (v == null)
                    return default(T);

                return v is T
                    ? (T)v
                    : (T)Convert.ChangeType(v, typeof(T));
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                object v;
                if (!InternalTryGet(key, out v))
                {
                    value = default(T);
                    return false;
                }

                switch (v)
                {
                    case null:
                        value = default(T);
                        break;

                    case T directValue:
                        value = directValue;
                        break;

                    default:
                        value = (T)Convert.ChangeType(v, typeof(T));
                        break;
                }

                return true;
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                InternalSet(key, value, expiration);
            }
        }

        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                return InternalCreate(key, value, expiration);
            }
        }

        public bool Remove(string key)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                return DoRemove(key);
            }
        }

        public T Increase<T>(string key, T increment)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                CacheValue cacheValue;
                if (!InternalTryGetRaw(key, out cacheValue))
                    return default(T);

                var result = Adding.AddIntegers(cacheValue.Value, increment);
                cacheValue.Value = result.Value;

                return result.IsIncrementType
                    ? (T)result.Value
                    : (T)Convert.ChangeType(result, typeof(T));
            }
        }

        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            using (_lock.Enter(key))
            {
                CacheValue cacheValue;
                if (!InternalTryGetRaw(key, out cacheValue))
                {
                    InternalSet(key, increment, expiration);
                    return increment;
                }

                var result = Adding.AddIntegers(cacheValue.Value, increment);
                cacheValue.Value = result.Value;

                return result.IsIncrementType
                    ? (T)result.Value
                    : (T)Convert.ChangeType(result, typeof(T));
            }
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));
            ArgAssert.NotNullOrEmpty(field, nameof(field));

            TField value;
            FieldTryGet<T, TField>(key, field, out value);
            return value;
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));
            ArgAssert.NotNullOrEmpty(field, nameof(field));

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
            ArgAssert.NotNullOrEmpty(key, nameof(key));
            ArgAssert.NotNullOrEmpty(field, nameof(field));

            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return false;

            using (_lock.Enter(key))
            {
                object tar;
                if (!InternalTryGet(key, out tar) || tar == null)
                    return false;

                var fieldType = ResolveMemberType(typeof(T), field);
                var newFieldValue = typeof(TField) == fieldType
                    ? value
                    : Convert.ChangeType(value, fieldType);
                setter(tar, newFieldValue);

                return true;
            }
        }

        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));
            ArgAssert.NotNullOrEmpty(field, nameof(field));

            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return false;

            using (_lock.Enter(key))
            {
                object tar;
                var exists = InternalTryGet(key, out tar);

                if (!exists)
                {
                    var constructor = ConstructorInvokerGenerator.CreateDelegate(typeof(T));
                    tar = constructor();
                }
                else if (tar == null)
                {
                    // 缓存存在但为null，null不能进行字段赋值，如果此时生成新对象替换掉null，就不
                    // 仅仅是更新了字段，而是更新了整个缓存，与API定义预期不符，所以这里直接返回false
                    return false;
                }

                var fieldType = ResolveMemberType(typeof(T), field);
                var newFieldValue = typeof(TField) == fieldType
                    ? value
                    : Convert.ChangeType(value, fieldType);
                setter(tar, newFieldValue);

                return exists || InternalCreate(key, tar, expiration);
            }
        }

        public TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));
            ArgAssert.NotNullOrEmpty(field, nameof(field));

            var getter = TypeMemberAccessorUtils.GetGetAccessor(typeof(T), field);
            if (getter == null)
                return default(TField);

            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return default(TField);

            using (_lock.Enter(key))
            {
                object tar;
                if (!InternalTryGet(key, out tar) || tar == null)
                    return default(TField);

                var oldFieldValue = getter(tar);
                var result = Adding.AddIntegers(oldFieldValue, increment);

                var newFieldValue = result.IsTargetType
                    ? result.Value
                    : Convert.ChangeType(result.Value, oldFieldValue.GetType());
                setter(tar, newFieldValue);

                return result.IsIncrementType
                    ? (TField)result.Value
                    : (TField)Convert.ChangeType(result, typeof(TField));
            }
        }

        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));
            ArgAssert.NotNullOrEmpty(field, nameof(field));

            var getter = TypeMemberAccessorUtils.GetGetAccessor(typeof(T), field);
            if (getter == null)
                return default(TField);

            var setter = TypeMemberAccessorUtils.GetSetAccessor(typeof(T), field);
            if (setter == null)
                return default(TField);

            using (_lock.Enter(key))
            {
                object tar, newFieldValue;
                if (!InternalTryGet(key, out tar))
                {
                    var constructor = ConstructorInvokerGenerator.CreateDelegate(typeof(T));
                    tar = constructor();

                    var fieldType = ResolveMemberType(typeof(T), field);
                    newFieldValue = typeof(TField) == fieldType
                        ? increment
                        : Convert.ChangeType(increment, fieldType);
                    setter(tar, newFieldValue);

                    InternalSet(key, tar, expiration);
                    return increment;
                }

                if (tar == null)
                    return default(TField);

                var oldFieldValue = getter(tar);
                var result = Adding.AddIntegers(oldFieldValue, increment);

                newFieldValue = result.IsTargetType
                    ? result.Value
                    : Convert.ChangeType(result.Value, oldFieldValue.GetType());
                setter(tar, newFieldValue);

                return result.IsIncrementType
                    ? (TField)result.Value
                    : (TField)Convert.ChangeType(result, typeof(TField));
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
        /// 仅当缓存不存在时，创建缓存，并设置其过期时间。<paramref name="value"/>不会为null。
        /// </summary>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        protected abstract bool DoCreate(string key, object value, TimeSpan expiration);

        /// <summary>
        /// 从缓存中移除指定的键。
        /// </summary>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        protected abstract bool DoRemove(string key);

        private object InternalGet(string key)
        {
            var v = DoGet(key);
            return v == null || ReferenceEquals(CacheEnv.NullValue, v)
                ? null
                : ((CacheValue)v).Value;
        }

        private bool InternalTryGet(string key, out object value)
        {
            CacheValue cacheValue;
            var res = InternalTryGetRaw(key, out cacheValue);
            value = cacheValue?.Value;
            return res;
        }

        private bool InternalTryGetRaw(string key, out CacheValue value)
        {
            var v = DoGet(key);
            if (v == null || ReferenceEquals(CacheEnv.NullValue, v))
            {
                value = null;
                return v != null;
            }

            value = (CacheValue)v;
            return true;
        }

        private bool InternalCreate(string key, object value, TimeSpan expiration)
        {
            var cacheValue = value == null ? CacheEnv.NullValue : new CacheValue(value);
            return DoCreate(key, cacheValue, expiration);
        }

        private void InternalSet(string key, object value, TimeSpan expiration)
        {
            var cacheValue = value == null ? CacheEnv.NullValue : new CacheValue(value);
            DoSet(key, cacheValue, expiration);
        }

        private static readonly ConcurrentDictionary<TypeMemberIndentity, Type> MemberTypes
            = new ConcurrentDictionary<TypeMemberIndentity, Type>();

        private static Type ResolveMemberType(Type type, string memberName)
        {
            var id = new TypeMemberIndentity(type, memberName);
            return MemberTypes.GetOrAdd(id, x =>
            {
                var propInfo = type.GetProperty(memberName);
                if (propInfo != null)
                    return propInfo.PropertyType;

                var fieldInfo = type.GetField(memberName);
                if (fieldInfo != null)
                    return fieldInfo.FieldType;

                var msg = $"The member '{memberName}' was not found on type '{type}'.";
                throw new InvalidOperationException(msg);
            });
        }
    }
}
