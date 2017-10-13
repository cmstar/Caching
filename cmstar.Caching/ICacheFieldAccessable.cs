using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /// <summary>
    /// 定义一组方法，用于对缓存对象上的指定字段进行基本的读写操作。
    /// </summary>
    public interface ICacheFieldAccessable : ICacheProvider
    {
        /// <summary>
        /// 当指定键的缓存存在时，获取该缓存对象的指定字段的值。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <returns>
        /// 缓存对象的指定字段的值。
        /// 若未能获取到值，返回<typeparamref name="TField"/>的默认值。
        /// </returns>
        TField FieldGet<T, TField>(string key, string field);

        /// <summary>
        /// 当指定键的缓存存在时，获取该缓存对象的指定字段的值。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="value">当获取到了指定字段的值，存放该字段的值。</param>
        /// <returns>true当获取到了指定字段的值；否则为false。</returns>
        bool FieldTryGet<T, TField>(string key, string field, out TField value);

        /// <summary>
        /// 当指定键的缓存存在时，设置具有指定键的缓存对象的指定字段的值。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="value">字段的值。</param>
        /// <returns>true若成功设置了字段值；否则返回false。</returns>
        bool FieldSet<T, TField>(string key, string field, TField value);

        /// <summary>
        /// 设置具有指定键的缓存对象的指定字段的值。
        /// 若对象或字段不存在，尝试创建对象或字段。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="value">字段的值。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <returns>true若成功设置了字段值，或创建了缓存对象或字段；否则返回false。</returns>
        bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration);

        /// <summary>
        /// 当指定键的缓存存在时，获取该缓存对象的指定字段的值。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="FieldGet{T,TField}"/>。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <returns>
        /// 缓存对象的指定字段的值。
        /// 若未能获取到值，返回<typeparamref name="TField"/>的默认值。
        /// </returns>
        Task<TField> FieldGetAsync<T, TField>(string key, string field);

        /// <summary>
        /// 当指定键的缓存存在时，获取该缓存对象的指定字段的值。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="FieldTryGet{T,TField}"/>。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <returns>true当获取到了指定字段的值；否则为false。</returns>
        Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field);

        /// <summary>
        /// 当指定键的缓存存在时，设置具有指定键的缓存对象的指定字段的值。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="FieldSet{T,TField}"/>。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="value">字段的值。</param>
        /// <returns>true若成功设置了字段值；否则返回false。</returns>
        Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value);

        /// <summary>
        /// 设置具有指定键的缓存对象的指定字段的值。
        /// 若对象或字段不存在，尝试创建对象或字段。
        /// 若支持异步操作，则以异步方式处理；否则以非异步方式处理，等同于<see cref="FieldSetOrCreate{T,TField}"/>。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <typeparam name="TField">字段的数据类型。</typeparam>
        /// <param name="key">缓存的键。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="value">字段的值。</param>
        /// <param name="expiration">
        /// 指定被创建的缓存对象的过期时间。使用<see cref="TimeSpan.Zero"/>表示缓存不会过期。
        /// </param>
        /// <returns>true若成功设置了字段值，或创建了缓存对象或字段；否则返回false。</returns>
        Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration);
    }
}
