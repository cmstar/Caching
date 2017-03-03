using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 定义一组方法，用于对数值型的缓存值进行增减操作。
    /// </summary>
    public interface ICacheIncreasable : ICacheProvider
    {
        /// <summary>
        /// 当指定键的缓存存在时，将一个增量累加到该缓存的值上。
        /// 增量必须是整数，或能够转换为整数。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="increment">增量，必须是整数，或能够转换为整数。使用负数来做减法。</param>
        /// <returns>指定键的缓存存在时，返回增加后的值；否则返回<typeparamref name="T"/>的默认值。</returns>
        T Increase<T>(string key, T increment);

        /// <summary>
        /// 当指定键的缓存存在时，将一个增量累加到该缓存的值上。
        /// 若缓存不存在，则初始化其值为增量值，并具有给定的过期时间。
        /// 增量必须是整数，或能够转换为整数。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="increment">增量，必须是整数，或能够转换为整数。使用负数来做减法。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <returns>返回增加（缓存不存在时为新建）后的值。</returns>
        /// <remarks>'Cx' means 'Create if not eXists'.</remarks>
        T IncreaseCx<T>(string key, T increment, TimeSpan expiration);
    }
}