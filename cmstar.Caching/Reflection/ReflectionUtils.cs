using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace cmstar.Caching.Reflection
{
    /// <summary>
    /// �������ͷ����ж���صķ�����
    /// </summary>
    public static class ReflectionUtils
    {
        private static readonly ConcurrentDictionary<Type, object> DefaultValues
            = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// �жϸ��������Ƿ�ɱ���ֵΪnull��
        /// </summary>
        /// <param name="t">���͡�</param>
        /// <returns>������Ϊ�������ͣ�����<c>true</c>�����򷵻�<c>false</c>��</returns>
        /// <exception cref="ArgumentNullException">��<paramref name="t"/>Ϊ<c>null</c>��</exception>
        public static bool IsNullable(Type t)
        {
            ArgAssert.NotNull(t, "t");
            return !t.IsValueType || IsNullableType(t);
        }

        /// <summary>
        /// �жϸ��������Ƿ���<see cref="System.Nullable{T}"/>��ʵ����
        /// </summary>
        /// <param name="t">���͡�</param>
        /// <returns>������Ϊ<see cref="System.Nullable{T}"/>������<c>true</c>�����򷵻�<c>false</c>��</returns>
        /// <exception cref="ArgumentNullException">��<paramref name="t"/>Ϊ<c>null</c>��</exception>
        public static bool IsNullableType(Type t)
        {
            ArgAssert.NotNull(t, "t");
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// ��ȡ���ͻ������ڰ����Ĳ��ɿ����ͣ�������Ϊ<see cref="System.Nullable{T}"/>ʱ����
        /// </summary>
        /// <param name="t">���͡�</param>
        /// <returns>�������ͣ�������Ϊ<see cref="System.Nullable{T}"/>���������ڲ����͡�</returns>
        /// <exception cref="ArgumentNullException">��<paramref name="t"/>Ϊ<c>null</c>��</exception>
        public static Type GetUnderlyingType(Type t)
        {
            return IsNullableType(t) ? Nullable.GetUnderlyingType(t) : t;
        }

        /// <summary>
        /// �жϸ��������Ƿ���Ŀ�����ͻ���Ŀ�����͵����ࡣ
        /// </summary>
        /// <param name="thisType">���ж����͡�</param>
        /// <param name="targetType">Ŀ�����͡�</param>
        /// <returns>������������Ŀ�����ͻ���Ŀ�����͵����࣬����<c>true</c>�����򷵻�<c>false</c>��</returns>
        /// <exception cref="ArgumentNullException">��<paramref name="thisType"/>Ϊ<c>null</c>��</exception>
        /// <exception cref="ArgumentNullException">��<paramref name="targetType"/>Ϊ<c>null</c>��</exception>
        public static bool IsOrIsSubClassOf(Type thisType, Type targetType)
        {
            ArgAssert.NotNull(thisType, "thisType");
            ArgAssert.NotNull(targetType, "targetType");

            return thisType == targetType || thisType.IsSubclassOf(targetType);
        }

        /// <summary>
        /// ��ȡLambda����ʽ��ָ��ĳ�Ա���ƣ���Ա���������Ի���
        /// </summary>
        /// <param name="memberAccessor">��Ա���ʱ���ʽ��</param>
        /// <returns>��Ա���ơ�</returns>
        /// <exception cref="ArgumentNullException">��<paramref name="memberAccessor"/>Ϊ<c>null</c>��</exception>
        /// <exception cref="ArgumentException">
        /// ��<paramref name="memberAccessor"/>��<see cref="LambdaExpression.Body"/>�������Է��ʡ�
        /// </exception>
        public static string GetMemberName<T>(Expression<Func<T, object>> memberAccessor)
        {
            ArgAssert.NotNull(memberAccessor, "memberAccessor");

            var body = memberAccessor.Body;

            if (body is UnaryExpression)
                body = ((UnaryExpression)memberAccessor.Body).Operand;

            if (!(body is MemberExpression))
                throw new ArgumentException(
                    "Need member-access expression, current:" + memberAccessor, "memberAccessor");

            return ((MemberExpression)body).Member.Name;
        }

        /// <summary>
        /// ��ȡLambda����ʽ��ָ��ĳ�Ա���ƣ���Ա���������Ի���
        /// </summary>
        /// <param name="memberAccessor">��Ա���ʱ���ʽ��</param>
        /// <returns>��Ա���ơ�</returns>
        /// <exception cref="ArgumentNullException">��<paramref name="memberAccessor"/>Ϊ<c>null</c>��</exception>
        /// <exception cref="ArgumentException">
        /// ��<paramref name="memberAccessor"/>��<see cref="LambdaExpression.Body"/>�������Է��ʡ�
        /// </exception>
        public static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> memberAccessor)
        {
            ArgAssert.NotNull(memberAccessor, "memberAccessor");

            var body = memberAccessor.Body;

            if (body is UnaryExpression)
                body = ((UnaryExpression)memberAccessor.Body).Operand;

            if (!(body is MemberExpression))
                throw new ArgumentException(
                    "Need member-access expression, current: " + memberAccessor, "memberAccessor");

            return ((MemberExpression)body).Member.Name;
        }

        /// <summary>
        /// �Ӹ����������Ͻ������Ͳ���������ָ����������ʱ��ʹ�õķ������Ͷ��塣
        /// </summary>
        /// <param name="type">�Ӵ������Ͻ������Ͳ�������</param>
        /// <param name="genericTypeDefinition">�������Ͷ��塣�����ǽӿڣ�Ҳ�����Ƿǽӿ����͡�</param>
        /// <returns>���͵ķ��Ͳ��������飬��Ԫ��˳�������Ͷ����˳��һ�¡�</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/>Ϊ<c>null</c>��</exception>
        /// <exception cref="ArgumentNullException"><paramref name="genericTypeDefinition"/>Ϊ<c>null</c>��</exception>
        /// <exception cref="ArgumentException"><paramref name="genericTypeDefinition"/>���Ƿ������Ͷ��塣</exception>
        /// <example>
        /// һ�����Ϳ��Լ̳ж���������ͣ����磺
        /// GenericClass{T} : IDictionary{T, int}, ICollection{string}
        /// ��ʱ������GenericClass{long}��ʹ�÷��Ͷ���IDictionary{,}����ȡ������������ [long, int]��
        /// �����ڷ��Ͷ���ICollection{}������ [string]��
        /// </example>
        public static Type[] GetGenericArguments(Type type, Type genericTypeDefinition)
        {
            ArgAssert.NotNull(type, "type");
            ArgAssert.NotNull(genericTypeDefinition, "genericTypeDefinition");

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                var msg = string.Format(
                    "The type {0} is not a generic type definition.",
                    genericTypeDefinition.Name);
                throw new ArgumentException(msg, "genericTypeDefinition");
            }

            if (genericTypeDefinition.IsInterface)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return type.GetGenericArguments();

                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                        continue;

                    if (interfaceType.GetGenericTypeDefinition() != genericTypeDefinition)
                        continue;

                    return interfaceType.GetGenericArguments();
                }
            }
            else
            {
                var baseType = type;
                do
                {
                    if (!baseType.IsGenericType)
                        continue;

                    if (baseType.GetGenericTypeDefinition() != genericTypeDefinition)
                        continue;

                    return baseType.GetGenericArguments();

                } while ((baseType = baseType.BaseType) != null);
            }

            return null;
        }

        /// <summary>
        /// �жϸ����������Ƿ����������͡�
        /// </summary>
        /// <param name="type">���жϵ����͡�</param>
        /// <returns>true���������������������ͣ�����Ϊfalse��</returns>
        public static bool IsAnonymousType(Type type)
        {
            ArgAssert.NotNull(type, "type");

            if (!type.IsGenericType)
                return false;

            if (!Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false))
                return false;

            if ((type.Attributes & TypeAttributes.NotPublic) != TypeAttributes.NotPublic)
                return false;

            return type.Name.Contains("AnonymousType");
        }

        /// <summary>
        /// ��ȡָ�����͵�Ĭ��ֵ��Ч��ͬdefault(T)�������
        /// �����������ͣ�Ĭ��ֵΪnull������ֵ������Ϊ����ֵ��
        /// </summary>
        /// <param name="type">���͡�</param>
        /// <returns>���͵�Ĭ��ֵ��</returns>
        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
                return null;

            object value;
            if (!DefaultValues.TryGetValue(type, out value))
            {
                value = Activator.CreateInstance(type);
                DefaultValues.TryAdd(type, value);
            }

            return value;
        }
    }
}