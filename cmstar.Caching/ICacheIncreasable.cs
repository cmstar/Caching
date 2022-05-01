using System;

namespace cmstar.Caching
{
    /// <summary>
    /// ����һ�鷽�������ڶ���ֵ�͵Ļ���ֵ��������������
    /// </summary>
    public interface ICacheIncreasable : ICacheProvider
    {
        /// <summary>
        /// ��ָ�����Ļ������ʱ����һ�������ۼӵ��û���Ե�ֵ�ϡ�
        /// </summary>
        /// <typeparam name="T">�����������͡�</typeparam>
        /// <param name="key">����ļ���</param>
        /// <param name="increment">������ʹ�ø�������������</param>
        /// <returns>true�����Ѵ��ڵĻ�������������ֵ�����򷵻�false��</returns>
        bool Increase<T>(string key, T increment);

        /// <summary>
        /// ��ָ�����Ļ������ʱ����һ�������ۼӵ��û���Ե�ֵ�ϡ�
        /// </summary>
        /// <typeparam name="T">�����������͡�</typeparam>
        /// <param name="key">����ļ���</param>
        /// <param name="increment">������ʹ�ø�������������</param>
        /// <param name="result">���ɹ������˻���ֵʱ��������Ӻ��ֵ��</param>
        /// <returns>true�����Ѵ��ڵĻ�������������ֵ�����򷵻�false��</returns>
        bool IncreaseGet<T>(string key, T increment, out T result);

        /// <summary>
        /// ��ָ�����Ļ������ʱ����һ�������ۼӵ��û����ֵ�ϡ�
        /// �����治���ڣ����ʼ����ֵΪ����ֵ��
        /// </summary>
        /// <typeparam name="T">�����������͡�</typeparam>
        /// <param name="key">����ļ���</param>
        /// <param name="increment">������ʹ�ø�������������</param>
        /// <param name="expiration">
        /// ָ���������Ļ������Ĺ���ʱ�䡣ʹ��<see cref="TimeSpan.Zero"/>��ʾ���治����ڡ�
        /// </param>
        /// <returns>
        /// true�����Ѵ��ڵĻ�������������ֵ�����򷵻�false����ʱ�������µĻ������
        /// </returns>
        bool IncreaseCx<T>(string key, T increment, TimeSpan expiration);

        /// <summary>
        /// ��ָ�����Ļ������ʱ����һ�������ۼӵ��û����ֵ�ϡ�
        /// �����治���ڣ����ʼ����ֵΪ����ֵ��
        /// </summary>
        /// <typeparam name="T">�����������͡�</typeparam>
        /// <param name="key">����ļ���</param>
        /// <param name="increment">������ʹ�ø�������������</param>
        /// <param name="expiration">
        /// ָ���������Ļ������Ĺ���ʱ�䡣ʹ��<see cref="TimeSpan.Zero"/>��ʾ���治����ڡ�
        /// </param>
        /// <param name="result">������Ӻ��ֵ��</param>
        /// <returns>
        /// true�����Ѵ��ڵĻ�������������ֵ�����򷵻�false����ʱ�������µĻ������
        /// </returns>
        bool IncreaseGetCx<T>(string key, T increment, TimeSpan expiration, out T result);
    }
}