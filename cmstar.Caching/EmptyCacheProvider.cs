using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /// <summary>
    /// 一个空的缓存提供器，仅实现了接口，并不提供缓存功能。
    /// </summary>
    /// <remarks>
    /// 缓存的任何存在性判定结果均为否。
    /// 所以对于<see cref="ICacheProvider.Create{T}"/>方法，返回<c>true</c>，表示缓存键原本是不存在的；
    /// 对于<see cref="ICacheProvider.Remove"/>则返回<c>false</c>。
    /// </remarks>
    public class EmptyCacheProvider : ICacheFieldIncreasable, ICacheIncreasable
    {
        /// <inheritdoc />
        public virtual T Get<T>(string key)
        {
            return default(T);
        }

        /// <inheritdoc />
        public virtual bool TryGet<T>(string key, out T value)
        {
            value = default(T);
            return false;
        }

        /// <inheritdoc />
        public virtual bool Create<T>(string key, T value, TimeSpan expiration)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual void Set<T>(string key, T value, TimeSpan expiration)
        {
        }

        /// <inheritdoc />
        public virtual bool Remove(string key)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual TField FieldGet<T, TField>(string key, string field)
        {
            return default(TField);
        }

        /// <inheritdoc />
        public virtual bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            value = default(TField);
            return false;
        }

        /// <inheritdoc />
        public virtual bool FieldSet<T, TField>(string key, string field, TField value)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            return default(TField);
        }

        /// <inheritdoc />
        public virtual TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            return default(TField);
        }

        /// <inheritdoc />
        public virtual T Increase<T>(string key, T increment)
        {
            return default(T);
        }

        /// <inheritdoc />
        public virtual T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            return default(T);
        }

        /// <inheritdoc />
        public virtual Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(default(T));
        }

        /// <inheritdoc />
        public virtual Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            return Task.FromResult(new TryGetResult<T>(false, default(T)));
        }

        /// <inheritdoc />
        public virtual Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public virtual Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task<bool> RemoveAsync(string key)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public virtual Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            return Task.FromResult(default(TField));
        }

        /// <inheritdoc />
        public virtual Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            return Task.FromResult(new TryGetResult<TField>(false, default(TField)));
        }

        /// <inheritdoc />
        public virtual Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public virtual Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public virtual Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            return Task.FromResult(default(TField));
        }

        /// <inheritdoc />
        public virtual Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            return Task.FromResult(default(TField));
        }

        /// <inheritdoc />
        public virtual Task<T> IncreaseAsync<T>(string key, T increment)
        {
            return Task.FromResult(default(T));
        }

        /// <inheritdoc />
        public virtual Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            return Task.FromResult(default(T));
        }
    }
}