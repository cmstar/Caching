using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /// <summary>
    /// 定义缓存提供器上的基础异步操作。
    /// </summary>
    public interface ICacheProviderAsync
    {
        /// <summary>
        /// 获取具有指定键的缓存值。这是一个异步操作。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>缓存的值。若缓存不存在，返回<typeparamref name="T"/>的默认值。</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 尝试获取具有指定键的缓存值。这是一个异步操作。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>获取的结果。</returns>
        Task<TryGetResult<T>> TryGetAsync<T>(string key);

        /// <summary>
        /// 仅当缓存不存在时，创建缓存。这是一个异步操作。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        /// <returns>true表示创建了缓存；false说明缓存已经存在了。</returns>
        Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 创建或更新具有指定键的缓存。这是一个异步操作。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 移除具有指定键的缓存。这是一个异步操作。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <returns>true若缓存被移除；若缓存键不存在，返回false。</returns>
        Task<bool> RemoveAsync(string key);
    }
}
