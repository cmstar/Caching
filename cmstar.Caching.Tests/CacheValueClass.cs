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
            IntField = 55,
            StringField = "abc",
            StructField = CacheValueStruct.CloneSample(),
            ArrayField = new[] { -55, 0, 0, 11 }
        };

        public static CacheValueClass CloneSample()
        {
            return (CacheValueClass)Sample.Clone();
        }

        public string StringField;
        public int IntField;
        public DateTime DateTimeField;
        public IList<int> ArrayField;
        public CacheValueStruct StructField;

        public bool Equals(CacheValueClass other)
        {
            if (other == null
                || other.StringField != StringField
                || other.IntField != IntField
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
