using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 定义一组方法，用于对数值型的缓存值进行增减操作。
    /// </summary>
    public interface ICacheIncreasable : ICacheProvider
    {
        /// <summary>
        /// 当指定键的缓存存在时，将一个增量累加到该缓存对的值上。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <returns>true若对已存在的缓存对象进行了设值；否则返回false。</returns>
        bool Increase<T>(string key, T increment);

        /// <summary>
        /// 当指定键的缓存存在时，将一个增量累加到该缓存对的值上。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <param name="result">当成功设置了缓存值时，存放增加后的值。</param>
        /// <returns>true若对已存在的缓存对象进行了设值；否则返回false。</returns>
        bool IncreaseGet<T>(string key, T increment, out T result);

        /// <summary>
        /// 当指定键的缓存存在时，将一个增量累加到该缓存的值上。
        /// 若缓存不存在，则初始化其值为增量值。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <returns>
        /// true若对已存在的缓存对象进行了设值；否则返回false，此时创建了新的缓存对象。
        /// </returns>
        bool IncreaseCx<T>(string key, T increment, TimeSpan expiration);

        /// <summary>
        /// 当指定键的缓存存在时，将一个增量累加到该缓存的值上。
        /// 若缓存不存在，则初始化其值为增量值。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <param name="result">存放增加后的值。</param>
        /// <returns>
        /// true若对已存在的缓存对象进行了设值；否则返回false，此时创建了新的缓存对象。
        /// </returns>
        bool IncreaseGetCx<T>(string key, T increment, TimeSpan expiration, out T result);
    }
}