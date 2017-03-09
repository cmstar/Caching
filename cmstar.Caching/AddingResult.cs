namespace cmstar.Caching
{
    /// <summary>
    /// 记录两数相加的结果。
    /// </summary>
    internal struct AddingResult
    {
        /// <summary>
        /// 相加后的结果，其类型可能经过转换，与加数/被加数的类型不一致。
        /// </summary>
        public object Value;

        /// <summary>
        /// <seealso cref="Value"/>的类型是否与被加数一致。
        /// </summary>
        public bool IsTargetType;

        /// <summary>
        /// <seealso cref="Value"/>的类型是否与加数一致。
        /// </summary>
        public bool IsIncrementType;
    }
}