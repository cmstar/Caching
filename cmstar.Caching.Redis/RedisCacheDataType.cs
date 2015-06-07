namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 定义CLR类型在Redis上存储的形态。
    /// </summary>
    public enum RedisCacheDataType
    {
        // 需要多做一次序列化、反序列化处理的类型
        Object,
        Guid,
        DateTime,
        DateTimeOffset,
        DBNull,
        Decimal,

        // 能够直接以原始形式存储的类型
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
