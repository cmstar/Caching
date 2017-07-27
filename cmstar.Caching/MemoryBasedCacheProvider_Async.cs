#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    // MemoryBasedCacheProvider 的异步操作的实现，实际上全部是非异步处理的。
    public abstract partial class MemoryBasedCacheProvider
    {
        public Task<T> GetAsync<T>(string key)
        {
            var res = Get<T>(key);
            return Task.FromResult(res);
        }

        public Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            T value;
            var hasValue = TryGet(key, out value);
            var res = new TryGetResult<T>(hasValue, value);
            return Task.FromResult(res);
        }

        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            var res = Create(key, value, expiration);
            return Task.FromResult(res);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            Set(key, value, expiration);
            return Task.Delay(0);
        }

        public Task<bool> RemoveAsync(string key)
        {
            var res = Remove(key);
            return Task.FromResult(res);
        }

        public Task<T> IncreaseAsync<T>(string key, T increment)
        {
            var res = Increase(key, increment);
            return Task.FromResult(res);
        }

        public Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            var res = IncreaseOrCreate(key, increment, expiration);
            return Task.FromResult(res);
        }

        public Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            var res = FieldGet<T, TField>(key, field);
            return Task.FromResult(res);
        }

        public Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            TField value;
            var hasValue = FieldTryGet<T, TField>(key, field, out value);
            var res = new TryGetResult<TField>(hasValue, value);
            return Task.FromResult(res);
        }

        public Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            var res = FieldSet<T, TField>(key, field, value);
            return Task.FromResult(res);
        }

        public Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var res = FieldSetOrCreate<T, TField>(key, field, value, expiration);
            return Task.FromResult(res);
        }

        public Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            var res = FieldIncrease<T, TField>(key, field, increment);
            return Task.FromResult(res);
        }

        public Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            var res = FieldIncreaseOrCreate<T, TField>(key, field, increment, expiration);
            return Task.FromResult(res);
        }
    }
}