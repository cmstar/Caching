using System;
using System.Text;
using cmstar.Serialization.Json;
using NUnit.Framework;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    [TestFixture]
    public class RedisConvertTests
    {
        [Test]
        public void TestToRedisValue()
        {
            void Check(object value, string expected)
            {
                var redisValue = RedisConvert.ToRedisValue(value);
                var msgBuilder = new StringBuilder();
                msgBuilder.Append("Check failed on ");
                if (value == null)
                {
                    msgBuilder.Append("<null> .");
                }
                else
                {
                    msgBuilder.Append(value).Append(" , the type is ").Append(value.GetType().Name).Append(" .");
                }

                Assert.AreEqual(expected, redisValue.ToString(), msgBuilder.ToString());
            }

            // null
            Check(null, CacheEnv.NullValueString);

            // primitive types
            // ReSharper disable RedundantCast
            Check(true, "1");
            Check(false, "0");
            Check((sbyte)-10, "-10");
            Check((sbyte)126, "126");
            Check((byte)0, "0");
            Check((byte)255, "255");
            Check((short)0, "0");
            Check((short)12345, "12345");
            Check((short)-22321, "-22321");
            Check((ushort)65535, "65535");
            Check(0, "0");
            Check(123456789, "123456789");
            Check(-123456789, "-123456789");
            Check((uint)123456789, "123456789");
            Check((long)0, "0");
            Check((long)-1234567890, "-1234567890");
            Check((long)9876543210, "9876543210");
            Check((ulong)3339876543210, "3339876543210");
            Check((decimal)0, "0");
            Check((decimal)139876543210.223, "139876543210.223");
            Check((decimal)-139876543210.223, "-139876543210.223");

            // 浮点数有精度误差，转字符串前后结果可能不一致，需单独断言。
            Check((float)0, "0");
            Assert.AreEqual((float)-987.654321, (float)(double)RedisConvert.ToRedisValue((float)-987.654321));
            Check(0.0, "0");
            Assert.AreEqual(982227.654321, (double)RedisConvert.ToRedisValue(982227.654321));
            Assert.AreEqual(-982227.654321, (double)RedisConvert.ToRedisValue(-982227.654321));

            // other simple types
            const string s = "a string\t!@#$%^&*()\n\u1313";
            Check(s, s);
            Check('a', "a");
            Check('\0', "\0");
            Check(DBNull.Value, "");

            // DateTime and DateTimeOffset
            var now = DateTime.Now;
            Check(now, now.Ticks.ToString());
            Check(new DateTimeOffset(2020, 4, 24, 17, 07, 03, 666, TimeSpan.FromHours(8)), "637233448236660000+288000000000");

            // guid
            var guid = Guid.NewGuid();
            Check(guid, guid.ToString("N"));

            // byte array
            var bytes = guid.ToByteArray();
            Assert.AreEqual((byte[])RedisConvert.ToRedisValue(bytes), bytes);

            // object
            var valueTypeValue = CacheValueStruct.CloneSample();
            Check(valueTypeValue, JsonSerializer.Default.Serialize(valueTypeValue));

            var obj = CacheValueClass.CloneSample();
            Check(obj, JsonSerializer.Default.Serialize(obj));

            // nullable
            Check((int?)0, "0");
            Check((int?)123, "123");
            Check((DateTime?)now, now.Ticks.ToString());
            Check((Guid?)guid, guid.ToString("N"));
            Check((CacheValueStruct?)valueTypeValue, JsonSerializer.Default.Serialize(valueTypeValue));
        }

        [Test]
        public void TestFromRedisValue()
        {
            void Check<T>(RedisValue value, T excpected)
            {
                var res = RedisConvert.FromRedisValue<T>(value);

                if (typeof(T) == typeof(byte[]))
                {
                    Assert.AreEqual((byte[])(object)excpected, (byte[])(object)res);
                }
                else
                {
                    Assert.AreEqual(excpected, res);
                }
            }

            void CheckSelf<T>(T excpected)
            {
                var redisValue = RedisConvert.ToRedisValue(excpected);
                Check(redisValue, excpected);
            }

            // primitive types
            // ReSharper disable RedundantCast
            CheckSelf(true);
            CheckSelf(false);
            CheckSelf((sbyte)-10);
            CheckSelf((sbyte)126);
            CheckSelf((byte)0);
            CheckSelf((byte)255);
            CheckSelf((short)0);
            CheckSelf((short)12345);
            CheckSelf((short)-22321);
            CheckSelf((ushort)65535);
            CheckSelf(0);
            CheckSelf(123456789);
            CheckSelf(-123456789);
            CheckSelf((uint)123456789);
            CheckSelf((long)0);
            CheckSelf((long)-1234567890);
            CheckSelf((long)9876543210);
            CheckSelf((ulong)3339876543210);
            Check("0", 0M);
            Check("139876543210.223", 139876543210.223);
            Check("-139876543210.223", -139876543210.223);

            // 浮点数有精度误差，转字符串前后结果可能不一致，需单独断言。
            CheckSelf((float)0);
            CheckSelf((float)-987.654321);
            CheckSelf(0.0);
            CheckSelf(982227.654321);
            CheckSelf(-982227.654321);

            // other simple types
            const string s = "a string\t!@#$%^&*()\n\u1313";
            CheckSelf(s);
            Check("a", 'a');
            Check("\0", '\0');
            Check("", DBNull.Value);

            // DateTime and DateTimeOffset
            var now = DateTime.Now;
            Check(637233448236660000, new DateTime(2020, 4, 24, 17, 07, 03, DateTimeKind.Local).AddMilliseconds(666));
            Check("637233448236660000+288000000000", new DateTimeOffset(2020, 4, 24, 17, 07, 03, 666, TimeSpan.FromHours(8)));

            // guid
            var guid = Guid.NewGuid();
            Check(guid.ToString("N"), guid);

            // byte array
            var bytes = guid.ToByteArray();
            Assert.AreEqual(bytes, bytes);

            // object
            var valueTypeValue = CacheValueStruct.CloneSample();
            Check(JsonSerializer.Default.Serialize(valueTypeValue), valueTypeValue);

            var obj = CacheValueClass.CloneSample();
            Check(RedisConvert.ToRedisValue(obj), JsonSerializer.Default.Serialize(obj));

            // nullable
            Check(RedisConvert.ToRedisValue(null), (int?)null);
            CheckSelf((int?)0);
            CheckSelf((int?)123);
            CheckSelf((DateTime?)now);
            CheckSelf((Guid?)guid);
            CheckSelf((CacheValueStruct?)valueTypeValue);
        }
    }
}