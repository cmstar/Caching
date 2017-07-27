using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /// <summary>
    /// 定义基本的缓存提供器。
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// 获取具有指定键的缓存值。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>缓存的值。若缓存不存在，返回<typeparamref name="T"/>的默认值。</returns>
        /// <remarks>
        /// 对于缓存值也可以为null的情况，使用此方法不能分辨缓存是否存在。
        /// 此时可使用<see cref="TryGet{T}"/>方法替代。
        /// </remarks>
        T Get<T>(string key);

        /// <summary>
        /// 获取具有指定键的缓存值。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">当缓存存在时，存放缓存的值。</param>
        /// <returns>true若缓存存在；否则为false。</returns>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// 仅当缓存不存在时，创建缓存。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        bool Create<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 创建或更新具有指定键的缓存。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        void Set<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 移除具有指定键的缓存。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        bool Remove(string key);

        /// <summary>
        /// 获取具有指定键的缓存值。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="Get{T}"/>。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>缓存的值。若缓存不存在，返回<typeparamref name="T"/>的默认值。</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 尝试获取具有指定键的缓存值。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="TryGet{T}"/>。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>获取的结果。</returns>
        Task<TryGetResult<T>> TryGetAsync<T>(string key);

        /// <summary>
        /// 仅当缓存不存在时，创建缓存。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="Create{T}"/>。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 创建或更新具有指定键的缓存。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="Set{T}"/>。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 移除具有指定键的缓存。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="Remove"/>。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        Task<bool> RemoveAsync(string key);
    }
}
