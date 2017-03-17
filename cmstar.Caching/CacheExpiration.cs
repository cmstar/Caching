using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 表示一个缓存过期时间。
    /// </summary>
    public class CacheExpiration
    {
        /// <summary>
        /// 表示缓存不会过期。
        /// </summary>
        public static CacheExpiration Zero = new CacheExpiration(0, 0);

        /// <summary>
        /// 以秒为单位，创建一个新的<see cref="CacheExpiration"/>实例。
        /// </summary>
        /// <param name="baseExpirationSeconds">缓存的基本过期时间（秒）。</param>
        /// <param name="randomRangeSeconds">过期时间的随机量（秒），使用0表示不添加随机量。</param>
        /// <returns><see cref="CacheExpiration"/>实例。</returns>
        public static CacheExpiration FromSeconds(int baseExpirationSeconds, int randomRangeSeconds = 0)
        {
            return new CacheExpiration(baseExpirationSeconds, randomRangeSeconds);
        }

        /// <summary>
        /// 以分钟为单位，创建一个新的<see cref="CacheExpiration"/>实例。
        /// </summary>
        /// <param name="baseExpirationMinutes">缓存的基本过期时间（分钟）。</param>
        /// <param name="randomRangeMinutes">过期时间的随机量（分钟），使用0表示不添加随机量。</param>
        /// <returns><see cref="CacheExpiration"/>实例。</returns>
        public static CacheExpiration FromMinutes(int baseExpirationMinutes, int randomRangeMinutes = 0)
        {
            return new CacheExpiration(baseExpirationMinutes * 60, randomRangeMinutes * 60);
        }

        /// <summary>
        /// 以小时为单位，创建一个新的<see cref="CacheExpiration"/>实例。
        /// </summary>
        /// <param name="baseExpirationHours">缓存的基本过期时间（小时）。</param>
        /// <param name="randomRangeHours">过期时间的随机量（小时），使用0表示不添加随机量。</param>
        /// <returns><see cref="CacheExpiration"/>实例。</returns>
        public static CacheExpiration FromHours(int baseExpirationHours, int randomRangeHours = 0)
        {
            return new CacheExpiration(baseExpirationHours * 3600, randomRangeHours * 3600);
        }

        /// <summary>
        /// 以<see cref="TimeSpan"/>为参照值，创建一个新的<see cref="CacheExpiration"/>实例。
        /// 时间精确到秒。
        /// </summary>
        /// <param name="baseExpiration">缓存的基本过期时间。</param>
        /// <param name="randomRange">过期时间的随机量。</param>
        /// <returns><see cref="CacheExpiration"/>实例。</returns>
        public static CacheExpiration FromTimeSpan(TimeSpan baseExpiration, TimeSpan? randomRange = null)
        {
            var rnd = randomRange.HasValue ? (int)randomRange.Value.TotalSeconds : 0;
            return new CacheExpiration((int)baseExpiration.TotalSeconds, rnd);
        }

        private readonly int _baseExpirationSeconds;
        private readonly int _randomRangeSeconds;
        private readonly Random _random;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="baseExpirationSeconds">缓存的基本过期时间（秒）。</param>
        /// <param name="randomRangeSeconds">过期时间的随机量（秒）。</param>
        public CacheExpiration(int baseExpirationSeconds, int randomRangeSeconds)
        {
            _baseExpirationSeconds = baseExpirationSeconds;
            _randomRangeSeconds = Math.Abs(randomRangeSeconds);

            if (_baseExpirationSeconds < 0)
            {
                var msg = string.Format(
                    "The expiration time {0}s should not be less than zero.",
                    baseExpirationSeconds);
                throw new ArgumentOutOfRangeException("baseExpirationSeconds", msg);
            }

            if (_randomRangeSeconds != 0)
            {
                if (_randomRangeSeconds < 0 || _randomRangeSeconds >= _baseExpirationSeconds)
                {
                    var msg = string.Format(
                        "The random range {0}s is not valid (base expiration {1}s).",
                        randomRangeSeconds, baseExpirationSeconds);
                    throw new ArgumentOutOfRangeException("randomRangeSeconds", msg);
                }

                _random = new Random();
            }
        }

        /// <summary>
        /// 获取缓存的基本过期时间（秒）。
        /// </summary>
        public int BaseExpirationSeconds
        {
            get { return _baseExpirationSeconds; }
        }

        /// <summary>
        /// 获取缓存过期时间的随机量（秒）。
        /// </summary>
        public int RandomRangeSeconds
        {
            get { return _randomRangeSeconds; }
        }

        /// <summary>
        /// 得到一个新的过期时间，单位为秒。若过期时间包含有随机量，该时间已经经过随机量的计算。
        /// </summary>
        /// <returns>过期时间。</returns>
        public int NewExpirationSeconds()
        {
            if (_baseExpirationSeconds == 0)
                return 0;

            // Random不是线程安全的，同时Next可能产生相同的值，但在这里无所谓，故不加锁
            var expirationSeconds = _random == null
                ? _baseExpirationSeconds
                : _baseExpirationSeconds + _random.Next(-_randomRangeSeconds, _randomRangeSeconds + 1);
            return expirationSeconds;
        }
    }
}
