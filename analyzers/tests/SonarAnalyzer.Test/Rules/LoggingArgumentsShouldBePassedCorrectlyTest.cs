/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LoggingArgumentsShouldBePassedCorrectlyTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<LoggingArgumentsShouldBePassedCorrectly>();

    [TestMethod]
    public void LoggingArgumentsShouldBePassedCorrectly_CS() =>
        builder.AddPaths("LoggingArgumentsShouldBePassedCorrectly.cs").AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions()).Verify();

    [TestMethod]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void LoggingArgumentsShouldBePassedCorrectly_MicrosoftExtensionsLogging_NonCompliant_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;
            using Microsoft.Extensions.Logging.Abstractions;

            public class Program
            {
                public void Method(ILogger logger, Exception e)
                {
                    logger.{{methodName}}("Expected exception.");
                    logger.{{methodName}}(e, "Expected exception.");
                    logger.{{methodName}}(null, "Expected exception.", e);                                                                     // FN
                    logger.{{methodName}}(new EventId(), "Expected exception.");
                    logger.{{methodName}}(new EventId(), e, "Expected exception.");
                    LoggerExtensions.{{methodName}}(logger, "Expected exception.");
                    LoggerExtensions.{{methodName}}(logger, new EventId(), e, "Expected exception.");

                    logger.{{methodName}}(new EventId(), e, "Expected exception.", e, new EventId(), LogLevel.Critical);                       // Noncompliant (log level)
                                                                                                                                               // Secondary @-1
                    logger.{{methodName}}(new EventId(), "Expected exception.", e, new EventId(), LogLevel.Critical);                          // Noncompliant (exception, log level)
                                                                                                                                               // Secondary @-1
                                                                                                                                               // Secondary @-2
                    logger.{{methodName}}(e, "Expected exception.", e, new EventId(), LogLevel.Critical);                                      // Noncompliant (event id, log level)
                                                                                                                                               // Secondary @-1
                                                                                                                                               // Secondary @-2
                    logger.{{methodName}}("Expected exception.", e, new EventId(), LogLevel.Critical);                                         // Noncompliant (exception, event id, log level)
                                                                                                                                               // Secondary @-1
                                                                                                                                               // Secondary @-2
                                                                                                                                               // Secondary @-3
                    LoggerExtensions.{{methodName}}(logger, "Expected exception.", e, new EventId(), LogLevel.Critical);                       // Noncompliant (exception, event id, log level)
                                                                                                                                               // Secondary @-1
                                                                                                                                               // Secondary @-2
                                                                                                                                               // Secondary @-3
                    LoggerExtensions.{{methodName}}(logger, new EventId(), e, "Expected exception.", e, new EventId(), LogLevel.Critical);     // Noncompliant (log level)
                                                                                                                                               // Secondary @-1
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
           .Verify();

    [TestMethod]
    public void LoggingArgumentsShouldBePassedCorrectly_MicrosoftExtensionsLogging_Log_CS() =>
        builder.AddSnippet("""
            using System;
            using Microsoft.Extensions.Logging;
            using Microsoft.Extensions.Logging.Abstractions;

            public class Program
            {
                public void Method(ILogger logger, Exception e)
                {
                    logger.Log(LogLevel.Warning, "Expected exception.");
                    logger.Log(LogLevel.Warning, e, "Expected exception.");
                    logger.Log(LogLevel.Warning, new EventId(), "Expected exception.");
                    logger.Log(LogLevel.Warning, new EventId(), e, "Expected exception.");
                    LoggerExtensions.Log(logger, LogLevel.Warning, "Expected exception.");
                    LoggerExtensions.Log(logger, LogLevel.Warning, new EventId(), e, "Expected exception.");

                    logger.Log(LogLevel.Warning, new EventId(), e, "Expected exception.", e, new EventId(), LogLevel.Critical);
                    logger.Log(LogLevel.Warning, new EventId(), "Expected exception.", e, new EventId(), LogLevel.Critical);                          // Noncompliant (exception)
                                                                                                                                                      // Secondary @-1
                    logger.Log(LogLevel.Warning, e, "Expected exception.", e, new EventId(), LogLevel.Critical);                                      // Noncompliant (event id)
                                                                                                                                                      // Secondary @-1
                    logger.Log(LogLevel.Warning, "Expected exception.", e, new EventId(), LogLevel.Critical);                                         // Noncompliant (exception, event id)
                                                                                                                                                      // Secondary @-1
                                                                                                                                                      // Secondary @-2
                    LoggerExtensions.Log(logger, LogLevel.Warning, "Expected exception.", e, new EventId(), LogLevel.Critical);                       // Noncompliant (exception, event id)
                                                                                                                                                      // Secondary @-1
                                                                                                                                                      // Secondary @-2
                    LoggerExtensions.Log(logger, LogLevel.Warning, new EventId(), e, "Expected exception.", e, new EventId(), LogLevel.Critical);
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
           .Verify();

    [TestMethod]
    [DataRow("DebugFormat")]
    [DataRow("ErrorFormat")]
    [DataRow("FatalFormat")]
    [DataRow("InfoFormat")]
    [DataRow("TraceFormat")]
    [DataRow("WarnFormat")]
    public void LoggingArgumentsShouldBePassedCorrectly_CastleCore_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using Castle.Core.Logging;

            public class Program
            {
                public void Method(ILogger logger, string message, Exception e)
                {
                    logger.{{methodName}}(message);                                       // Compliant
                    logger.{{methodName}}(message, e);                                    // Noncompliant
                                                                                          // Secondary @-1
                    logger.{{methodName}}(CultureInfo.CurrentCulture, message);           // Compliant
                    logger.{{methodName}}(CultureInfo.CurrentCulture, message, e);        // Noncompliant
                                                                                          // Secondary @-1
                    logger.{{methodName}}(e, message);                                    // Compliant
                    logger.{{methodName}}(e, message, e);                                 // Compliant
                    logger.{{methodName}}(e, CultureInfo.CurrentCulture, message);        // Compliant
                    logger.{{methodName}}(e, CultureInfo.CurrentCulture, message, e);     // Compliant
                }
            }
            """)
        .AddReferences(NuGetMetadataReference.CastleCore())
        .Verify();

    [TestMethod]
    [DataRow("Log", "LogLevel.Debug,", "")]
    [DataRow("Debug")]
    [DataRow("ConditionalDebug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("ConditionalTrace")]
    [DataRow("Warn")]
    public void LoggingArgumentsShouldBePassedCorrectly_NLog_CS(string methodName, string logLevel = "", string logLevelExpectation = "// Secondary @-2") =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using NLog;

            public class Program
            {
                public void Method(Logger logger, Exception e)
                {
                    logger.{{methodName}}({{logLevel}} e);                                                    // Compliant
                    logger.{{methodName}}({{logLevel}} CultureInfo.CurrentCulture, e);                        // Compliant
                    logger.{{methodName}}({{logLevel}} e, "Message!");                                        // Compliant
                    logger.{{methodName}}({{logLevel}} e, "Message!", e);                                     // Compliant
                    logger.{{methodName}}({{logLevel}} e, CultureInfo.CurrentCulture, "Message!", e);         // Compliant
                    logger.{{methodName}}({{logLevel}} CultureInfo.CurrentCulture, "Message!", 1, 2, 3, e);   // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}} "Message!");                                           // Compliant
                    logger.{{methodName}}({{logLevel}} "Message!", 1, 2, 3, e);                               // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}} "Message!", e);                                        // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}} CultureInfo.CurrentCulture, "Message!", e);            // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}<int>({{logLevel}} "Message!", 1);                                   // Compliant
                    logger.{{methodName}}<Exception>({{logLevel}} "Message!", e);                             // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}}CultureInfo.CurrentCulture, "Message!", 1, e);          // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}} "Message!", 1, e);                                     // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}} CultureInfo.CurrentCulture, "Message!", 1, 2, e);      // Noncompliant
                                                                                                              // Secondary @-1
                    logger.{{methodName}}({{logLevel}} "Message!", 1, LogLevel.Debug, e);                     // Noncompliant
                                                                                                              // Secondary @-1
                                                                                                              {{logLevelExpectation}}
                    ILoggerExtensions.{{methodName}}(logger, {{logLevel}} e, null);                           // Compliant
                }
            }
            """)
        .AddReferences(NuGetMetadataReference.NLog())
        .Verify();

    [TestMethod]
    [DataRow("ConditionalDebug")]
    [DataRow("ConditionalTrace")]
    public void LoggingArgumentsShouldBePassedCorrectly_NLog_ConditionalExtensions_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using NLog;

            public class Program
            {
                public void Method(Logger logger, Exception e)
                {
                    ILoggerExtensions.{{methodName}}(logger, e);                                                    // Compliant
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, e);                        // Compliant
                    ILoggerExtensions.{{methodName}}(logger, e, "Message!");                                        // Compliant
                    ILoggerExtensions.{{methodName}}(logger, e, "Message!", e);                                     // Compliant
                    ILoggerExtensions.{{methodName}}(logger, e, CultureInfo.CurrentCulture, "Message!", e);         // Compliant
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", 1, 2, 3, e);   // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, "Message!");                                           // Compliant
                    ILoggerExtensions.{{methodName}}(logger, "Message!", 1, 2, 3, e);                               // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, "Message!", e);                                        // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", e);            // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}<int>(logger, "Message!", 1);                                   // Compliant
                    ILoggerExtensions.{{methodName}}<Exception>(logger, "Message!", e);                             // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", 1, e);         // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, "Message!", 1, e);                                     // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", 1, 2, e);      // Noncompliant
                                                                                                                    // Secondary @-1
                    ILoggerExtensions.{{methodName}}(logger, "Message!", 1, 2, e);                                  // Noncompliant
                                                                                                                    // Secondary @-1
                }
            }
            """)
        .AddReferences(NuGetMetadataReference.NLog())
        .Verify();

    [TestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Information")]
    [DataRow("Verbose")]
    [DataRow("Warning")]
    public void LoggingArgumentsShouldBePassedCorrectly_Serilog_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using Serilog;

            public class Program
            {
                public void Method(ILogger logger, string message, Exception e)
                {
                    Log.{{methodName}}("Message!");
                    Log.{{methodName}}("Message!", e);                                   // Noncompliant
                                                                                         // Secondary @-1
                    Log.{{methodName}}<int>("Message!", 1);
                    Log.{{methodName}}<Exception>("Message!", e);                        // Noncompliant
                                                                                         // Secondary @-1
                    Log.{{methodName}}<int, Exception>("Message!", 1, e);                // Noncompliant
                                                                                         // Secondary @-1
                    Log.{{methodName}}<int, Exception, bool>("Message!", 1, e, true);    // Noncompliant
                                                                                         // Secondary @-1
                    Log.{{methodName}}("Message!", 1, 2, 3, e, true);                    // Noncompliant
                                                                                         // Secondary @-1
                    Log.{{methodName}}(e, "Message");
                    Log.{{methodName}}<Exception>(e, "Message", e);
                    Log.{{methodName}}<int, Exception>(e, "Message", 1, e);
                    Log.{{methodName}}<int, Exception, bool>(e, "Message", 1, e, true);
                    Log.{{methodName}}(e, "Message!", 1, 2, 3, e, true);
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.Serilog())
           .Verify();

    [TestMethod]
    public void LoggingArgumentsShouldBePassedCorrectly_Serilog_Write_CS() =>
    builder.AddSnippet("""
        using System;
        using Serilog;
        using Serilog.Events;

        public class Program
        {
            public void Method(LogEvent logEvent, LogEventLevel level, Exception exception)
            {
                Log.Write(logEvent);
                Log.Write(level, "Message");
                Log.Write(level, "Message", level);
                Log.Write(level, "Message", 1);
                Log.Write(level, "Message", level, exception);                      // Noncompliant
                                                                                    // Secondary @-1
                Log.Write(level, "Message", 1, exception, 2);                       // Noncompliant
                                                                                    // Secondary @-1
                Log.Write(level, "Message", level, exception, 1, 2);                // Noncompliant
                                                                                    // Secondary @-1
                Log.Write(level, exception, "Message");
                Log.Write(level, exception, "Message", level);
                Log.Write(level, exception, "Message", level, exception);
                Log.Write(level, exception, "Message", level, exception, 1);
                Log.Write(level, exception, "Message", level, exception, 1, 2);
            }
        }
        """)
       .AddReferences(NuGetMetadataReference.Serilog())
       .Verify();
}
