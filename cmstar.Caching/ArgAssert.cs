using System;

namespace cmstar.Caching
{
    /// <summary>
    /// ����������֤���йط�����
    /// </summary>
    internal static class ArgAssert
    {
        /// <summary>
        /// ���Բ����ǿ����á�
        /// </summary>
        /// <param name="arg">����ʵ����</param>
        /// <param name="name">�������ơ�</param>
        public static void NotNull(object arg, string name)
        {
            if (arg == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        /// �����ַ������͵Ĳ����ǿ����û���ַ�����
        /// </summary>
        /// <param name="value">�ַ���ʵ����</param>
        /// <param name="parameterName">�������ơ�</param>
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            if (value.Length == 0)
                throw new ArgumentException(
                    string.Format("The parameter '{0}' cannot be null.", parameterName));
        }

        /// <summary>
        /// �����ַ������͵Ĳ����ǿ����û���ַ�����ֻ�����հ��ַ���
        /// </summary>
        /// <param name="value">�ַ���ʵ����</param>
        /// <param name="parameterName">�������ơ�</param>
        public static void NotNullOrEmptyOrWhitespace(string value, string parameterName)
        {
            NotNullOrEmpty(value, parameterName);

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    string.Format("The parameter '{0}' cannot be null or whitespace.", parameterName));
        }
    }
}
