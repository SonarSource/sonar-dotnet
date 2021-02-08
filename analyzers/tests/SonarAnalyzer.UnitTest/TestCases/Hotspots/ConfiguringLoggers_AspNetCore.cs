using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging => // Noncompliant {{Make sure that this logger's configuration is safe.}}
            {
                // ...
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            IConfiguration config = null;
            LogLevel level = LogLevel.Critical;
            bool includeScopes = false;
            Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter = null;
            Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings consoleSettings = null;
            Microsoft.Extensions.Logging.AzureAppServices.AzureAppServicesDiagnosticsSettings azureSettings = null;
            Microsoft.Extensions.Logging.EventLog.EventLogSettings eventLogSettings = null;

            // An issue will be raised for each call to an ILoggerFactory extension methods adding loggers.
            loggerFactory.AddAzureWebAppDiagnostics();
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that this logger's configuration is safe.}}
            loggerFactory.AddAzureWebAppDiagnostics(azureSettings); // Noncompliant
            loggerFactory.AddConsole(); // Noncompliant
            loggerFactory.AddConsole(level); // Noncompliant
            loggerFactory.AddConsole(level, includeScopes); // Noncompliant
            loggerFactory.AddConsole(filter); // Noncompliant
            loggerFactory.AddConsole(filter, includeScopes); // Noncompliant
            loggerFactory.AddConsole(config); // Noncompliant
            loggerFactory.AddConsole(consoleSettings); // Noncompliant
            loggerFactory.AddDebug(); // Noncompliant
            loggerFactory.AddDebug(level); // Noncompliant
            loggerFactory.AddDebug(filter); // Noncompliant
            loggerFactory.AddEventLog(); // Noncompliant
            loggerFactory.AddEventLog(eventLogSettings); // Noncompliant
            loggerFactory.AddEventLog(level); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that this logger's configuration is safe.}}

            // Testing the next method using a hack - see notes at the end of the file
            loggerFactory.AddEventSourceLogger(); // Noncompliant

            IEnumerable<ILoggerProvider> providers = null;
            LoggerFilterOptions filterOptions1 = null;
            IOptionsMonitor<LoggerFilterOptions> filterOptions2 = null;

            LoggerFactory factory = new LoggerFactory(); // Noncompliant
//                                  ^^^^^^^^^^^^^^^^^^^    {{Make sure that this logger's configuration is safe.}}

            new LoggerFactory(providers); // Noncompliant
            new LoggerFactory(providers, filterOptions1); // Noncompliant
            new LoggerFactory(providers, filterOptions2); // Noncompliant
        }

        public void AdditionalTests(IWebHostBuilder webHostBuilder,  IServiceCollection serviceDescriptors)
        {
            var factory = new MyLoggerFactory();
//                        ^^^^^^^^^^^^^^^^^^^^^
            new MyLoggerFactory("data"); // Noncompliant

            // Calling extension methods as static methods
            WebHostBuilderExtensions.ConfigureLogging(webHostBuilder, (Action<ILoggingBuilder>)null);            // Noncompliant
            LoggingServiceCollectionExtensions.AddLogging(serviceDescriptors, (Action<ILoggingBuilder>)null);    // Noncompliant

            AzureAppServicesLoggerFactoryExtensions.AddAzureWebAppDiagnostics(factory, null);    // Noncompliant
            ConsoleLoggerExtensions.AddConsole(factory);                            // Noncompliant
            DebugLoggerFactoryExtensions.AddDebug(factory);                         // Noncompliant
            EventLoggerFactoryExtensions.AddEventLog(factory);                      // Noncompliant
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
