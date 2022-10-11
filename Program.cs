using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace raw_ws
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        // Forzamos la ejecución en el puerto 9002
                        // .UseUrls("http://localhost:9002");
                        .UseUrls("http://localhost:9012");
                });
    }
}