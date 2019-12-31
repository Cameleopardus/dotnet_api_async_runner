using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using async_runner.Helpers;
using async_runner.Worker;

namespace async_runner
{
    public class Program
    {
        public static void Main(string[] args)
        {

            string mode = System.Environment.GetEnvironmentVariable("APP_ENV");

            if (mode == null){
                Console.WriteLine("APP_ENV not detected, defaulting to API instance.");
                mode = "api";
            }
            switch (mode){
                case "worker":
                    TaskReceiver receiver = new TaskReceiver();
                    TaskHandler worker = new TaskHandler();
                    receiver.StartConsumer(worker);
                    break;
                case "api":
                    Console.WriteLine("Starting API Mode.");
                    CreateWebHostBuilder(args).Build().Run();
                    break;

            }
            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
