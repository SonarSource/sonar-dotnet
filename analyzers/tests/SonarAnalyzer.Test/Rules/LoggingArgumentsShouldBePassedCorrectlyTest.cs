/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LoggingArgumentsShouldBePassedCorrectlyTest
{
    private const string EventIdParameter = "new EventId(1),";
    private const string LogLevelParameter = "LogLevel.Warning,";
    private const string NoParameter = "";
    private readonly VerifierBuilder builder = new VerifierBuilder<LoggingArgumentsShouldBePassedCorrectly>();

    [TestMethod]
    public void LoggingArgumentsShouldBePassedCorrectly_CS() =>
        builder.AddPaths("LoggingArgumentsShouldBePassedCorrectly.cs").AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions()).Verify();

    [DataTestMethod]
    [DataRow("Log", LogLevelParameter)]
    [DataRow("Log", LogLevelParameter, EventIdParameter)]
    [DataRow("LogCritical")]
    [DataRow("LogCritical", NoParameter, EventIdParameter)]
    [DataRow("LogDebug")]
    [DataRow("LogDebug", NoParameter, EventIdParameter)]
    [DataRow("LogError")]
    [DataRow("LogError", NoParameter, EventIdParameter)]
    [DataRow("LogInformation")]
    [DataRow("LogInformation", NoParameter, EventIdParameter)]
    [DataRow("LogTrace")]
    [DataRow("LogTrace", NoParameter, EventIdParameter)]
    [DataRow("LogWarning")]
    [DataRow("LogWarning", NoParameter, EventIdParameter)]
    public void LoggingArgumentsShouldBePassedCorrectly_MicrosoftExtensionsLogging_NonCompliant_CS(string methodName, string logLevel = "", string eventId = "") =>
        builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;
            using Microsoft.Extensions.Logging.Abstractions;

            public class Program
            {
                public void Method(ILogger logger, Exception e)
                {
                    logger.{{methodName}}({{logLevel}}{{eventId}} "An exception occured {Exception}.", e);          // Noncompliant
                    logger.{{methodName}}({{logLevel}}{{eventId}} "Expected exception.", e);                        // Noncompliant
                    logger.{{methodName}}({{logLevel}}{{eventId}} "Expected exception.", e, e);                     // Noncompliant
                                                                                                                    // Noncompliant@-1
                    LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} "Expected exception.", e);      // Noncompliant
                    LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} "Expected exception.", e, e);   // Noncompliant
                                                                                                                    // Noncompliant@-1
                    logger.{{methodName}}({{logLevel}}{{eventId}} e, "Expected exception.");
                    logger.{{methodName}}({{logLevel}}{{eventId}} e, "Expected exception.", e);
                    LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} e, "Expected exception.");
                    LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} e, "Expected exception.", e);
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
           .Verify();

    [DataTestMethod]
    [DataRow("Error")]
    [DataRow("Debug")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("Warn")]
    // https://github.com/castleproject/Core/blob/dca4ed09df545dd7512c82778127219795668d30/src/Castle.Core/Core/Logging/ILogger.cs
    public void LoggingArgumentsShouldBePassedCorrectly_CastleCore_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using Castle.Core.Logging;

            public class Program
            {
                public void Method(ILogger logger, string message, Exception e)
                {
                    logger.{{methodName}}Format(message, e);                                    // Noncompliant
                    logger.{{methodName}}Format(CultureInfo.CurrentCulture, message, e);        // Noncompliant
                    logger.{{methodName}}(message, e);                                          // Compliant
                    logger.{{methodName}}Format(e, message);                                    // Compliant
                    logger.{{methodName}}Format(e, message, e);                                 // Compliant
                    logger.{{methodName}}Format(e, CultureInfo.CurrentCulture, message);        // Compliant
                    logger.{{methodName}}Format(e, CultureInfo.CurrentCulture, message, e);     // Compliant
                }
            }
            """)
        .AddReferences(NuGetMetadataReference.CastleCore())
        .Verify();

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("ConditionalDebug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("ConditionalTrace")]
    [DataRow("Warn")]
    public void LoggingArgumentsShouldBePassedCorrectly_NLog_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using NLog;

            public class Program
            {
                public void Method(Logger logger, Exception e)
                {
                    logger.{{methodName}}(e);                                                    // Compliant
                    logger.{{methodName}}(CultureInfo.CurrentCulture, e);                        // Compliant
                    logger.{{methodName}}(e, "Message!");                                        // Compliant
                    logger.{{methodName}}(e, "Message!", e);                                     // Compliant
                    logger.{{methodName}}(e, CultureInfo.CurrentCulture, "Message!", e);         // Compliant
                    logger.{{methodName}}(CultureInfo.CurrentCulture, "Message!", 1, 2, 3, e);   // Noncompliant
                    logger.{{methodName}}("Message!");                                           // Compliant
                    logger.{{methodName}}("Message!", 1, 2, 3, e);                               // Noncompliant
                    logger.{{methodName}}("Message!", e);                                        // Noncompliant
                    logger.{{methodName}}(CultureInfo.CurrentCulture, "Message!", e);            // Noncompliant
                    logger.{{methodName}}<int>("Message!", 1);                                   // Compliant
                    logger.{{methodName}}<Exception>("Message!", e);                             // Noncompliant
                    logger.{{methodName}}(CultureInfo.CurrentCulture, "Message!", 1, e);         // Noncompliant
                    logger.{{methodName}}("Message!", 1, e);                                     // Noncompliant
                    logger.{{methodName}}(CultureInfo.CurrentCulture, "Message!", 1, 2, e);      // Noncompliant
                    logger.{{methodName}}("Message!", 1, 2, e);                                  // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, e, null);                           // Compliant
                }
            }
            """)
        .AddReferences(NuGetMetadataReference.NLog(Constants.NuGetLatestVersion))
        .Verify();

    [DataTestMethod]
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
                    ILoggerExtensions.{{methodName}}(logger, "Message!");                                           // Compliant
                    ILoggerExtensions.{{methodName}}(logger, "Message!", 1, 2, 3, e);                               // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, "Message!", e);                                        // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", e);            // Noncompliant
                    ILoggerExtensions.{{methodName}}<int>(logger, "Message!", 1);                                   // Compliant
                    ILoggerExtensions.{{methodName}}<Exception>(logger, "Message!", e);                             // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", 1, e);         // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, "Message!", 1, e);                                     // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, CultureInfo.CurrentCulture, "Message!", 1, 2, e);      // Noncompliant
                    ILoggerExtensions.{{methodName}}(logger, "Message!", 1, 2, e);                                  // Noncompliant
                }
            }
            """)
        .AddReferences(NuGetMetadataReference.NLog(Constants.NuGetLatestVersion))
        .Verify();

    [DataTestMethod]
    [DataRow("Error")]
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
                    Log.{{methodName}}<int>("Message!", 1);
                    Log.{{methodName}}<Exception>("Message!", e);                        // Noncompliant
                    Log.{{methodName}}<int, Exception>("Message!", 1, e);                // Noncompliant
                    Log.{{methodName}}<int, Exception, bool>("Message!", 1, e, true);    // Noncompliant
                    Log.{{methodName}}("Message!", 1, 2, 3, e, true);                    // Noncompliant
                    Log.{{methodName}}(e, "Message");
                    Log.{{methodName}}<Exception>(e, "Message", e);
                    Log.{{methodName}}<int, Exception>(e, "Message", 1, e);
                    Log.{{methodName}}<int, Exception, bool>(e, "Message", 1, e, true);
                    Log.{{methodName}}(e, "Message!", 1, 2, 3, e, true);
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.Serilog(Constants.NuGetLatestVersion))
           .Verify();
}
