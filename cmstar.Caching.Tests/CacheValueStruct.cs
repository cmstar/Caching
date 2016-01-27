using System;

namespace cmstar.Caching
{
    public struct CacheValueStruct : IEquatable<CacheValueStruct>
    {
        private static readonly CacheValueStruct Sample = new CacheValueStruct
        {
            D = 15.988M,
            S = "string",
            F = 3.14F,
            G = Guid.NewGuid()
        };

        public static CacheValueStruct CloneSample()
        {
            return Sample;
        }

        public string S;
        public float F;
        public decimal D;
        public Guid G;

        public bool Equals(CacheValueStruct other)
        {
            return other.S == S
                && other.D == D
                && other.F - F <= float.Epsilon
                && other.G == G;
        }

        public override string ToString()
        {
            return string.Format("{0}_{1}_{2}_{3:N}", S, F, D, G);
        }
    }
}
