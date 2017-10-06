using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using cmstar.Caching.Reflection;

namespace cmstar.Caching
{
    // KeyOperation 的异步操作部分。
    public partial struct KeyOperation<TValue>
    {
        /// <summary>
        /// 获取缓存值。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        /// <returns>缓存的值。若缓存不存在，返回null。</returns>
        public async Task<TValue> GetAsync()
        {
            // 通过 CacheProvider.GetAsync 不能知道key是否存在——它在“key不存在”和“key存在但值为默认值”
            // 这两种情况都返回默认值。为了能知道key是否存在，这里调用 TryGet 处理。
            var res = await _cacheManager.CacheProvider.TryGetAsync<TValue>(_key);

            OnSearch(res.HasValue);
            return res.Value;
        }

        /// <summary>
        /// 获取缓存值。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public async Task<TryGetResult<TValue>> TryGetAsync()
        {
            var res = await _cacheManager.CacheProvider.TryGetAsync<TValue>(_key);

            OnSearch(res.HasValue);
            return res;
        }

        /// <summary>
        /// 创建或更新具有指定键的缓存，该缓存带有默认的超时时间<seealso cref="BaseExpirationSeconds"/>。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        public Task SetAsync(TValue value)
        {
            OnAccess();

            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return _cacheManager.CacheProvider.SetAsync(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 创建或更新具有指定键的缓存，并指定其超时时间。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <param name="expirationSeconds">超时时间，单位为秒。</param>
        public Task SetAsync(TValue value, int expirationSeconds)
        {
            OnAccess();
            return _cacheManager.CacheProvider.SetAsync(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 仅当缓存不存在时，创建缓存，该缓存带有默认的超时时间<seealso cref="BaseExpirationSeconds"/>。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        public Task<bool> CreateAsync(TValue value)
        {
            OnAccess();

            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return _cacheManager.CacheProvider.CreateAsync(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 仅当缓存不存在时，创建缓存，并指定其超时时间。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        /// <param name="expirationSeconds">超时时间，单位为秒。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        public Task<bool> CreateAsync(TValue value, int expirationSeconds)
        {
            OnAccess();
            return _cacheManager.CacheProvider.CreateAsync(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 移除具有指定键的缓存。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        public async Task<bool> RemoveAsync()
        {
            var removed = await _cacheManager.CacheProvider.RemoveAsync(_key);
            OnRemove(removed);
            return removed;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheIncreasable.IncreaseAsync{T}(String, T)"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TValue> IncreaseAsync(TValue increment)
        {
            var provider = GetCacheIncreasableProvider();
            OnAccess();
            return provider.IncreaseAsync(_key, increment);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheIncreasable.IncreaseOrCreateAsync{T}(String, T, TimeSpan)"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TValue> IncreaseOrCreateAsync(TValue increment)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return IncreaseOrCreateAsync(increment, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间（秒）在当前对象所绑定的缓存上应用
        /// <see cref="ICacheIncreasable.IncreaseOrCreateAsync{T}(String, T, TimeSpan)"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TValue> IncreaseOrCreateAsync(TValue increment, int expirationSeconds)
        {
            var provider = GetCacheIncreasableProvider();

            OnAccess();

            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.IncreaseOrCreateAsync(_key, increment, expiration);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldGetAsync{T,TField}"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public async Task<TField> FieldGetAsync<TField>(string field)
        {
            var provider = GetCacheFieldAccessableProvider();
            var res = await provider.FieldTryGetAsync<TValue, TField>(_key, field);

            OnSearch(res.HasValue);
            return res.Value;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldGetAsync{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldGetAsync<TField>(Expression<Func<TValue, TField>> selector)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldGetAsync<TField>(name);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldTryGetAsync{T,TField}"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public async Task<TryGetResult<TField>> FieldTryGetAsync<TField>(string field)
        {
            var provider = GetCacheFieldAccessableProvider();
            var res = await provider.FieldTryGetAsync<TValue, TField>(_key, field);

            OnSearch(res.HasValue);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldTryGetAsync{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TryGetResult<TField>> FieldTryGetAsync<TField>(Expression<Func<TValue, TField>> selector)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldTryGetAsync<TField>(name);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetAsync{T,TField}"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<bool> FieldSetAsync<TField>(string field, TField value)
        {
            var provider = GetCacheFieldAccessableProvider();

            OnAccess();

            var res = provider.FieldSetAsync<TValue, TField>(_key, field, value);
            return res;
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetAsync{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<bool> FieldSetAsync<TField>(Expression<Func<TValue, TField>> selector, TField value)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetAsync(name, value);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreateAsync{T,TField}"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<bool> FieldSetOrCreateAsync<TField>(string field, TField value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            return FieldSetOrCreateAsync(field, value, expirationSeconds);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreateAsync{T,TField}"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<bool> FieldSetOrCreateAsync<TField>(string field, TField value, int expirationSeconds)
        {
            var provider = GetCacheFieldAccessableProvider();

            OnAccess();

            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldSetOrCreateAsync<TValue, TField>(_key, field, value, expiration);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreateAsync{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<bool> FieldSetOrCreateAsync<TField>(Expression<Func<TValue, TField>> selector, TField value)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetOrCreateAsync(name, value);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用<see cref="ICacheFieldAccessable.FieldSetOrCreateAsync{T,TField}"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<bool> FieldSetOrCreateAsync<TField>(Expression<Func<TValue, TField>> selector, TField value, int expirationSeconds)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldSetOrCreateAsync(name, value, expirationSeconds);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseAsync{T, TField}(String, String, TField)"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldIncreaseAsync<TField>(string field, TField increment)
        {
            var provider = GetCacheFieldIncreasableProvider();
            OnAccess();
            return provider.FieldIncreaseAsync<TValue, TField>(_key, field, increment);
        }

        /// <summary>
        /// 在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseAsync{T, TField}(String, String, TField)"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldIncreaseAsync<TField>(Expression<Func<TValue, TField>> selector, TField increment)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseAsync(name, increment);
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreateAsync{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldIncreaseOrCreateAsync<TField>(string field, TField increment)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            var res = FieldIncreaseOrCreateAsync(field, increment, expirationSeconds);
            return res;
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreateAsync{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldIncreaseOrCreateAsync<TField>(string field, TField increment, int expirationSeconds)
        {
            var provider = GetCacheFieldIncreasableProvider();

            OnAccess();

            var expiration = TimeSpan.FromSeconds(expirationSeconds);
            var res = provider.FieldIncreaseOrCreateAsync<TValue, TField>(_key, field, increment, expiration);
            return res;
        }

        /// <summary>
        /// 使用默认的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreateAsync{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldIncreaseOrCreateAsync<TField>(Expression<Func<TValue, TField>> selector, TField increment)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseOrCreateAsync(name, increment);
        }

        /// <summary>
        /// 使用指定的过期时间在当前对象所绑定的缓存上应用
        /// <see cref="ICacheFieldIncreasable.FieldIncreaseOrCreateAsync{T, TField}(String, String, TField, TimeSpan)"/>方法。
        /// 通过表达式选择类型成员。
        /// 若缓存提供器支持异步操作，则以异步方式处理；否则以非异步方式处理。
        /// </summary>
        public Task<TField> FieldIncreaseOrCreateAsync<TField>(
            Expression<Func<TValue, TField>> selector, TField increment, int expirationSeconds)
        {
            var name = ReflectionUtils.GetMemberName(selector);
            return FieldIncreaseOrCreateAsync(name, increment, expirationSeconds);
        }
    }
}