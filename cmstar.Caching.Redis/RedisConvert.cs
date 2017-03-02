using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
        public static object FromRedisValue<T>(RedisValue redisValue)
        {
            return FromRedisValue(redisValue, typeof(T));
        }

        /// <summary>
        /// 将<see cref="RedisValue"/>转换为指定的CLR类型的实例。
        /// </summary>
        /// <param name="redisValue">待转换的<see cref="RedisValue"/>。</param>
        /// <param name="type"></param>
        /// <returns>转换后的值，类型为<paramref name="type"/>。</returns>
        public static object FromRedisValue(RedisValue redisValue, Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.String:
                    var v = (string)redisValue;
                    return v == CacheEnv.NullValueString ? null : v;

                case TypeCode.Boolean:
                    return (bool)redisValue;

                case TypeCode.Char:
                    return (char)redisValue;

                case TypeCode.SByte:
                    return (SByte)redisValue;

                case TypeCode.Byte:
                    return (byte)redisValue;

                case TypeCode.Int16:
                    return (short)redisValue;

                case TypeCode.UInt16:
                    return (ushort)redisValue;

                case TypeCode.Int32:
                    return (int)redisValue;

                case TypeCode.UInt32:
                    return (uint)redisValue;

                case TypeCode.Int64:
                    return long.Parse(redisValue);

                case TypeCode.UInt64:
                    return ulong.Parse(redisValue);

                case TypeCode.Single:
                    return float.Parse(redisValue);

                case TypeCode.Double:
                    return (double)redisValue;

                case TypeCode.Decimal:
                    return decimal.Parse(redisValue);

                case TypeCode.DateTime:
                    return new DateTime(long.Parse(redisValue));

                case TypeCode.DBNull:
                    return DBNull.Value;
            }

            if (type == typeof(DateTimeOffset))
                return StringToDateTimeOffset(redisValue);

            if (type == typeof(Guid))
                return new Guid((string)redisValue);

            var stringValue = redisValue;
            if (CacheEnv.NullValueString.Equals(stringValue))
                return ReflectionUtils.GetDefaultValue(type);

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
            var type = value.GetType();
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.String:
                    return (string)value;

                case TypeCode.Boolean:
                    return (bool)value;

                case TypeCode.Char:
                    return (char)value;

                case TypeCode.SByte:
                    return (SByte)value;

                case TypeCode.Byte:
                    return (byte)value;

                case TypeCode.Int16:
                    return (short)value;

                case TypeCode.UInt16:
                    return (ushort)value;

                case TypeCode.Int32:
                    return (int)value;

                case TypeCode.UInt32:
                    break;
                case TypeCode.Int64:
                    return (long)value;

                case TypeCode.UInt64:
                    return ((ulong)value).ToString(CultureInfo.InvariantCulture);

                case TypeCode.Single:
                    return (float)value;

                case TypeCode.Double:
                    return (double)value;

                case TypeCode.Decimal:
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture);

                case TypeCode.DateTime:
                    return ((DateTime)value).Ticks;

                case TypeCode.DBNull:
                    return String.Empty;
            }

            if (type == typeof(DateTimeOffset))
                return DateTimeOffsetToString((DateTimeOffset)value);

            if (type == typeof(Guid))
                return ((Guid)value).ToString("N");

            return JsonSerializer.Default.FastSerialize(value);
        }

        /// <summary>
        /// 获取指定的CLR类型在Redis上存储的方式。
        /// </summary>
        /// <param name="type">CLR类型。</param>
        /// <returns>给定Redis上存储的方式。</returns>
        public static RedisDataType GetDataType(Type type)
        {
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
            return dataType != RedisDataType.Object;
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

        private class TypeMemberCache
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
