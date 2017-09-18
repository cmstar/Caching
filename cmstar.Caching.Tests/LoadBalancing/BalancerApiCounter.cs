using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace cmstar.Caching.LoadBalancing
{
    public class BalancerApiCounter
    {
        private readonly CacheBalancer _balancer;
        private readonly List<CountedCacheProvider> _providers = new List<CountedCacheProvider>();
        private readonly List<int> _weight = new List<int>();
        private double _weightSummation;

        public BalancerApiCounter(CacheBalancer balancerWithoutNodes, IEnumerable<int> weights)
        {
            _balancer = balancerWithoutNodes;

            foreach (var weight in weights)
            {
                AddNode(weight);
            }
        }

        public void Test(string apiName, Func<CountedCacheProvider, int> counterSelector, Action<ICacheProvider, string> act)
        {
            /*
             * 利用随机的key（使用GUID）调用指定的API，如果key够随机，那么两个缓存提供器的调用占比
             * 应接近 1：1。这里调用API足够多次后，统计这个占比，用于确定负载分配是否均匀。
             */

            const int times = 20000; // 调用API的次数，一般来说样本越多统计越准确。
            const double epsilon = 0.05; // 实际统计结果很难刚好和预期结果完全一致，需允许一个误差范围。

            Console.WriteLine($"Testing API '{apiName}'...");

            for (int i = 0; i < times; i++)
            {
                act(_balancer, Guid.NewGuid().ToString("N"));
            }

            // 获取每个提供器上的 counter（API调用次数计数），并计算它们的和。
            var len = _providers.Count;
            var counters = new int[len];
            var sum = 0D;
            for (int i = 0; i < len; i++)
            {
                var counter = counterSelector(_providers[i]);
                counters[i] = counter;
                sum += counter;
            }

            // 计算每个 counter 的比例。
            var proportions = new double[len];
            for (int i = 0; i < len; i++)
            {
                var proportion = counters[i] / sum;
                proportions[i] = proportion;
            }

            var des = GetDescription(counters, proportions);
            Assert.AreEqual(times, sum, $"sum!={times}. " + des);

            // 验证每个 counter 的比例，应接近缓存提供器的比重占比，允许误差范围在 epsilon 内。
            for (int i = 0; i < len; i++)
            {
                var proportion = proportions[i];
                var weightProportion = _weight[i] / _weightSummation;
                var minus = Math.Abs(proportion - weightProportion);

                Assert.Greater(epsilon, minus, $"proportion{i}, coutner:{proportion}, weight:{weightProportion}. " + des);
            }

            Console.WriteLine("Passed. " + des);
        }

        private void AddNode(int weight)
        {
            var countedProvider = new CountedCacheProvider(new EmptyCacheProvider());
            _balancer.AddNode(countedProvider, weight);
            _providers.Add(countedProvider);
            _weight.Add(weight);
            _weightSummation += weight;
        }

        private static string GetDescription(int[] counters, double[] proportions)
        {
            var sb = new StringBuilder();
            var len = counters.Length;

            for (int i = 0; i < len; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(counters[i]);
                sb.Append(" (");
                sb.Append(proportions[i].ToString("0.000"));
                sb.Append(")");
            }

            return sb.ToString();
        }
    }
}
