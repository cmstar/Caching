using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 提供两个数字相加的方法，这些数字在编码时不能知道明确类型，需要在运行时判定。
    /// </summary>
    internal static class Adding
    {
        /// <summary>
        /// 计算两个整数的和。
        /// </summary>
        /// <param name="target">被加数。</param>
        /// <param name="increment">加数。</param>
        /// <returns></returns>
        public static AddingResult AddIntegers(object target, object increment)
        {
            if (target == null || increment == null)
                throw new InvalidCastException("Can not cast NULL to a number.");

            var typeCodeX = Type.GetTypeCode(target.GetType());
            var typeCodeY = Type.GetTypeCode(increment.GetType());

            if (typeCodeX == typeCodeY)
            {
                return new AddingResult
                {
                    Value = AddIntegersOfSameType(typeCodeX, target, increment),
                    IsTargetType = true,
                    IsIncrementType = true
                };
            }

            return AddIntegersOfDifferentTypes(typeCodeX, typeCodeY, target, increment);
        }

        private static AddingResult AddIntegersOfDifferentTypes(
            TypeCode targetTypeCode, TypeCode incrementTypeCode, object target, object increment)
        {
            object result;
            TypeCode resultTypeCode;

            // 与.net的标准规则一致，当两个不同类型的整数相加时：
            // 1 将小于4字节的数字，都统一到4个字节计算；
            // 2 将精度较小的数，转行为较大的数进行计算。
            var maxTypeCode = (TypeCode)Math.Max((int)targetTypeCode, (int)incrementTypeCode);
            switch (maxTypeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    result = Convert.ToInt32(target) + Convert.ToInt32(increment);
                    resultTypeCode = TypeCode.Int32;
                    break;

                case TypeCode.UInt32:
                case TypeCode.Int64:
                    result = Convert.ToInt64(target) + Convert.ToInt64(increment);
                    resultTypeCode = TypeCode.Int64;
                    break;

                case TypeCode.UInt64:
                    var targetLong = Convert.ToInt64(target);
                    if (targetLong < 0)
                    {
                        result = targetLong + Convert.ToInt64(increment);
                        resultTypeCode = TypeCode.Int64;
                    }
                    else
                    {
                        result = Convert.ToUInt64(target) + Convert.ToUInt64(increment);
                        resultTypeCode = TypeCode.UInt64;
                    }
                    break;

                default:
                    throw NotIntegerError();
            }

            return new AddingResult
            {
                Value = result,
                IsTargetType = targetTypeCode == resultTypeCode,
                IsIncrementType = incrementTypeCode == resultTypeCode
            };
        }

        private static object AddIntegersOfSameType(TypeCode typeCode, object x, object y)
        {
            // ReSharper disable BuiltInTypeReferenceStyle
            // ReSharper disable RedundantCast
            switch (typeCode)
            {
                case TypeCode.SByte: return (SByte)((SByte)x + (SByte)y);
                case TypeCode.Byte: return (Byte)((Byte)x + (Byte)y);
                case TypeCode.Int16: return (Int16)((Int16)x + (Int16)y);
                case TypeCode.UInt16: return (UInt16)((UInt16)x + (UInt16)y);
                case TypeCode.Int32: return (Int32)((Int32)x + (Int32)y);
                case TypeCode.UInt32: return (UInt32)((UInt32)x + (UInt32)y);
                case TypeCode.Int64: return (Int64)((Int64)x + (Int64)y);
                case TypeCode.UInt64: return (UInt64)((UInt64)x + (UInt64)y);
                default: throw NotIntegerError();
            }
            // ReSharper restore RedundantCast
            // ReSharper restore BuiltInTypeReferenceStyle
        }

        private static InvalidCastException NotIntegerError()
        {
            return new InvalidCastException("The value is not an integer.");
        }
    }
}
