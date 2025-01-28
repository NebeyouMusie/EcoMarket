using DotNetEnv;
using System.Diagnostics;
using System.IO;

namespace EcoMarket
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            DotNetEnv.Env.Load(envPath);

            var mongoUrl = Environment.GetEnvironmentVariable("MONGO_URL");
            Debug.WriteLine($"Environment file path: {envPath}");
            Debug.WriteLine($"Environment file exists: {File.Exists(envPath)}");
            Debug.WriteLine($"MONGO_URL found: {!string.IsNullOrEmpty(mongoUrl)}");

            if (string.IsNullOrEmpty(mongoUrl))
            {
                throw new InvalidOperationException($"MONGO_URL not found in .env file at {envPath}. File exists: {File.Exists(envPath)}");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
