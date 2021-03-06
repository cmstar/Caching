using System;
using System.Collections.Concurrent;
using cmstar.RapidReflection.Emit;

namespace cmstar.Caching.Reflection
{
    /// <summary>
    /// 提供了获取类型成员访问方法相关的方法。
    /// </summary>
    public static class TypeMemberAccessorUtils
    {
        private static readonly ConcurrentDictionary<TypeMemberIndentity, Func<object, object>> Getters
            = new ConcurrentDictionary<TypeMemberIndentity, Func<object, object>>();

        private static readonly ConcurrentDictionary<TypeMemberIndentity, Action<object, object>> Setters
            = new ConcurrentDictionary<TypeMemberIndentity, Action<object, object>>();

        /// <summary>
        /// 获取指定类型上指定名称的成员值的访问器。
        /// 成员可以是属性或域，通过名称搜索成员时，属性的优先级高于域。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="memberName">属性或域的名称。</param>
        /// <returns>成员值的访问器。</returns>
        public static Func<object, object> GetGetAccessor(Type type, string memberName)
        {
            ArgAssert.NotNull(type, "type");
            ArgAssert.NotNull(memberName, "memberName");

            var id = new TypeMemberIndentity(type, memberName);
            Func<object, object> res;

            if (Getters.TryGetValue(id, out res))
                return res;

            res = GenerateGetAccessor(type, memberName);
            Getters.TryAdd(id, res);
            return res;
        }

        /// <summary>
        /// 获取指定类型上指定名称的成员值的设置器。
        /// 成员可以是属性或域，通过名称搜索成员时，属性的优先级高于域。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="memberName">属性或域的名称。</param>
        /// <returns>成员值的设置器。</returns>
        public static Action<object, object> GetSetAccessor(Type type, string memberName)
        {
            ArgAssert.NotNull(type, "type");
            ArgAssert.NotNull(memberName, "memberName");

            var id = new TypeMemberIndentity(type, memberName);
            Action<object, object> res;

            if (Setters.TryGetValue(id, out res))
                return res;

            res = GenerateSetAccessor(type, memberName);
            Setters.TryAdd(id, res);
            return res;
        }

        private static Action<object, object> GenerateSetAccessor(Type type, string memberName)
        {
            var propInfo = type.GetProperty(memberName);
            if (propInfo != null)
                return PropertyAccessorGenerator.CreateSetter(propInfo);

            var fieldInfo = type.GetField(memberName);
            if (fieldInfo != null)
                return FieldAccessorGenerator.CreateSetter(fieldInfo);

            return null;
        }

        private static Func<object, object> GenerateGetAccessor(Type type, string memberName)
        {
            var propInfo = type.GetProperty(memberName);
            if (propInfo != null)
                return PropertyAccessorGenerator.CreateGetter(propInfo);

            var fieldInfo = type.GetField(memberName);
            if (fieldInfo != null)
                return FieldAccessorGenerator.CreateGetter(fieldInfo);

            return null;
        }
    }
}
