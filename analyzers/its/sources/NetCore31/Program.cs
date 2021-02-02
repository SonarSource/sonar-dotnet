using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace NetCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>()).Build().Run();

            Console.WriteLine("Hello World!");
            if (args[0] != null)
            {
                Console.WriteLine(args[0].ToString());
            }
        }
    }
}
