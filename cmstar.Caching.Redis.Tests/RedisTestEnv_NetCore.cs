#if NETCORE
using Microsoft.Extensions.Configuration;

namespace cmstar.Caching.Redis
{
    public static class RedisTestEnv
    {
        private static readonly IConfiguration Config;

        static RedisTestEnv()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }

        public static string GetRedisServerConfiguration()
        {
            var config = Config.GetSection("Redis")["Server1"];

            if (string.IsNullOrWhiteSpace(config))
            {
                config = "localhost:6379";
            }

            return config;
        }

        public static string GetRedisServerConfiguration2()
        {
            var config = Config.GetSection("Redis")["Server2"];

            if (string.IsNullOrWhiteSpace(config))
            {
                config = "localhost:6380";
            }

            return config;
        }

        public static string GetRedisServerConfiguration3()
        {
            var config = Config.GetSection("Redis")["Server3"];

            if (string.IsNullOrWhiteSpace(config))
            {
                config = "localhost:6381";
            }

            return config;
        }
    }
}
#endif