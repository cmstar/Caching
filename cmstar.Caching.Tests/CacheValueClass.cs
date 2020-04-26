using System;
using System.Collections.Generic;
using cmstar.Serialization.Json;

namespace cmstar.Caching
{
    public class CacheValueClass : IEquatable<CacheValueClass>, ICloneable
    {
        private static readonly CacheValueClass Sample = new CacheValueClass
        {
            DateTimeField = DateTime.Now,
            LongField = 589962545522,
            IntField = 55,
            ShortField = 12345,
            ByteField = 33,
            StringField = "abc",
            StructField = CacheValueStruct.CloneSample(),
            ArrayField = new[] { -55, 0, 0, 11 }
        };

        public static CacheValueClass CloneSample()
        {
            return (CacheValueClass)Sample.Clone();
        }

        public string StringField;
        public long LongField;
        public int IntField;
        public short ShortField;
        public byte ByteField;
        public int? NullableField;
        public DateTime DateTimeField;
        public IList<int> ArrayField;
        public CacheValueStruct StructField;

        public bool Equals(CacheValueClass other)
        {
            if (other == null
                || other.StringField != StringField
                || other.LongField != LongField
                || other.IntField != IntField
                || other.ShortField != ShortField
                || other.ByteField != ByteField
                || other.NullableField != NullableField
                || other.DateTimeField != DateTimeField
                || !other.StructField.Equals(StructField))
            {
                return false;
            }

            if (other.ArrayField == null && ArrayField == null)
                return true;

            if (other.ArrayField == null || ArrayField == null)
                return false;

            if (other.ArrayField.Count != ArrayField.Count)
                return false;

            for (int i = 0; i < ArrayField.Count; i++)
            {
                if (other.ArrayField[i] != ArrayField[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is CacheValueClass other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = (StringField != null ? StringField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LongField.GetHashCode();
                hashCode = (hashCode * 397) ^ IntField;
                hashCode = (hashCode * 397) ^ ShortField.GetHashCode();
                hashCode = (hashCode * 397) ^ ByteField.GetHashCode();
                hashCode = (hashCode * 397) ^ NullableField.GetHashCode();
                hashCode = (hashCode * 397) ^ DateTimeField.GetHashCode();
                hashCode = (hashCode * 397) ^ StructField.GetHashCode();

                if (ArrayField != null)
                {
                    for (int i = 0; i < ArrayField.Count; i++)
                    {
                        hashCode = (hashCode * 397) ^ ArrayField[i].GetHashCode();
                    }
                }
                // ReSharper restore NonReadonlyMemberInGetHashCode

                return hashCode;
            }
        }

        public object Clone()
        {
            var json = JsonSerializer.Default.Serialize(this);
            var clone = JsonSerializer.Default.Deserialize(json, GetType());
            return clone;
        }

        public override string ToString()
        {
            return string.Format("{0}_{1}_{2}_[{3}]_({4})",
                StringField, IntField, DateTimeField,
                ArrayField == null ? "null" : ArrayField.Count.ToString(),
                StructField);
        }
    }
}
