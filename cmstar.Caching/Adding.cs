using System;

// ReSharper disable RedundantCast
namespace cmstar.Caching
{
    internal static class Adding
    {
        public static object Add(object x, object y)
        {
            if (x == null || y == null)
                throw new InvalidCastException();

            var typeCodeX = Type.GetTypeCode(x.GetType());
            var typeCodeY = Type.GetTypeCode(y.GetType());
            var targetTypeCode = (TypeCode)Math.Max((int)typeCodeX, (int)typeCodeY);

            switch (targetTypeCode)
            {
                case TypeCode.SByte: return AddSByte(x, y);
                case TypeCode.Byte: return AddByte(x, y);
                case TypeCode.Int16: return AddInt16(x, y);
                case TypeCode.UInt16: return AddUInt16(x, y);
                case TypeCode.Int32: return AddInt32(x, y);
                case TypeCode.UInt32: return AddUInt32(x, y);
                case TypeCode.Int64: return AddInt64(x, y);
                case TypeCode.UInt64: return AddUInt64(x, y);
                case TypeCode.Single: return AddSingle(x, y);
                case TypeCode.Double: return AddDouble(x, y);
                case TypeCode.Decimal: return AddDecimal(x, y);
                default: throw new InvalidCastException("Unsupported type.");
            }
        }

        private static object AddSByte(object x, object y)
        {
            var nx = Convert.ToSByte(x);
            var ny = Convert.ToSByte(y);
            return (sbyte)(nx + ny);
        }

        private static object AddByte(object x, object y)
        {
            var nx = Convert.ToByte(x);
            var ny = Convert.ToByte(y);
            return (byte)(nx + ny);
        }

        private static object AddInt16(object x, object y)
        {
            var nx = Convert.ToInt16(x);
            var ny = Convert.ToInt16(y);
            return (short)(nx + ny);
        }

        private static object AddUInt16(object x, object y)
        {
            var nx = Convert.ToUInt16(x);
            var ny = Convert.ToUInt16(y);
            return (ushort)(nx + ny);
        }

        private static object AddInt32(object x, object y)
        {
            var nx = Convert.ToInt32(x);
            var ny = Convert.ToInt32(y);
            return (int)(nx + ny);
        }

        private static object AddUInt32(object x, object y)
        {
            var nx = Convert.ToUInt32(x);
            var ny = Convert.ToUInt32(y);
            return (uint)(nx + ny);
        }

        private static object AddInt64(object x, object y)
        {
            var nx = Convert.ToInt64(x);
            var ny = Convert.ToInt64(y);
            return (long)(nx + ny);
        }

        private static object AddUInt64(object x, object y)
        {
            var nx = Convert.ToUInt64(x);
            var ny = Convert.ToUInt64(y);
            return (ulong)(nx + ny);
        }

        private static object AddSingle(object x, object y)
        {
            var nx = Convert.ToSingle(x);
            var ny = Convert.ToSingle(y);
            return (float)(nx + ny);
        }

        private static object AddDouble(object x, object y)
        {
            var nx = Convert.ToDouble(x);
            var ny = Convert.ToDouble(y);
            return (double)(nx + ny);
        }

        private static object AddDecimal(object x, object y)
        {
            var nx = Convert.ToDecimal(x);
            var ny = Convert.ToDecimal(y);
            return (decimal)(nx + ny);
        }
    }
}
