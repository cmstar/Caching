using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using cmstar.Caching.Reflection;
using cmstar.RapidReflection.Emit;
using cmstar.Serialization.Json;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 包含CLR对象与Redis存储值间转换的相关方法。
    /// </summary>
    public static class RedisConvert
    {
        /// <summary>
        /// 当对象不能被拆解为键值对存储在hash上时，仍要使用hash存储，则将字段序列化（为JSON），
        /// 以单一键值的形式存储，此时hash仅包含此字段。使用一个特殊的名称表示此字段。
        /// </summary>
        public const string EntryNameForSpecialValue = "$<>_..value";

        /// <summary>
        /// 将对象序列化到<see cref="HashEntry"/>的集合。
        /// </summary>
        /// <param name="obj">被序列化的对象。</param>
        /// <param name="shouldRemoveSpecialEntry">
        /// 指定在覆盖原有的entry时，是否需要先将<see cref="EntryNameForSpecialValue"/>域移除。
        /// 因为该域与其他域互斥。
        /// </param>
        /// <returns><see cref="HashEntry"/>的集合。</returns>
        public static HashEntry[] ToHashEntries(object obj, out bool shouldRemoveSpecialEntry)
        {
            /*
             * 对于简单类型，其在hash上的值是直接存储在EntryNameForSpecialValue域上的；
             * 复杂类型是将各个字段分别存在不同的域上的，但null和没有字段的类型（比如 new object()）除外，
             * 对于这类值，使用EntryNameForSpecialValue域存，其中，null使用CacheEnv.NullValueString作为值，
             * 没有字段的类型使用空字符串
             */

            shouldRemoveSpecialEntry = false;
            if (obj == null)
                return new[] { new HashEntry(EntryNameForSpecialValue, CacheEnv.NullValueString) };

            var type = obj.GetType();
            if (IsSimpleType(type))
            {
                var redisValue = ToRedisValue(obj);
                return new[] { new HashEntry(EntryNameForSpecialValue, redisValue) };
            }

            var members = TypeMemberCache.GetMembers(type);
            var count = members.Count;
            if (count == 0)
                return new[] { new HashEntry(EntryNameForSpecialValue, string.Empty) };

            var entries = new HashEntry[count];
            var index = 0;
            foreach (var member in members.Values)
            {
                var value = member.Getter(obj);
                var redisValue = ToRedisValue(value);
                var entry = new HashEntry(member.Name, redisValue);
                entries[index] = entry;
                index++;
            }

            shouldRemoveSpecialEntry = true;
            return entries;
        }

        /// <summary>
        /// 将<see cref="HashEntry"/>集合反序列化到指定对象的实例。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="entries">包含类型的各字段值的<see cref="HashEntry"/>集合。</param>
        /// <returns>对象的实例。</returns>
        public static T FromCacheEntries<T>(IEnumerable<HashEntry> entries)
        {
            return (T)FromCacheEntries(entries, typeof(T));
        }

        /// <summary>
        /// 将<see cref="HashEntry"/>集合反序列化到指定对象的实例。
        /// </summary>
        /// <param name="entries">包含类型的各字段值的<see cref="HashEntry"/>集合。</param>
        /// <param name="type">目标类型。</param>
        /// <returns>对象的实例。</returns>
        public static object FromCacheEntries(IEnumerable<HashEntry> entries, Type type)
        {
            if (IsSimpleType(type))
            {
                // 对于简单类型，其在hash上的值是直接存储在EntryNameForSpecialValue域上的
                foreach (var entry in entries)
                {
                    if (!EntryNameForSpecialValue.Equals(entry.Name))
                        continue;

                    var value = FromRedisValue(entry.Value, type);
                    return value;
                }

                // 没有找到EntryNameForSpecialValue域的情况，直接返回默认值
                return ReflectionUtils.GetDefaultValue(type);
            }

            // 对于 Nullable<> ，只需要处理器内在类型。
            // 如果值为 null ，会存储在 EntryNameForSpecialValue 域并直接结束处理过程。
            type = ReflectionUtils.GetUnderlyingType(type);

            using (var itor = entries.GetEnumerator())
            {
                if (!itor.MoveNext())
                    return ReflectionUtils.GetDefaultValue(type);

                /*
                 * 复杂类型是将各个字段分别存在不同的域上的，但null和没有字段的类型（比如 new object()）除外，
                 * 对于这类值，使用EntryNameForSpecialValue域存，其中，null使用CacheEnv.NullValueString作为值，
                 * 没有字段的类型使用空字符串；
                 * 同时，EntryNameForSpecialValue域与其他字段域是互斥的，即有此域时，不能有其他字段
                 */
                var entry = itor.Current;
                var isEmptyObject = false;
                if (entry.Name == EntryNameForSpecialValue)
                {
                    if (CacheEnv.NullValueString.Equals(entry.Value))
                        return null;

                    if (string.Empty.Equals(entry.Value))
                    {
                        isEmptyObject = true;
                    }
                }

                var ctor = ConstructorInvokerGenerator.CreateDelegate(type);
                var result = ctor();

                if (isEmptyObject)
                    return result;

                var members = TypeMemberCache.GetMembers(type);
                while (true)
                {
                    TypeMember member;
                    if (!members.TryGetValue(entry.Name, out member))
                        continue;

                    var value = FromRedisValue(entry.Value, member.Type);
                    member.Setter(result, value);

                    if (!itor.MoveNext())
                        break;

                    entry = itor.Current;
                }

                return result;
            }
        }

        /// <summary>
        /// 将<see cref="RedisValue"/>转换为指定的CLR类型的实例。
        /// </summary>
        /// <typeparam name="T">转换后的目标类型。</typeparam>
        /// <param name="redisValue">待转换的<see cref="RedisValue"/>。</param>
        /// <returns>转换后的值，类型为<typeparamref name="T"/>。</returns>
        public static T FromRedisValue<T>(RedisValue redisValue)
        {
            var v = FromRedisValue(redisValue, typeof(T));
            return (T)v;
        }

        /// <summary>
        /// 将<see cref="RedisValue"/>转换为指定的CLR类型的实例。
        /// </summary>
        /// <param name="redisValue">待转换的<see cref="RedisValue"/>。</param>
        /// <param name="type"></param>
        /// <returns>转换后的值，类型为<paramref name="type"/>。</returns>
        public static object FromRedisValue(RedisValue redisValue, Type type)
        {
            string stringValue = null;

            // null to Nullable<>
            if (ReflectionUtils.IsNullableType(type))
            {
                stringValue = redisValue;
                if (CacheEnv.NullValueString.Equals(stringValue))
                    return ReflectionUtils.GetDefaultValue(type);
            }

            var underlyingType = ReflectionUtils.GetUnderlyingType(type);
            var typeCode = Type.GetTypeCode(underlyingType);
            switch (typeCode)
            {
                case TypeCode.String:
                    stringValue = stringValue ?? redisValue;
                    return stringValue == CacheEnv.NullValueString ? null : stringValue;

                case TypeCode.Boolean:
                    return (bool)redisValue;

                // char 的处理需要和 ToRedisValue 方法一致，它和 RedisValue 内部定义的不一致。
                case TypeCode.Char:
                    stringValue = stringValue ?? redisValue;

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    return stringValue == CacheEnv.NullValueString || stringValue == null ? '\0' : stringValue[0];

                // 转换过程需和 ToRedisValue 方法匹配，故没有直接使用 IConvertible.ToXX 方法。
                case TypeCode.SByte:
                    return (sbyte)(int)redisValue;

                case TypeCode.Byte:
                    return (byte)(int)redisValue;

                case TypeCode.Int16:
                    return (short)(int)redisValue;

                case TypeCode.UInt16:
                    return (ushort)(int)redisValue;

                case TypeCode.Int32:
                    return (int)redisValue;

                case TypeCode.UInt32:
                    return (uint)(long)redisValue;

                case TypeCode.Int64:
                    return (long)redisValue;

                case TypeCode.UInt64:
                    return ulong.Parse(redisValue);

                case TypeCode.Single:
                    return (float)(double)redisValue;

                case TypeCode.Double:
                    return (double)redisValue;

                case TypeCode.Decimal:
                    return decimal.Parse(redisValue);

                case TypeCode.DateTime:
                    return new DateTime((long)redisValue);

                case TypeCode.DBNull:
                    return DBNull.Value;
            }

            if (underlyingType == typeof(DateTimeOffset))
                return StringToDateTimeOffset(redisValue);

            if (underlyingType == typeof(Guid))
                return new Guid((string)redisValue);

            if (underlyingType == typeof(byte[]))
                return ConvertToBinary(redisValue);

            stringValue = stringValue ?? redisValue;
            if (CacheEnv.NullValueString.Equals(stringValue))
                return ReflectionUtils.GetDefaultValue(underlyingType);

            // objects
            return JsonSerializer.Default.Deserialize(stringValue, type);
        }

        /// <summary>
        /// 将给定的对象转换为<see cref="RedisValue"/>。
        /// </summary>
        /// <param name="value">待转换的对象。</param>
        /// <returns>转换后的<see cref="RedisValue"/>。</returns>
        public static RedisValue ToRedisValue(object value)
        {
            if (value == null)
                return CacheEnv.NullValueString;

            /*
             * CLR数值类型在RedisValue中也以数值类型（整数或浮点）的方式处理，
             * 然而redis数值是有符号的64位，不能表示ulong和decimal中超出其可存储范围的部分，
             * 对于这部分类型，直接ToString处理，避免RedisValue将其作为数值处理从而导致值溢出或精度损失
             */

            // null 在上面处理掉了，这里对于 Nullable<> 直接处理其内部类型即可。
            var type = ReflectionUtils.GetUnderlyingType(value.GetType());
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.String:
                    return (string)value;

                case TypeCode.Boolean:
                    return (bool)value;

                // RedisValue 没有公开创建自 char 的方法，从其内部的 ToChar 方法可知道是作为 uint 处理的，
                // 如果直接转为 uint 则使用的是 .net 内码 UTF-16，这和 string 的存储字符集 UTF-8 不一致。
                // 这种不一致性导致存储的 char 被作为 string 读取出来时变成一段数字而不是一个字符，这很难受。
                // char 应当和 string 有更好的兼容性，既然 RedisValue 没有直接定义怎么从 char 创建值，
                // 这里就以 string 的兼容性为优先，将其作为字符串存储。
                case TypeCode.Char:
                    return value.ToString();

                // RedisValue 没有定义如 sbyte 和 byte 的转换运算符，且在不同版本有不同的实现，比如2.x版多了
                // float 转换运算符，1.x版则没有。这里将这些（可能）支持得不太好的类型都转成更长的、受支持的
                // 类型以避免这些兼容性问题，在各版本均被良好支持的数值类型有： int long double 。
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return Convert.ToInt32(value);

                case TypeCode.Int32:
                    return (int)value;

                case TypeCode.UInt32:
                    return Convert.ToInt64(value);

                case TypeCode.Int64:
                    return (long)value;

                case TypeCode.Single:
                    return Convert.ToDouble(value);

                case TypeCode.Double:
                    return (double)value;

                // decimal 未被直接支持，其可能超过 ulong 的最大值，直接以字符串形式处理。
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                    return value.ToString();

                case TypeCode.DateTime:
                    return ((DateTime)value).Ticks;

                case TypeCode.DBNull:
                    return string.Empty;
            }

            if (type == typeof(DateTimeOffset))
                return DateTimeOffsetToString((DateTimeOffset)value);

            // GUID是可以以二进制存储的，只需要16个字节。
            // 但考虑到数据易读性和各个使用场景下的兼容性，这里采用了字符串存储的方案。
            if (type == typeof(Guid))
                return ((Guid)value).ToString("N");

            // 二进制数据以原生形式存储。
            if (type == typeof(byte[]))
                return (byte[])value;

            return JsonSerializer.Default.FastSerialize(value);
        }

        /// <summary>
        /// 获取指定的CLR类型在Redis上存储的方式。
        /// </summary>
        /// <param name="type">CLR类型。</param>
        /// <returns>给定Redis上存储的方式。</returns>
        public static RedisDataType GetDataType(Type type)
        {
            if (ReflectionUtils.IsNullableType(type))
                return RedisDataType.Nullable;

            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.DBNull:
                    return RedisDataType.DBNull;

                case TypeCode.Boolean:
                    return RedisDataType.Boolean;

                case TypeCode.Char:
                    return RedisDataType.Char;

                case TypeCode.SByte:
                    return RedisDataType.SByte;

                case TypeCode.Byte:
                    return RedisDataType.Byte;

                case TypeCode.Int16:
                    return RedisDataType.Int16;

                case TypeCode.UInt16:
                    return RedisDataType.UInt16;

                case TypeCode.Int32:
                    return RedisDataType.Int32;

                case TypeCode.UInt32:
                    return RedisDataType.UInt32;

                case TypeCode.Int64:
                    return RedisDataType.Int64;

                case TypeCode.UInt64:
                    return RedisDataType.UInt64;

                case TypeCode.Single:
                    return RedisDataType.Single;

                case TypeCode.Double:
                    return RedisDataType.Double;

                case TypeCode.Decimal:
                    return RedisDataType.Decimal;

                case TypeCode.DateTime:
                    return RedisDataType.DateTime;

                case TypeCode.String:
                    return RedisDataType.String;
            }

            if (type == typeof(DateTimeOffset))
                return RedisDataType.DateTimeOffset;

            if (type == typeof(Guid))
                return RedisDataType.Guid;

            if (type == typeof(byte[]))
                return RedisDataType.Binary;

            return RedisDataType.Object;
        }

        /// <summary>
        /// 判断指定的类型的实例存储在redis上时，是否不需要经过复杂（通常是JSON）的序列化。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <returns>true若不需要经过复杂（通常是JSON）的序列化；否则为false。</returns>
        public static bool IsSimpleType(Type type)
        {
            var dataType = GetDataType(type);

            switch (dataType)
            {
                case RedisDataType.Object:
                    return false;

                case RedisDataType.Nullable:
                    var underlyingType = ReflectionUtils.GetUnderlyingType(type);
                    dataType = GetDataType(underlyingType);
                    return dataType != RedisDataType.Object;

                default:
                    return true;
            }
        }

        /// <summary>
        /// 在对一个值进行 INCR* 操作后，判断返回的值是否是新建的。
        /// </summary>
        /// <param name="result">INCR* 的返回值。</param>
        /// <param name="increment">增量，必须是整数，或能够转换为整数。使用负数来做减法。</param>
        /// <returns>true若值为新建的；否则表明值原来是存在的，INCR更新了该值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNewlyCreatedAfterIncreasing(long result, long increment)
        {
            // redis 的 INCR* 命令本身没有提供在新建值的同时设置过期时间的机制，
            // 附带超时时间时，需要先 INCR* 再设置过期时间。
            // 然而 INCR* 仅返回 INCR* 后的值，并不说明值是刚新建的还是原有值更新的，
            // 我们只能在 INCR* 后的值与增量一致时，推断其是新建的。
            return result == increment;
        }

        /// <summary>
        /// 若异常表示在redis上对一个无效的值进行了加减（INCR）操作，返回<see cref="InvalidCastException"/>，
        /// 并以给定的异常作为内部异常；否则返回null。
        /// </summary>
        public static Exception TryConvertExceptionForNumberIncrementOnInvalidValue(Exception raw)
        {
            // 对无效值进行 INCR 操作时，会得到redis服务返回的错误，总是以 RedisServerException 表示的，
            // redis没有定义具体的错误码，只能通过错误消息的内容来判断，此错误的消息为：
            // “value is not an integer or out of range”或“hash value is not an integer”
            const string redisServerExceptionMessageTrait = "not an integer";

            if (raw is AggregateException aggEx)
            {
                raw = aggEx.InnerExceptions[0];
            }

            if (!(raw is RedisServerException redisServerException))
                return raw;

            if (!redisServerException.Message.Contains(redisServerExceptionMessageTrait))
                return raw;

            return new InvalidCastException("The value is not an integer or out of range.", raw);
        }

        private static byte[] ConvertToBinary(RedisValue value)
        {
            if (!value.HasValue)
                return new byte[0];

            var bin = (byte[])value;
            return IsBinaryNullValue(bin) ? null : bin;
        }

        // 比较redis上的值是否表示CLR null。
        // redis不能直接存储null，null是使用一个特殊的值（CacheEnv.NullValueString）替代的，
        // 此方法逐字节比较给定数据是否刚好是这个表示null的值。
        private static bool IsBinaryNullValue(byte[] value)
        {
            if (value == null)
                return true;

            var len = value.Length;
            if (len != CacheEnv.NullValueString.Length)
                return false;

            for (int i = 0; i < len; i++)
            {
                if (value[i] != CacheEnv.NullValueString[i])
                    return false;
            }

            return true;
        }

        private static string DateTimeOffsetToString(DateTimeOffset d)
        {
            var s = string.Concat(d.Ticks, "+", d.Offset.Ticks);
            return s;
        }

        private static DateTimeOffset StringToDateTimeOffset(string v)
        {
            var idx = v.IndexOf("+", StringComparison.Ordinal);
            if (idx < 0)
                throw new FormatException("Bad format for a DateTimeOffset value.");

            var ticks = v.Substring(0, idx);
            var offset = v.Substring(idx, v.Length - idx);
            return new DateTimeOffset(long.Parse(ticks), TimeSpan.FromTicks(long.Parse(offset)));
        }

        private static class TypeMemberCache
        {
            private static readonly ConcurrentDictionary<Type, Dictionary<string, TypeMember>> Cache
                = new ConcurrentDictionary<Type, Dictionary<string, TypeMember>>();

            public static Dictionary<string, TypeMember> GetMembers(Type type)
            {
                return Cache.GetOrAdd(type, Create);
            }

            private static Dictionary<string, TypeMember> Create(Type type)
            {
                var memberMap = new Dictionary<string, TypeMember>();

                var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var getter = PropertyAccessorGenerator.CreateGetter(prop);
                    var setter = PropertyAccessorGenerator.CreateSetter(prop);
                    var typeMember = new TypeMember
                    {
                        Name = prop.Name,
                        Type = prop.PropertyType,
                        Getter = getter,
                        Setter = setter
                    };
                    memberMap.Add(prop.Name, typeMember);
                }

                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var getter = FieldAccessorGenerator.CreateGetter(field);
                    var setter = FieldAccessorGenerator.CreateSetter(field);
                    var typeMember = new TypeMember
                    {
                        Name = field.Name,
                        Type = field.FieldType,
                        Getter = getter,
                        Setter = setter
                    };
                    memberMap.Add(field.Name, typeMember);
                }

                return memberMap;
            }
        }

        private class TypeMember
        {
            public Action<object, object> Setter;
            public Func<object, object> Getter;
            public string Name;
            public Type Type;
        }
    }
}
