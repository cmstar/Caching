using System;

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
        /// <returns>缓存的值。若缓存不存在，返回null。</returns>
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
        /// 设置一个缓存。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        /// <param name="value">缓存的值。</param>
        /// <param name="expiration">指定缓存在多长时间后过期。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。</param>
        void Set<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// 移除具有指定键的缓存。
        /// </summary>
        /// <param name="key">缓存的键。</param>
        bool Remove(string key);
    }
}
