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
        /*
         * 当对象不能被拆解为键值对存储在hash上时，仍要使用hash存储，则将字段序列化（为JSON），
         * 以单一键值的形式存储，此时hash仅包含此字段。使用一个特殊的名称表示此字段
         */
        private const string EntryNameForNonObjects = "$<>__value";

        /// <summary>
        /// 将对象序列化到<see cref="HashEntry"/>的集合。
        /// </summary>
        /// <param name="obj">被序列化的对象。</param>
        /// <returns><see cref="HashEntry"/>的集合。</returns>
        public static HashEntry[] ToHashEntries(object obj)
        {
            if (obj == null)
                return new[] { new HashEntry(EntryNameForNonObjects, CacheEnv.NullValueString) };

            var type = obj.GetType();
            if (IsSimpleType(type))
            {
                var redisValue = ToRedisValue(obj);
                return new[] { new HashEntry(EntryNameForNonObjects, redisValue) };
            }

            var members = TypeMemberCache.GetMembers(type);
            var entries = new HashEntry[members.Count];
            var index = 0;

            foreach (var member in members.Values)
            {
                var value = member.Getter(obj);
                var redisValue = ToRedisValue(value);
                var entry = new HashEntry(member.Name, redisValue);
                entries[index] = entry;
                index++;
            }

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
                foreach (var entry in entries)
                {
                    if (((string)entry.Name) != EntryNameForNonObjects)
                        continue;

                    var value = FromRedisValue(entry.Value, type);
                    return value;
                }
            }

            var ctor = ConstructorInvokerGenerator.CreateDelegate(type);
            var result = ctor();
            var members = TypeMemberCache.GetMembers(type);

            foreach (var entry in entries)
            {
                TypeMember member;
                if (!members.TryGetValue(entry.Name, out member))
                    continue;

                var value = FromRedisValue(entry.Value, member.Type);
                member.Setter(result, value);
            }

            return result;
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
                    return (long)redisValue;

                case TypeCode.UInt64:
                    return (ulong)redisValue;

                case TypeCode.Single:
                    return (float)redisValue;

                case TypeCode.Double:
                    return (double)redisValue;

                case TypeCode.Decimal:
                    return Decimal.Parse(redisValue);

                case TypeCode.DateTime:
                    return new DateTime((long)redisValue);

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
        /// 将<see cref="RedisValue"/>转换为指定的CLR类型的实例。
        /// </summary>
        /// <typeparam name="T">转换后的目标类型。</typeparam>
        /// <param name="dataType">指定值是以何种方式存放在Redis上的。</param>
        /// <param name="redisValue">待转换的<see cref="RedisValue"/>。</param>
        /// <returns>转换后的值。</returns>
        public static object FromRedisValue<T>(RedisDataType dataType, RedisValue redisValue)
        {
            switch (dataType)
            {
                case RedisDataType.String:
                    var v = (string)redisValue;
                    return v == CacheEnv.NullValueString ? null : v;

                case RedisDataType.Boolean:
                    return (bool)redisValue;

                case RedisDataType.Char:
                    return (char)redisValue;

                case RedisDataType.SByte:
                    return (SByte)redisValue;

                case RedisDataType.Byte:
                    return (byte)redisValue;

                case RedisDataType.Int16:
                    return (short)redisValue;

                case RedisDataType.UInt16:
                    return (ushort)redisValue;

                case RedisDataType.Int32:
                    return (int)redisValue;

                case RedisDataType.UInt32:
                    return (uint)redisValue;

                case RedisDataType.Int64:
                    return (long)redisValue;

                case RedisDataType.UInt64:
                    return (ulong)redisValue;

                case RedisDataType.Single:
                    return (float)redisValue;

                case RedisDataType.Double:
                    return (double)redisValue;

                case RedisDataType.Decimal:
                    return Decimal.Parse(redisValue);

                case RedisDataType.DateTime:
                    return new DateTime((long)redisValue);

                case RedisDataType.DateTimeOffset:
                    return StringToDateTimeOffset(redisValue);

                case RedisDataType.DBNull:
                    return DBNull.Value;

                case RedisDataType.Guid:
                    return new Guid((string)redisValue);

                default:
                    var stringValue = redisValue;
                    if (CacheEnv.NullValueString.Equals(stringValue))
                        return default(T);

                    return JsonSerializer.Default.Deserialize<T>(stringValue);
            }
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
                    return (ulong)value;

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
        /// 将给定的对象转换为<see cref="RedisValue"/>。
        /// </summary>
        /// <param name="dataType">指定值是以何种方式存放在Redis上的。</param>
        /// <param name="value">待转换的对象。</param>
        /// <returns>转换后的<see cref="RedisValue"/>。</returns>
        public static RedisValue ToRedisValue(RedisDataType dataType, object value)
        {
            if (value == null)
                return CacheEnv.NullValueString;

            switch (dataType)
            {
                case RedisDataType.String:
                    return (string)value;

                case RedisDataType.Boolean:
                    return (bool)value;

                case RedisDataType.Char:
                    return (char)value;

                case RedisDataType.SByte:
                    return (SByte)value;

                case RedisDataType.Byte:
                    return (byte)value;

                case RedisDataType.Int16:
                    return (short)value;

                case RedisDataType.UInt16:
                    return (ushort)value;

                case RedisDataType.Int32:
                    return (int)value;

                case RedisDataType.UInt32:
                    return (uint)value;

                case RedisDataType.Int64:
                    return (long)value;

                case RedisDataType.UInt64:
                    return (ulong)value;

                case RedisDataType.Single:
                    return (float)value;

                case RedisDataType.Double:
                    return (double)value;

                case RedisDataType.Decimal:
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture);

                case RedisDataType.DateTime:
                    return ((DateTime)value).Ticks;

                case RedisDataType.DateTimeOffset:
                    return DateTimeOffsetToString((DateTimeOffset)value);

                case RedisDataType.DBNull:
                    return String.Empty;

                case RedisDataType.Guid:
                    return ((Guid)value).ToString("N");

                default:
                    return JsonSerializer.Default.FastSerialize(value);
            }
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
