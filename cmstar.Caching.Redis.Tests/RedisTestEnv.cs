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
    }
}
