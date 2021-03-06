using System.Configuration;

namespace cmstar.Caching.Redis
{
    public static class RedisTestEnv
    {
        public static string GetRedisServerConfiguration()
        {
            var config = ConfigurationManager.AppSettings["redis.server1"];

            if (string.IsNullOrWhiteSpace(config))
            {
                config = "localhost:6379";
            }

            return config;
        }

        public static string GetRedisServerConfiguration2()
        {
            var config = ConfigurationManager.AppSettings["redis.server2"];

            if (string.IsNullOrWhiteSpace(config))
            {
                config = "localhost:6380";
            }

            return config;
        }

        public static string GetRedisServerConfiguration3()
        {
            var config = ConfigurationManager.AppSettings["redis.server3"];

            if (string.IsNullOrWhiteSpace(config))
            {
                config = "localhost:6381";
            }

            return config;
        }
    }
}
