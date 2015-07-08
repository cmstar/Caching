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
        /// 用于表示null。
        /// </summary>
        public const string NullValue = "NIL*Zrx40vMTx23vnGxnA";

        /// <summary>
        /// 将<see cref="RedisValue"/>转换为指定的CLR类型的实例。
        /// </summary>
        /// <typeparam name="T">转换后的目标类型。</typeparam>
        /// <param name="dataType">指定值是以何种方式存放在Redis上的。</param>
        /// <param name="redisValue">待转换的<see cref="RedisValue"/>。</param>
        /// <returns>转换后的值。</returns>
        public static object FromRedisValue<T>(RedisCacheDataType dataType, RedisValue redisValue)
        {
            switch (dataType)
            {
                case RedisCacheDataType.String:
                    var v = (string)redisValue;
                    return v == NullValue ? null : v;

                case RedisCacheDataType.Boolean:
                    return (bool)redisValue;

                case RedisCacheDataType.Char:
                    return (char)redisValue;

                case RedisCacheDataType.SByte:
                    return (SByte)redisValue;

                case RedisCacheDataType.Byte:
                    return (byte)redisValue;

                case RedisCacheDataType.Int16:
                    return (short)redisValue;

                case RedisCacheDataType.UInt16:
                    return (ushort)redisValue;

                case RedisCacheDataType.Int32:
                    return (int)redisValue;

                case RedisCacheDataType.UInt32:
                    return (uint)redisValue;

                case RedisCacheDataType.Int64:
                    return (long)redisValue;

                case RedisCacheDataType.UInt64:
                    return (ulong)redisValue;

                case RedisCacheDataType.Single:
                    return (float)redisValue;

                case RedisCacheDataType.Double:
                    return (double)redisValue;

                case RedisCacheDataType.Decimal:
                    return Decimal.Parse(redisValue);

                case RedisCacheDataType.DateTime:
                    return new DateTime((long)redisValue);

                case RedisCacheDataType.DateTimeOffset:
                    return StringToDateTimeOffset(redisValue);

                case RedisCacheDataType.DBNull:
                    return DBNull.Value;

                case RedisCacheDataType.Guid:
                    return new Guid((string)redisValue);

                default:
                    var stringValue = redisValue;
                    if (NullValue.Equals(stringValue))
                        return null;

                    return JsonSerializer.Default.Deserialize<T>(stringValue);
            }
        }

        /// <summary>
        /// 将给定的对象转换为<see cref="RedisValue"/>。
        /// </summary>
        /// <param name="dateType">指定值是以何种方式存放在Redis上的。</param>
        /// <param name="value">待转换的对象。</param>
        /// <returns>转换后的<see cref="RedisValue"/>。</returns>
        public static RedisValue ToRedisValue(RedisCacheDataType dateType, object value)
        {
            if (value == null)
                return NullValue;

            switch (dateType)
            {
                case RedisCacheDataType.String:
                    return (byte)value;

                case RedisCacheDataType.Boolean:
                    return (bool)value;

                case RedisCacheDataType.Char:
                    return (char)value;

                case RedisCacheDataType.SByte:
                    return (SByte)value;

                case RedisCacheDataType.Byte:
                    return (byte)value;

                case RedisCacheDataType.Int16:
                    return (short)value;

                case RedisCacheDataType.UInt16:
                    return (ushort)value;

                case RedisCacheDataType.Int32:
                    return (int)value;

                case RedisCacheDataType.UInt32:
                    return (uint)value;

                case RedisCacheDataType.Int64:
                    return (long)value;

                case RedisCacheDataType.UInt64:
                    return (ulong)value;

                case RedisCacheDataType.Single:
                    return (float)value;

                case RedisCacheDataType.Double:
                    return (double)value;

                case RedisCacheDataType.Decimal:
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture);

                case RedisCacheDataType.DateTime:
                    return ((DateTime)value).Ticks;

                case RedisCacheDataType.DateTimeOffset:
                    return DateTimeOffsetToString((DateTimeOffset)value);

                case RedisCacheDataType.DBNull:
                    return String.Empty;

                case RedisCacheDataType.Guid:
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
        public static RedisCacheDataType GetDataType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.DBNull:
                    return RedisCacheDataType.DBNull;

                case TypeCode.Boolean:
                    return RedisCacheDataType.Boolean;

                case TypeCode.Char:
                    return RedisCacheDataType.Char;

                case TypeCode.SByte:
                    return RedisCacheDataType.SByte;

                case TypeCode.Byte:
                    return RedisCacheDataType.Byte;

                case TypeCode.Int16:
                    return RedisCacheDataType.Int16;

                case TypeCode.UInt16:
                    return RedisCacheDataType.UInt16;

                case TypeCode.Int32:
                    return RedisCacheDataType.Int32;

                case TypeCode.UInt32:
                    return RedisCacheDataType.UInt32;

                case TypeCode.Int64:
                    return RedisCacheDataType.Int64;

                case TypeCode.UInt64:
                    return RedisCacheDataType.UInt64;

                case TypeCode.Single:
                    return RedisCacheDataType.Single;

                case TypeCode.Double:
                    return RedisCacheDataType.Double;

                case TypeCode.Decimal:
                    return RedisCacheDataType.Decimal;

                case TypeCode.DateTime:
                    return RedisCacheDataType.DateTime;

                case TypeCode.String:
                    return RedisCacheDataType.String;
            }

            if (type == typeof(DateTimeOffset))
                return RedisCacheDataType.DateTimeOffset;

            if (type == typeof(Guid))
                return RedisCacheDataType.Guid;

            return RedisCacheDataType.Object;
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
