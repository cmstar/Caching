using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 包含参数验证的有关方法。
    /// </summary>
    internal static class ArgAssert
    {
        /// <summary>
        /// 断言参数非空引用。
        /// </summary>
        /// <param name="arg">参数实例。</param>
        /// <param name="name">参数名称。</param>
        public static void NotNull(object arg, string name)
        {
            if (arg == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        /// 断言字符串类型的参数非空引用或空字符串。
        /// </summary>
        /// <param name="value">字符串实例。</param>
        /// <param name="parameterName">参数名称。</param>
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            if (value.Length == 0)
                throw new ArgumentException(
                    string.Format("The parameter '{0}' cannot be null.", parameterName));
        }

        /// <summary>
        /// 断言字符串类型的参数非空引用或空字符串或只包含空白字符。
        /// </summary>
        /// <param name="value">字符串实例。</param>
        /// <param name="parameterName">参数名称。</param>
        public static void NotNullOrEmptyOrWhitespace(string value, string parameterName)
        {
            NotNullOrEmpty(value, parameterName);

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    string.Format("The parameter '{0}' cannot be null or whitespace.", parameterName));
        }
    }
}
