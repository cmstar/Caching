﻿using System;
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
