using System;

namespace cmstar.Caching
{
    public struct CacheValueStruct : IEquatable<CacheValueStruct>
    {
        private static readonly CacheValueStruct Sample = new CacheValueStruct
        {
            N = 2368893,
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
        public int N;
        public float F;
        public decimal D;
        public Guid G;

        public bool Equals(CacheValueStruct other)
        {
            return other.S == S
                && other.N == N
                && other.D == D
                && AboutEquals(other.F, F)
                && other.G == G;
        }

        public override string ToString()
        {
            return string.Format("N:{0}_S:{1}_F:{2}_D:{3}_G:{4:N}", N, S, F, D, G);
        }

        /// <summary>
        /// 判断两个浮点数是否（大概）相等。
        /// </summary>
        /// <remarks>
        /// 由于浮点数运算过程中可能产生的精度损失，经过一些列计算或转换（比如转成字符串再
        /// 转回来），同一个数在过程前后可能会有微小的误差。为了比较他们，我们可以认为，两个
        /// 数在一定的精度下相等则他们就是相等的。根据资料，<seealso cref="double"/>可以维持
        /// e+15的精度，所以我们判定两个数在此精度下相等即可。
        /// </remarks>
        /// <seealso cref="http://stackoverflow.com/questions/2411392/double-epsilon-for-equality-greater-than-less-than-less-than-or-equal-to-gre"/>
        private static bool AboutEquals(double x, double y)
        {
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1e-15;
            return Math.Abs(x - y) <= epsilon;
        }
    }
}
