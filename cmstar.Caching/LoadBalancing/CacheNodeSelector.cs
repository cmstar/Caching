using System;

namespace cmstar.Caching.LoadBalancing
{
    /// <summary>
    /// 一个简单的容器，用于记录和选取用于负载均衡的缓存节点。
    /// </summary>
    internal class CacheNodeSelector
    {
        /*
         * 节点选择算法：
         * 
         * 每个节点带有一个大于0的比重值 weight，每次添加一个节点时，将节点数据追加在一个数组尾部，
         * 并记录此时的所有比重之和。假设添加了4个节点，比重分别为 W1、W2、W3、W4，则各点添加后的
         * 比重累加值在数组上形成一个类似下图的坐标轴：
         * 
         * node1:W1
         * node2:W2
         * node3:W3
         * node4:W4
         * 
         * <-node1-><-  node2  -><-   node3    -><- node4 ->
         *          W1         W1+W2         W1+W2+W3  W1+W2+W3+W4
         * ---------|------------|---------------|----------|-------> _nodes
         *   ^            ^                           ^
         *  hash1       hash2                        hash3
         * 
         * 这个轴（数组）的最后一个元素记录的权重和即为所有节点的权重之和，记为 WSUM。
         * 
         * 对于给定的 key，将其哈希值用 WSUM 取模，得到 S = HASH % WSUM，显然有 0 <= S < WSUM ，
         * 即 S 一定能落在坐标轴的起点到 WSUM 之间。以每个节点加入数组时所记录的比重和为上边界，
         * 即可找到 S 所归属的节点。上图中标记了每个节点所覆盖的区间。
         */

        private const int InitialNodesLength = 4;
        private readonly object _syncBlock = new object();

        // 所有已知的节点，添加新节点时，若长度不足，则扩容。
        // 实际的节点数量存放在 _nextNodeIndex。
        private CacheNode[] _nodes = new CacheNode[InitialNodesLength];

        // 当前已知节点的数量；也是下一个被添加的节点对应位置的索引。
        private int _nextNodeIndex;

        // 记录的当前所有节点的比重（weight）值的和，与 _nodes 最后一个元素记录的一致，
        // 为方便计算，单独记录一份。
        private int _weightSummation;

        /// <summary>
        /// 根据给定的缓存键的哈希值，为其分配一个缓存提供器。
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <returns>分配给该缓存键的提供器。</returns>
        public ICacheProvider Determine(string key)
        {
            ArgAssert.NotNullOrEmpty(key, nameof(key));

            if (_weightSummation == 0)
                throw new InvalidOperationException("There must be at least one cache provider node.");

            // 对 key 的 hash 取模，得到一个 [0, weightSummation) 间的值，这个值一定会落在
            // 某个节点的区间内，逐个遍历节点，找到该区间。
            // 这里不用加锁。目前只能添加新节点，不能移除节点，如果刚好有新的节点被添加，
            // 这里使用“旧的”字段值做处理，会忽略刚刚添加的节点，并不会产生数据错乱。

            var hash = Math.Abs(key.GetHashCode()) % _weightSummation;
            var len = _nextNodeIndex;
            var nodes = _nodes;

            for (int i = 0; i < len; i++)
            {
                var node = nodes[i];

                if (hash < node.WeightSummation)
                    return node.Provider;
            }

            return nodes[0].Provider;
        }

        /// <summary>
        /// 向选择器中添加一个节点。
        /// </summary>
        /// <param name="provider">缓存提供器的实例。</param>
        /// <param name="weight">该节点的比重，必须是正数。</param>
        public void AddCacheNode(ICacheProvider provider, int weight)
        {
            if (weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight), "The weight must be greater than zero.");

            lock (_syncBlock)
            {
                // 当存储节点的数组长度不够用时，将容量扩大到原来的 2n + 1 。
                if (_nodes.Length == _nextNodeIndex)
                {
                    var newSize = _nodes.Length * 2 + 1;
                    var newArrary = new CacheNode[newSize];
                    Array.Copy(_nodes, newArrary, _nodes.Length);
                    _nodes = newArrary;
                }

                var newWeightSummation = _weightSummation + weight;
                var newNode = new CacheNode
                {
                    Provider = provider,
                    WeightSummation = newWeightSummation
                };

                // 因为选取节点时没有加锁，要注意这边的赋值顺序，以防其数据错乱。
                // 根据流程，_weightSummation 需要最后赋值。
                _nodes[_nextNodeIndex] = newNode;
                _nextNodeIndex++;
                _weightSummation = newWeightSummation;
            }
        }

        private struct CacheNode
        {
            public ICacheProvider Provider;
            public int WeightSummation;
        }
    }
}
