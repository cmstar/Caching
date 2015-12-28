using System;
using System.Linq.Expressions;
using cmstar.Caching.Reflection;

namespace cmstar.Caching
{
    /// <summary>
    /// 包含对缓存提供器上的一个特定的缓存键的操作。
    /// </summary>
    public class KeyOperation<TValue>
    {
        private readonly CacheOperationBase _cacheManager;
        private readonly string _key;

        internal KeyOperation(CacheOperationBase cacheManager, string key)
        {
            _cacheManager = cacheManager;
            _key = key;
        }

        /// <summary>
        /// 获取当前使用的缓存键。
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// 获取当前绑定的基本缓存超时时间，单位为秒。
        /// </summary>
        public int BaseExpirationSeconds
        {
            get { return _cacheManager.Expiration.BaseExpirationSeconds; }
        }

        /// <summary>
        /// 获取缓存值。
        /// </summary>
        /// <returns>缓存的值。若缓存不存在，返回null。</returns>
        /// <remarks>
        /// 对于缓存值也可以为null的情况，使用此方法不能分辨缓存是否存在。
        /// 此时可使用<see cref="TryGet"/>方法替代。
        /// </remarks>
        public TValue Get()
        {
            return _cacheManager.CacheProvider.Get<TValue>(_key);
        }

        /// <summary>
        /// 获取缓存值。
        /// </summary>
        /// <param name="value">当缓存存在时，存放缓存的值。</param>
        /// <returns>true若缓存存在；否则为false。</returns>
        public bool TryGet(out TValue value)
        {
            return _cacheManager.CacheProvider.TryGet(_key, out value);
        }

        /// <summary>
        /// 设置一个缓存，并自动设置其超时。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        public void Set(TValue value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            Set(value, expirationSeconds);
        }

        /// <summary>
        /// 设置一个缓存，并指定其超时时间。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <param name="expirationSeconds">超时时间，单位为秒。</param>
        public void Set(TValue value, int expirationSeconds)
        {
            _cacheManager.CacheProvider.Set(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 移除具有指定键的缓存。
        /// </summary>
        public void Remove()
        {
            _cacheManager.CacheProvider.Remove(_key);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.Increase{T}"/>方法。
        /// </summary>
        public bool Increase(TValue increment)
        {
            var provider = GetCacheIncreasableProvider();
            var res = provider.Increase(_key, increment);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.IncreaseGet{T}"/>方法。
        /// </summary>
        public bool IncreaseGet(TValue increment, out TValue result)
        {
            var provider = GetCacheIncreasableProvider();
            var res = provider.IncreaseGet(_key, increment, out result);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.IncreaseCx{T}"/>方法。
        /// </summary>
        public bool IncreaseCx(TValue increment)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return IncreaseCx(increment, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间（秒）在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.IncreaseCx{T}"/>方法。
        /// </summary>
        public bool IncreaseCx(TValue increment, int expirationSeconds)
        {
            var provider = GetCacheIncreasableProvider();
            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.IncreaseCx(_key, increment, expiration);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldGet{T,TField}"/>方法。
        /// </summary>
        public TField FieldGet<TField>(string field)
        {
            var provider = GetCacheFieldAccessableProvider();
            var res = provider.FieldGet<TValue, TField>(_key, field);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldGet{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public TField FieldGet<TField>(Expression<Func<TValue, TField>> selector)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldGet<TField>(name);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldTryGet{T,TField}"/>方法。
        /// </summary>
        public bool FieldTryGet<TField>(string field, out TField value)
        {
            var provider = GetCacheFieldAccessableProvider();
            return provider.FieldTryGet<TValue, TField>(_key, field, out value);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldTryGet{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldTryGet<TField>(Expression<Func<TValue, TField>> selector, out TField value)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldTryGet(name, out value);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSet{T,TField}"/>方法。
        /// </summary>
        public bool FieldSet<TField>(string field, TField value)
        {
            var provider = GetCacheFieldAccessableProvider();
            var res = provider.FieldSet<TValue, TField>(_key, field, value);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSet{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldSet<TField>(Expression<Func<TValue, TField>> selector, TField value)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSet(name, value);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetCx{T,TField}"/>方法。
        /// </summary>
        public bool FieldSetCx<TField>(string field, TField value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return FieldSetCx(field, value, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetCx{T,TField}"/>方法。
        /// </summary>
        public bool FieldSetCx<TField>(string field, TField value, int expirationSeconds)
        {
            var provider = GetCacheFieldAccessableProvider();
            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldSetCx<TValue, TField>(_key, field, value, expiration);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetCx{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldSetCx<TField>(Expression<Func<TValue, TField>> selector, TField value)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetCx(name, value);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetCx{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldSetCx<TField>(Expression<Func<TValue, TField>> selector, TField value, int expirationSeconds)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetCx(name, value, expirationSeconds);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldIncreasable.FieldIncrease{T,TField}"/>方法。
        /// </summary>
        public bool FieldIncrease<TField>(string field, TField increment)
        {
            var provider = GetCacheFieldIncreasableProvider();
            var res = provider.FieldIncrease<TValue, TField>(_key, field, increment);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldIncreasable.FieldIncrease{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldIncrease<TField>(Expression<Func<TValue, TField>> selector, TField increment)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncrease(name, increment);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldIncreasable.FieldIncreaseCx{T,TField}"/>方法。
        /// </summary>
        public bool FieldIncreaseCx<TField>(string field, TField increment)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return FieldIncreaseCx(field, increment, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldIncreasable.FieldIncreaseCx{T,TField}"/>方法。
        /// </summary>
        public bool FieldIncreaseCx<TField>(string field, TField increment, int expirationSeconds)
        {
            var provider = GetCacheFieldIncreasableProvider();
            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldIncreaseCx<TValue, TField>(_key, field, increment, expiration);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldIncreasable.FieldIncreaseCx{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldIncreaseCx<TField>(Expression<Func<TValue, TField>> selector, TField increment)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseCx(name, increment);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldIncreasable.FieldIncreaseCx{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public object FieldIncreaseCx<TField>(
            Expression<Func<TValue, TField>> selector, TField increment, int expirationSeconds)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseCx(name, increment, expirationSeconds);
        }

        private ICacheIncreasable GetCacheIncreasableProvider()
        {
            var res = _cacheManager.CacheProvider as ICacheIncreasable;
            if (res == null)
                throw InvalidOperationError();

            return res;
        }

        private ICacheFieldAccessable GetCacheFieldAccessableProvider()
        {
            var res = _cacheManager.CacheProvider as ICacheFieldAccessable;
            if (res == null)
                throw InvalidOperationError();

            return res;
        }

        private ICacheFieldIncreasable GetCacheFieldIncreasableProvider()
        {
            var res = _cacheManager.CacheProvider as ICacheFieldIncreasable;
            if (res == null)
                throw InvalidOperationError();

            return res;
        }

        private InvalidOperationException InvalidOperationError()
        {
            return new InvalidOperationException("The operation is not valid this cache provider.");
        }
    }
}