Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports Microsoft.AspNetCore
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options

Namespace MvcApp

    Public Class ProgramLogging

        Public Shared Function CreateWebHostBuilder(args As String()) As IWebHostBuilder

            Dim host = WebHost.CreateDefaultBuilder(args)
            host.ConfigureLogging(Function(hostingContext, Logging)  ' Noncompliant
                                      ' ...
                                  End Function) _
            .UseStartup(Of StartupLogging)()

            '...
        End Function
    End Class


    Public Class StartupLogging

        Public Sub ConfigureServices(services As IServiceCollection)

            services.AddLogging(Function(logging) ' Noncompliant
                                    '...
                                End Function)
        End Sub

        Public Sub Configure(app As IApplicationBuilder, env As IHostingEnvironment, loggerFactory As ILoggerFactory)

            Dim config As IConfiguration = Nothing
            Dim level As LogLevel = LogLevel.Critical
            Dim includeScopes As Boolean = False
            Dim filter As Func(Of String, Microsoft.Extensions.Logging.LogLevel, Boolean) = Nothing
            Dim consoleSettings As Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings = Nothing
            Dim azureSettings As Microsoft.Extensions.Logging.AzureAppServices.AzureAppServicesDiagnosticsSettings = Nothing
            Dim eventLogSettings As Microsoft.Extensions.Logging.EventLog.EventLogSettings = Nothing

            ' An issue will be raised for each call to an ILoggerFactory extension methods adding loggers.
            loggerFactory.AddAzureWebAppDiagnostics() ' Noncompliant
            loggerFactory.AddAzureWebAppDiagnostics(azureSettings) ' Noncompliant
            loggerFactory.AddConsole() ' Noncompliant
            loggerFactory.AddConsole(level) ' Noncompliant
            loggerFactory.AddConsole(level, includeScopes) ' Noncompliant
            loggerFactory.AddConsole(filter) ' Noncompliant
            loggerFactory.AddConsole(filter, includeScopes) ' Noncompliant
            loggerFactory.AddConsole(config) ' Noncompliant
            loggerFactory.AddConsole(consoleSettings) ' Noncompliant
            loggerFactory.AddDebug() ' Noncompliant
            loggerFactory.AddDebug(level) ' Noncompliant
            loggerFactory.AddDebug(filter) ' Noncompliant
            loggerFactory.AddEventLog() ' Noncompliant
            loggerFactory.AddEventLog(eventLogSettings) ' Noncompliant
            loggerFactory.AddEventLog(level) ' Noncompliant

            ' Only available for NET Standard 2.0 and above. Tested for C# using a hack.
            'loggerFactory.AddEventSourceLogger() ' Non  compliant

            Dim providers As IEnumerable(Of ILoggerProvider) = Nothing
            Dim filterOptions1 As LoggerFilterOptions = Nothing
            Dim filterOptions2 As IOptionsMonitor(Of LoggerFilterOptions) = Nothing

            Dim factory As LoggerFactory = New LoggerFactory() ' Noncompliant
            factory = New LoggerFactory(providers) ' Noncompliant
            factory = New LoggerFactory(providers, filterOptions1) ' Noncompliant
            factory = New LoggerFactory(providers, filterOptions2) ' Noncompliant
        End Sub

        Public Sub AdditionalTests(webHostBuilder As IWebHostBuilder, serviceDescriptors As IServiceCollection)
            Dim factory = New MyLoggerFactory()
'                         ^^^^^^^^^^^^^^^^^^^^^
            factory = New MyLoggerFactory("data") ' Noncompliant


            ' Calling extension methods as static methods
            WebHostBuilderExtensions.ConfigureLogging(webHostBuilder, CType(Nothing, Action(Of ILoggingBuilder)))    ' Noncompliant
            LoggingServiceCollectionExtensions.AddLogging(serviceDescriptors, CType(Nothing, Action(Of ILoggingBuilder)))    ' Noncompliant

            AzureAppServicesLoggerFactoryExtensions.AddAzureWebAppDiagnostics(factory, Nothing)  ' Noncompliant
            ConsoleLoggerExtensions.AddConsole(factory)          ' Noncompliant
            DebugLoggerFactoryExtensions.AddDebug(factory)       ' Noncompliant
            EventLoggerFactoryExtensions.AddEventLog(factory)    ' Noncompliant
        End Sub
    End Class

    Public Class MyLoggerFactory
        Implements ILoggerFactory
        Public Sub New()
        End Sub

        Public Sub New(Data As String)
        End Sub

        Public Sub AddProvider(provider As ILoggerProvider) Implements ILoggerFactory.AddProvider
        End Sub

        Public Function CreateLogger(categoryName As String) As ILogger Implements ILoggerFactory.CreateLogger
            Return Nothing
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub

    End Class
End Namespace
