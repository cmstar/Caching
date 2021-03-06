using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using cmstar.Caching.Reflection;

namespace cmstar.Caching
{
    /// <summary>
    /// 包含对缓存提供器上的一个特定的缓存键的操作。
    /// </summary>
    public partial struct KeyOperation<TValue>
    {
        private readonly CacheOperationBase _cacheManager;
        private readonly string _key;

        /// <summary>
        /// 创建<see cref="KeyOperation{TValue}"/>。
        /// 必须总是通过此构造函数创建，否则所依赖成员不能被正确初始化。
        /// </summary>
        /// <param name="cacheManager">
        /// 创建当前<see cref="KeyOperation{TValue}"/>的<see cref="CacheOperationBase"/>实例。
        /// </param>
        /// <param name="key">缓存的key。</param>
        internal KeyOperation(CacheOperationBase cacheManager, string key)
        {
            _cacheManager = cacheManager;
            _key = key;
        }

        /// <summary>
        /// 获取当前使用的缓存键。
        /// </summary>
        public string Key => _key;

        /// <summary>
        /// 缓存键的完整前缀，以命名空间开头，格式如 NAMESPACE:KEYROOT。
        /// </summary>
        public string KeyBase => _cacheManager.KeyBase;

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
            // 通过 CacheProvider.Get 不能知道key是否存在——它在“key不存在”和“key存在但值为默认值”
            // 这两种情况都返回默认值。为了能知道key是否存在，这里调用 TryGet 处理。
            TValue value;
            var hit = _cacheManager.CacheProvider.TryGet(_key, out value);

            OnSearch(hit);
            return value;
        }

        /// <summary>
        /// 获取缓存值。
        /// </summary>
        /// <param name="value">当缓存存在时，存放缓存的值。</param>
        /// <returns>true若缓存存在；否则为false。</returns>
        public bool TryGet(out TValue value)
        {
            var hit = _cacheManager.CacheProvider.TryGet(_key, out value);

            OnSearch(hit);
            return hit;
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
            OnAccess();
            _cacheManager.CacheProvider.Set(_key, value, TimeSpan.FromSeconds(expirationSeconds));
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
            OnAccess();
            return _cacheManager.CacheProvider.Create(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 移除具有指定键的缓存。
        /// </summary>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        public bool Remove()
        {
            var removed = _cacheManager.CacheProvider.Remove(_key);
            OnRemove(removed);
            return removed;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.Increase{T}(String, T)"/>方法。
        /// </summary>
        public TValue Increase(TValue increment)
        {
            var provider = GetCacheIncreasableProvider();
            OnAccess();
            return provider.Increase(_key, increment);
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

            OnAccess();

            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.IncreaseOrCreate(_key, increment, expiration);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldGet{T,TField}"/>方法。
        /// </summary>
        public TField FieldGet<TField>(string field)
        {
            var provider = GetCacheFieldAccessableProvider();

            TField value;
            var hit = provider.FieldTryGet<TValue, TField>(_key, field, out value);

            OnSearch(hit);
            return value;
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
            var hit = provider.FieldTryGet<TValue, TField>(_key, field, out value);

            OnSearch(hit);
            return hit;
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

            OnAccess();

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

            OnAccess();

            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldSetOrCreate<TValue, TField>(_key, field, value, expiration);
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
            OnAccess();
            return provider.FieldIncrease<TValue, TField>(_key, field, increment);
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

            OnAccess();

            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldIncreaseOrCreate<TValue, TField>(_key, field, increment, expiration);
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
            var msg = $"The operation is not valid on the cache provider {_cacheManager.CacheProvider.GetType()}.";
            return new InvalidOperationException(msg);
        }

#if  !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void OnAccess()
        {
            var observer = _cacheManager.Observer;
            observer?.Accessed(KeyBase);
        }

        // 此方法将 ICacheOperation.Accessed + Removed 合并在一个过程中调用，
        // 以减少对 _cacheManager.Observer 的访问和空值判断次数。
        // 调用此方法时，不需要再调用 OnAccess。
        private void OnRemove(bool removed)
        {
            var observer = _cacheManager.Observer;
            if (observer == null)
                return;

            if (removed)
            {
                observer.Removed(KeyBase);
            }
            else
            {
                observer.Accessed(KeyBase);
            }
        }

        private void OnSearch(bool hit)
        {
            var observer = _cacheManager.Observer;
            if (observer == null)
                return;

            if (hit)
            {
                observer.Hit(KeyBase);
            }
            else
            {
                observer.Missed(KeyBase);
            }
        }
    }
}