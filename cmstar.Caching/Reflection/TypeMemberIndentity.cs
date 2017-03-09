using System;

namespace cmstar.Caching.Reflection
{
    internal struct TypeMemberIndentity : IEquatable<TypeMemberIndentity>
    {
        private readonly Type _type;
        private readonly string _memberName;

        public TypeMemberIndentity(Type type, string memberName)
        {
            _type = type;
            _memberName = memberName;
        }

        public bool Equals(TypeMemberIndentity other)
        {
            return other._type == _type && other._memberName == _memberName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeMemberIndentity))
                return false;

            return Equals((TypeMemberIndentity)obj);
        }

        public override int GetHashCode()
        {
            var hash = _type.GetHashCode() ^ _memberName.GetHashCode();
            return hash;
        }
    }
}
