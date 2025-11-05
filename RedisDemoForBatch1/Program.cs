using System;
using StackExchange.Redis;

namespace RedisDemo
{
    internal class Program
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();
        private static bool connectionError = false;

        private static void Main(string[] args)
        {
            Console.WriteLine("=== Redis Demo Application ===\n");

            // Test connection on startup
            if (!TestConnection())
            {
                Console.WriteLine("Failed to connect to Redis. Please check your connection string and ensure Redis is running.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            string response = "Y";
            do
            {
                try
                {
                    Console.WriteLine("\n=== Main Menu ===");
                    Console.WriteLine("Press 1 to Get Value");
                    Console.WriteLine("Press 2 to Set Value");
                    Console.WriteLine("Press 3 to Delete Value");
                    Console.WriteLine("Press 4 to Exit");
                    Console.Write("\nEnter your choice: ");

                    string choice = Console.ReadLine()?.Trim();

                    switch (choice)
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

                        case "4":
                            Console.WriteLine("Exiting application...");
                            return;

                        default:
                            Console.WriteLine("Invalid choice. Please enter 1, 2, 3, or 4.");
                            break;
                    }

                    Console.Write("\nDo you want to continue? (Y/N): ");
                    response = Console.ReadLine()?.Trim();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nUnexpected error: {ex.Message}");
                    Console.WriteLine("Please try again.");
                }
            } while (response?.ToUpper() == "Y");

            Console.WriteLine("\nThank you for using Redis Demo!");
        }

        private static bool TestConnection()
        {
            try
            {
                var db = lazyConnection.Value.GetDatabase();
                return lazyConnection.Value.IsConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                connectionError = true;
                return false;
            }
        }

        private static void SetValue()
        {
            try
            {
                Console.Write("\nEnter the Key: ");
                string key = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("Error: Key cannot be empty.");
                    return;
                }

                Console.Write("Enter the Value: ");
                string value = Console.ReadLine();

                if (value == null)
                {
                    Console.WriteLine("Error: Value cannot be null.");
                    return;
                }

                Console.Write("Time to expire in seconds (0 for no expiration): ");
                string timeInput = Console.ReadLine()?.Trim();

                if (!int.TryParse(timeInput, out int time) || time < 0)
                {
                    Console.WriteLine("Error: Please enter a valid non-negative number for expiration time.");
                    return;
                }

                // Connection to Redis cache
                IDatabase redisConnection = lazyConnection.Value.GetDatabase();

                bool success;
                if (time == 0)
                {
                    success = redisConnection.StringSet(key, value);
                }
                else
                {
                    TimeSpan expiration = TimeSpan.FromSeconds(time);
                    success = redisConnection.StringSet(key, value, expiration);
                }

                if (success)
                {
                    Console.WriteLine($"Success: Key '{key}' has been set successfully.");
                    if (time > 0)
                    {
                        Console.WriteLine($"The key will expire in {time} seconds.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to set the value in Redis.");
                }
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"Redis connection error: {ex.Message}");
                Console.WriteLine("Please check your Redis server connection.");
            }
            catch (RedisException ex)
            {
                Console.WriteLine($"Redis error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private static void GetValue()
        {
            try
            {
                Console.Write("\nEnter the Key: ");
                string key = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("Error: Key cannot be empty.");
                    return;
                }

                IDatabase redisConnection = lazyConnection.Value.GetDatabase();
                RedisValue valueFromRedis = redisConnection.StringGet(key);

                if (valueFromRedis.IsNullOrEmpty)
                {
                    Console.WriteLine($"Key '{key}' not found in Redis or has no value.");
                }
                else
                {
                    Console.WriteLine($"Value from Redis for key '{key}': {valueFromRedis}");
                }
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"Redis connection error: {ex.Message}");
                Console.WriteLine("Please check your Redis server connection.");
            }
            catch (RedisException ex)
            {
                Console.WriteLine($"Redis error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private static void DeleteValue()
        {
            try
            {
                Console.Write("\nEnter the Key: ");
                string key = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("Error: Key cannot be empty.");
                    return;
                }

                IDatabase redisConnection = lazyConnection.Value.GetDatabase();
                bool deleted = redisConnection.KeyDelete(key);

                if (deleted)
                {
                    Console.WriteLine($"Success: Key '{key}' has been deleted from Redis.");
                }
                else
                {
                    Console.WriteLine($"Key '{key}' was not found in Redis.");
                }
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"Redis connection error: {ex.Message}");
                Console.WriteLine("Please check your Redis server connection.");
            }
            catch (RedisException ex)
            {
                Console.WriteLine($"Redis error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                try
                {
                    string cacheConnection = "<connection string here>";

                    // Validate connection string
                    if (string.IsNullOrWhiteSpace(cacheConnection) || cacheConnection == "<connection string here>")
                    {
                        throw new InvalidOperationException(
                            "Redis connection string is not configured. " +
                            "Please update the connection string in Program.cs:CreateConnection() method.");
                    }

                    Console.WriteLine("Connecting to Redis...");
                    ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(cacheConnection);
                    Console.WriteLine("Successfully connected to Redis.\n");
                    return connection;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Redis connection: {ex.Message}");
                    throw;
                }
            });
        }
    }
}