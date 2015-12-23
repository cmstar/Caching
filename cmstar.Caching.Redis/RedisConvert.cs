using System;
using System.Globalization;
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
                        return null;

                    return JsonSerializer.Default.Deserialize<T>(stringValue);
            }
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
    }
}
