using System;
using System.Linq.Expressions;
using cmstar.Caching.Reflection;

namespace cmstar.Caching
{
    public partial class KeyOperation<TValue>
    {
        private readonly CacheOperationBase _cacheManager;

        internal KeyOperation(CacheOperationBase cacheManager, string key)
        {
            _cacheManager = cacheManager;
            Key = key;
        }

        /// <summary>
        /// 获取当前使用的缓存键。
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 获取当前绑定的基本缓存超时时间，单位为秒。
        /// </summary>
        public int BaseExpirationSeconds => _cacheManager.Expiration.BaseExpirationSeconds;

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
            return _cacheManager.CacheProvider.Get<TValue>(Key);
        }

        /// <summary>
        /// 获取缓存值。
        /// </summary>
        /// <param name="value">当缓存存在时，存放缓存的值。</param>
        /// <returns>true若缓存存在；否则为false。</returns>
        public bool TryGet(out TValue value)
        {
            return _cacheManager.CacheProvider.TryGet(Key, out value);
        }

        /// <summary>
        /// 创建或更新具有指定键的缓存，该缓存带有默认的超时时间<seealso cref="BaseExpirationSeconds"/>。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        public void Set(TValue value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            Set(value, expirationSeconds);
        }

        /// <summary>
        /// 创建或更新具有指定键的缓存，并指定其超时时间。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <param name="expirationSeconds">超时时间，单位为秒。</param>
        public void Set(TValue value, int expirationSeconds)
        {
            _cacheManager.CacheProvider.Set(Key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 仅当缓存不存在时，创建缓存，该缓存带有默认的超时时间<seealso cref="BaseExpirationSeconds"/>。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        public bool Create(TValue value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return Create(value, expirationSeconds);
        }

        /// <summary>
        /// 仅当缓存不存在时，创建缓存，并指定其超时时间。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <param name="expirationSeconds">超时时间，单位为秒。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        public bool Create(TValue value, int expirationSeconds)
        {
            return _cacheManager.CacheProvider.Create(Key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 移除具有指定键的缓存。
        /// </summary>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        public bool Remove()
        {
            return _cacheManager.CacheProvider.Remove(Key);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.Increase{T}(String, T)"/>方法。
        /// </summary>
        public TValue Increase(TValue increment)
        {
            var provider = GetCacheIncreasableProvider();
            return provider.Increase(Key, increment);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheIncreasable.IncreaseOrCreate{T}(String, T, TimeSpan)"/>方法。
        /// </summary>
        public TValue IncreaseOrCreate(TValue increment)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return IncreaseOrCreate(increment, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间（秒）在当前对象所绑定的缓存上应用
        /// <see cref="ICacheIncreasable.IncreaseOrCreate{T}(String, T, TimeSpan)"/>方法。
        /// </summary>
        public TValue IncreaseOrCreate(TValue increment, int expirationSeconds)
        {
            var provider = GetCacheIncreasableProvider();
            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.IncreaseOrCreate(Key, increment, expiration);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldGet{T,TField}"/>方法。
        /// </summary>
        public TField FieldGet<TField>(string field)
        {
            var provider = GetCacheFieldAccessableProvider();
            var res = provider.FieldGet<TValue, TField>(Key, field);
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
            return provider.FieldTryGet<TValue, TField>(Key, field, out value);
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
            var res = provider.FieldSet<TValue, TField>(Key, field, value);
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
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreate{T,TField}"/>方法。
        /// </summary>
        public bool FieldSetOrCreate<TField>(string field, TField value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return FieldSetOrCreate(field, value, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreate{T,TField}"/>方法。
        /// </summary>
        public bool FieldSetOrCreate<TField>(string field, TField value, int expirationSeconds)
        {
            var provider = GetCacheFieldAccessableProvider();
            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldSetOrCreate<TValue, TField>(Key, field, value, expiration);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreate{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldSetOrCreate<TField>(Expression<Func<TValue, TField>> selector, TField value)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetOrCreate(name, value);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreate{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public bool FieldSetOrCreate<TField>(Expression<Func<TValue, TField>> selector, TField value, int expirationSeconds)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetOrCreate(name, value, expirationSeconds);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncrease{T, TField}(String, String, TField)"/>方法。
        /// </summary>
        public TField FieldIncrease<TField>(string field, TField increment)
        {
            var provider = GetCacheFieldIncreasableProvider();
            return provider.FieldIncrease<TValue, TField>(Key, field, increment);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncrease{T, TField}(String, String, TField)"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public TField FieldIncrease<TField>(Expression<Func<TValue, TField>> selector, TField increment)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncrease(name, increment);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreate{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// </summary>
        public TField FieldIncreaseOrCreate<TField>(string field, TField increment)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            var res = FieldIncreaseOrCreate(field, increment, expirationSeconds);
            return res;
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreate{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// </summary>
        public TField FieldIncreaseOrCreate<TField>(string field, TField increment, int expirationSeconds)
        {
            var provider = GetCacheFieldIncreasableProvider();
            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldIncreaseOrCreate<TValue, TField>(Key, field, increment, expiration);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreate{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public TField FieldIncreaseOrCreate<TField>(Expression<Func<TValue, TField>> selector, TField increment)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseOrCreate(name, increment);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreate{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// 通过表达式选择类型成员。
        /// </summary>
        public TField FieldIncreaseOrCreate<TField>(
            Expression<Func<TValue, TField>> selector, TField increment, int expirationSeconds)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseOrCreate(name, increment, expirationSeconds);
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
            var msg = string.Format(
                "The operation is not valid the cache provider {0}.",
                _cacheManager.CacheProvider.GetType());
            return new InvalidOperationException(msg);
        }
    }
}