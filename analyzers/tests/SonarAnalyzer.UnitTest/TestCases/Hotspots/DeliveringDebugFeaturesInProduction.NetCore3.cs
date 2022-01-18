﻿using Microsoft.Extensions.Options;
using Microsoft.AspNetCore;

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
                logging.AddConsole();
                // ...
            });
            return services;
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            IConfiguration config = null;
            LogLevel level = LogLevel.Critical;
            bool includeScopes = false;
            Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter = null;
            Microsoft.Extensions.Logging.Console.ConsoleLoggerOptions consoleSettings = null;
            Microsoft.Extensions.Logging.AzureAppServices.AzureBlobLoggerOptions azureSettings = null;
            Microsoft.Extensions.Logging.EventLog.EventLogSettings eventLogSettings = null;

            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddFilter("SampleApp.Program", LogLevel.Debug)
                                                                              .AddAzureWebAppDiagnostics() // Noncompliant
                                                                              .AddConsole()// Noncompliant
                                                                              .AddDebug() // Noncompliant
                                                                              .AddEventLog()// Noncompliant
                                                                              .AddEventSourceLogger())) // Noncompliant
            { }

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
            EventSourceLoggerFactoryExtensions.AddEventSourceLogger(factory);       // Noncompliant
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

// HACK - the tests are not currently built againts NET Core 2.0 so one of the
// methods we want to test is not available. Instead, we'll define a type with
// the expected name and method signature
namespace Microsoft.Extensions.Logging
{
    internal static class EventSourceLoggerFactoryExtensions
    {
        public static void AddEventSourceLogger(this ILoggerFactory factory) { /* no-op */ }
    }
}
