using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 定义一组方法，用于对缓存对象上的指定字段进行数值的增减操作。
    /// </summary>
    public interface ICacheFieldIncreasable : ICacheFieldAccessable
    {
        /// <summary>
        /// 将一个增量累加到具有指定键的缓存对象的的指定字段上。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <returns>true若对已存在的缓存对象的对应字段进行了设值；否则返回false。</returns>
        bool FieldIncrease<T, TField>(string key, string field, TField increment);

        /// <summary>
        /// 将一个增量累加到具有指定键的缓存对象的的指定字段上。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <param name="result">当成功设值后，存放增加后的值。</param>
        /// <returns>true若对已存在的缓存对象的对应字段进行了设值；否则返回false。</returns>
        bool FieldIncreaseGet<T, TField>(string key, string field, TField increment, out TField result);

        /// <summary>
        /// 将一个增量累加到具有指定键的缓存对象的的指定字段上。
        /// 若对象或字段不存在，尝试创建对象或字段。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <returns>
        /// true若对已存在的缓存对象的对应字段进行了设值，或创建了缓存对象或字段；否则返回false。
        /// </returns>
        /// <remarks>'Cx' means 'Create if not eXists'.</remarks>
        bool FieldIncreaseCx<T, TField>(string key, string field, TField increment, TimeSpan expiration);

        /// <summary>
        /// 将一个增量累加到具有指定键的缓存对象的的指定字段上。
        /// 若对象或字段不存在，尝试创建对象或字段。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="increment">增量。使用负数来做减法。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <param name="result">当成功设值后，存放增加后的值。</param>
        /// <returns>
        /// true若对已存在的缓存对象的对应字段进行了设值，或创建了缓存对象或字段；否则返回false。
        /// </returns>
        /// <remarks>'Cx' means 'Create if not eXists'.</remarks>
        bool FieldIncreaseGetCx<T, TField>(string key, string field, TField increment, TimeSpan expiration, out TField result);
    }
}