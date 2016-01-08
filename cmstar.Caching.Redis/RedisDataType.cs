namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 定义CLR类型在Redis上存储的形态。
    /// </summary>
    public enum RedisDataType
    {
        // 使用复杂序列化，使用JSON表示的类型
        Object,

        // 使用简单的序列化，能够用字符串表示的类型
        Guid,
        DateTime,
        DateTimeOffset,
        DBNull,
        Decimal,

        // 能够直接通过SE.Redis库存储的类型
        String,
        Boolean,
        Char,
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Single,
        Double,
    }
}
