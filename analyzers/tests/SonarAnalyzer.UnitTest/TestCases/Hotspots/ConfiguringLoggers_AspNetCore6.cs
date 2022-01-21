using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Logging.EventSource;


namespace MvcApp
{
    //RSPEC S4792: https://jira.sonarsource.com/browse/RSPEC-4792
    public class ProgramLogging
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args) // Noncompliant {{Make sure that this logger's configuration is safe.}}
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // ...
                })
                .UseStartup<StartupLogging>();
    }


    public class StartupLogging
    {
        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging => // Noncompliant {{Make sure that this logger's configuration is safe.}}
            {
                logging.AddConsole(); // Noncompliant
                // ...
            });
            return services;
        }

        public void Configure(IApplicationBuilder app)
        {
            IConfiguration config = null;
            LogLevel level = LogLevel.Critical;
            bool includeScopes = false;
            Func<string, LogLevel, bool> filter = null;
            EventLogSettings eventLogSettings = null;

            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddAzureWebAppDiagnostics() // Noncompliant [1,2,3,4,5,6,7]
                                                                              .AddAzureWebAppDiagnostics(azureSettings => { azureSettings.IncludeScopes = true; }) // Every invocation in this chain is noncompliant
                                                                              .AddConsole(options => { options.FormatterName = "simple"; })
                                                                              .AddDebug()
                                                                              .AddEventLog()
                                                                              .AddEventLog(eventLogSettings)
                                                                              .AddEventSourceLogger())) { }

            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddFilter(filter))) { } // Compliant, AddFilter does not add a new logger.

            IEnumerable<ILoggerProvider> providers = null;
            LoggerFilterOptions filterOptions1 = null;
            IOptionsMonitor<LoggerFilterOptions> filterOptions2 = null;

            LoggerFactory factory = new LoggerFactory(); // Noncompliant
//                                  ^^^^^^^^^^^^^^^^^^^    {{Make sure that this logger's configuration is safe.}}

            new LoggerFactory(providers); // Noncompliant
            new LoggerFactory(providers, filterOptions1); // Noncompliant
            new LoggerFactory(providers, filterOptions2); // Noncompliant
        }

        public void AdditionalTests(IWebHostBuilder webHostBuilder, IServiceCollection serviceDescriptors)
        {
            var factory = new MyLoggerFactory();
//                        ^^^^^^^^^^^^^^^^^^^^^
            new MyLoggerFactory("data"); // Noncompliant

            // Calling extension methods as static methods
            WebHostBuilderExtensions.ConfigureLogging(webHostBuilder, (Action<ILoggingBuilder>)null);            // Noncompliant
            LoggingServiceCollectionExtensions.AddLogging(serviceDescriptors, (Action<ILoggingBuilder>)null);    // Noncompliant
        }
    }

    public class MyLoggerFactory : ILoggerFactory
    {
        public MyLoggerFactory() { }
        public MyLoggerFactory(string data) { }

        public void AddProvider(ILoggerProvider provider) { /* no-op */ }
        public ILogger CreateLogger(string categoryName) => null;
        public void Dispose() { /* no-op */ }
    }
}
