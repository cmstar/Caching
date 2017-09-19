using System.Configuration;

namespace cmstar.Caching.Redis
{
    public static class RedisTestEnv
    {
        public static string GetRedisServerConfiguration()
        {
            var config = ConfigurationManager.AppSettings["redis.server"];

            if (string.IsNullOrWhiteSpace(config))
                throw new ConfigurationErrorsException("The appsetting key 'redis.server' is not configured.");

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
