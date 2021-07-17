using System;
using StackExchange.Redis;

namespace RedisDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var res = "Y";
            do
            {
                Console.WriteLine("Press 1 to Get Value\nPress 2 to Set Value\n3 to Delete Value");
                var Choice = Console.ReadLine();

                switch (Choice)
                {
                    case "1":
                        GetValue();
                        break;

                    case "2":
                        SetValue();
                        break;

                    case "3":
                        DeleteValue();
                        break;

                    default:
                        break;
                }

                Console.WriteLine("Do you want to continue more?");
                res = Console.ReadLine();
            } while (res.ToUpper() == "Y");
        }

        private static void SetValue()
        {
            Console.WriteLine("Enter the Key");
            var key = Console.ReadLine();

            Console.WriteLine("Enter the Value");
            var value = Console.ReadLine();

            Console.WriteLine("Time to expire in Secs");
            var time = Convert.ToInt32(Console.ReadLine());

            //connection to redis cache
            var redisConnection = lazyConnection.Value.GetDatabase();
            if (time == 0)
            {
                redisConnection.StringSet(key, value);
            }
            else
            {
                var span = new TimeSpan(0, 0, time);
                redisConnection.StringSet(key, value, span);
            }
        }

        private static void GetValue()
        {
            var redisConnection = lazyConnection.Value.GetDatabase();
            Console.WriteLine("Enter the Key");
            var key = Console.ReadLine();

            var valueFromRedis = redisConnection.StringGet(key);
            Console.WriteLine($"value from Redis for {key} = {valueFromRedis}");
        }

        private static void DeleteValue()
        {
            var redisConnection = lazyConnection.Value.GetDatabase();
            Console.WriteLine("Enter the Key");
            var key = Console.ReadLine();

            redisConnection.KeyDelete(key);
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = "<connection string here>";
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
        }
    }
}