using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /*
     * 将缓存提供器的异步操作转为非异步操作，使非异步操作的测试用例可以用在异步操作上。
     */

    /// <summary>
    /// 将<see cref="ICacheProvider"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheProviderAdapter : ICacheProvider
    {
        protected readonly ICacheProvider Cache;

        public CacheProviderAdapter(ICacheProvider cache)
        {
            Cache = cache;
        }

        public T Get<T>(string key)
        {
            return SafelyGetResult(Cache.GetAsync<T>(key));
        }

        public bool TryGet<T>(string key, out T value)
        {
            var res = SafelyGetResult(Cache.TryGetAsync<T>(key));
            value = res.Value;
            return res.HasValue;
        }

        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            return SafelyGetResult(Cache.CreateAsync(key, value, expiration));
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            SafelyWait(Cache.SetAsync(key, value, expiration));
        }

        public bool Remove(string key)
        {
            return SafelyGetResult(Cache.RemoveAsync(key));
        }

        public Task<T> GetAsync<T>(string key)
        {
            throw new NotSupportedException();
        }

        public Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            throw new NotSupportedException();
        }

        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }

        public Task<bool> RemoveAsync(string key)
        {
            throw new NotSupportedException();
        }

        protected T SafelyGetResult<T>(Task<T> task)
        {
            SafelyWait(task);
            return task.Result;
        }

        protected void SafelyWait(Task task)
        {
            while (!task.IsCompleted)
            {
                Thread.Sleep(1);
            }

            // 尝试保留原始异常。
            // 若不处理，异常会被装在 AggregateException 里，可能影响测试用例对于异常的断言。
            if (task.IsFaulted && task.Exception != null)
            {
                var ex = task.Exception.InnerExceptions[0];
                PreserveStackTrace(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 对异常进行处理，使异常的<see cref="Exception.StackTrace"/>在使用
        /// throw重新抛出后仍保留有throw前的内容。
        /// </summary>
        /// <param name="ex">待处理的异常实例。</param>
        /// <exception cref="ArgumentNullException">当<paramref name="ex"/>为null。</exception>
        /// <remarks>
        /// 此方式使用反射调用Exception类的内部方法InternalPreserveStackTrace，
        /// 该方法只保留rethrow前的异常堆栈的字符串描述信息，
        /// 若使用<see cref="T:System.Diagnostics.StackTrace"/>访问堆栈信息，
        /// 仍只能获取到rethrow的位置。
        /// </remarks>
        private static void PreserveStackTrace(Exception ex)
        {
            ArgAssert.NotNull(ex, nameof(ex));

            var m = ex.GetType().GetMethod(
                "InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic);

            // 如果找不到相关方法了，直接终止当前过程。
            // 调用当前方法多处于 catch 块里，如果这里抛一个异常，有导致整个程序崩溃的风险。
            if (m == null)
                return;

            m.Invoke(ex, null);

            //下面是另一种方法，不过它需要exception具有实现序列化的构造函数，在自定义异常上会有问题：
            /*
            var c = new StreamingContext(StreamingContextStates.CrossAppDomain);
            var m = new ObjectManager(null, c);
            var s = new SerializationInfo(ex.GetType(), new FormatterConverter());

            ex.GetObjectData(s, c);
            m.RegisterObject(ex, 1, s); //prepare for SetObjectData
            m.DoFixups(); //ObjectManager calls SetObjectData
             */
        }
    }

    /// <summary>
    /// 将<see cref="ICacheIncreasable"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheIncreasableAdapter : CacheProviderAdapter, ICacheIncreasable
    {
        private readonly ICacheIncreasable _cache;

        public CacheIncreasableAdapter(ICacheIncreasable cache)
            : base(cache)
        {
            _cache = cache;
        }

        public T Increase<T>(string key, T increment)
        {
            return SafelyGetResult(_cache.IncreaseAsync(key, increment));
        }

        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            return SafelyGetResult(_cache.IncreaseOrCreateAsync(key, increment, expiration));
        }

        public Task<T> IncreaseAsync<T>(string key, T increment)
        {
            throw new NotSupportedException();
        }

        public Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 将<see cref="ICacheFieldAccessable"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheFieldAccessableAdapter : CacheProviderAdapter, ICacheFieldAccessable
    {
        private readonly ICacheFieldAccessable _cache;

        public CacheFieldAccessableAdapter(ICacheFieldAccessable cache)
            : base(cache)
        {
            _cache = cache;
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            return SafelyGetResult(_cache.FieldGetAsync<T, TField>(key, field));
        }

        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            return SafelyGetResult(_cache.FieldSetAsync<T, TField>(key, field, value));
        }

        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            return SafelyGetResult(_cache.FieldSetOrCreateAsync<T, TField>(key, field, value, expiration));
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            var res = SafelyGetResult(_cache.FieldTryGetAsync<T, TField>(key, field));
            value = res.Value;
            return res.HasValue;
        }

        public Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            throw new NotSupportedException();
        }

        public Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            throw new NotSupportedException();
        }

        public Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            throw new NotSupportedException();
        }

        public Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 将<see cref="ICacheFieldIncreasable"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheFieldIncreasableAdapter : CacheFieldAccessableAdapter, ICacheFieldIncreasable
    {
        private readonly ICacheFieldIncreasable _cache;

        public CacheFieldIncreasableAdapter(ICacheFieldIncreasable cache)
            : base(cache)
        {
            _cache = cache;
        }

        public TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            return SafelyGetResult(_cache.FieldIncreaseAsync<T, TField>(key, field, increment));
        }

        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            return SafelyGetResult(_cache.FieldIncreaseOrCreateAsync<T, TField>(key, field, increment, expiration));
        }

        public Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            throw new NotSupportedException();
        }

        public Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }
    }
}
