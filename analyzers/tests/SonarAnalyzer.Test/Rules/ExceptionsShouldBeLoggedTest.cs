/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExceptionsShouldBeLoggedTest
{
    private const string EventIdParameter = "new EventId(1),";
    private const string LogLevelParameter = "LogLevel.Warning,";
    private const string NoParameter = "";
    private readonly VerifierBuilder builder = new VerifierBuilder<ExceptionsShouldBeLogged>();

    [TestMethod]
    public void ExceptionsShouldBeLogged_CS() =>
        builder
            .AddPaths("ExceptionsShouldBeLogged.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .Verify();

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
    public void ExceptionsShouldBeLogged_MicrosoftExtensionsLogging_NonCompliant_CS(string methodName, string logLevel = "", string eventId = "") =>
        builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;
            using Microsoft.Extensions.Logging.Abstractions;

            public class Program
            {
                public void Method(ILogger logger, string message)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}({{logLevel}} {{eventId}} message);                        // Noncompliant
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}({{logLevel}} {{eventId}} e, message);                     // Compliant
                    }
                    try { }
                    catch (Exception e)
                    {
                        LoggerExtensions.{{methodName}}(logger, {{logLevel}} {{eventId}} message);      // Noncompliant
                    }
                    try { }
                    catch (Exception e)
                    {
                        LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} e, message);    // Compliant
                    }
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
    public void ExceptionsShouldBeLogged_CastleCore_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using Castle.Core.Logging;

            public class Program
            {
                public void Method(ILogger logger, string message)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}(message);                                     // Noncompliant
                        logger.{{methodName}}Format(message);                               // Secondary
                        logger.{{methodName}}Format(CultureInfo.CurrentCulture, message);   // Secondary
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}(message, e);                                     // Compliant
                        logger.{{methodName}}Format(e, message);                               // Compliant
                        logger.{{methodName}}Format(e, CultureInfo.CurrentCulture, message);   // Compliant
                    }
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.CastleCore())
           .Verify();

    [DataTestMethod]
    [DataRow("Error")]
    [DataRow("Debug")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("Warn")]
    // https://www.fuget.org/packages/Common.Logging.Core/3.4.1/lib/netstandard1.0/Common.Logging.Core.dll/Common.Logging/ILog
    public void ExceptionsShouldBeLogged_CommonLoggingCore_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using Common.Logging;

            public class Program
            {
                public void Method(ILog logger, string message)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}("Message");                                           // Noncompliant
                        logger.{{methodName}}(_ => { });                                            // Secondary
                        logger.{{methodName}}(CultureInfo.CurrentCulture, _ => { });                // Secondary
                        logger.{{methodName}}Format("Message");                                     // Secondary
                        logger.{{methodName}}Format(CultureInfo.CurrentCulture, "Message");         // Secondary
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}("Message", e);
                        logger.{{methodName}}(_ => { }, e);
                        logger.{{methodName}}(CultureInfo.CurrentCulture, _ => { }, e);
                        logger.{{methodName}}Format("Message", e);
                        logger.{{methodName}}Format(CultureInfo.CurrentCulture, "Message", e);
                    }
                }
            }
            """)
            .AddReferences(NuGetMetadataReference.CommonLoggingCore())
            .Verify();

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Warn")]
    // https://logging.apache.org/log4net/release/sdk/html/T_log4net_ILog.htm
    public void ExceptionsShouldBeLogged_Log4net_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using log4net;
            using log4net.Util;

            public class Program
            {
                public void Method(ILog logger, string message)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}(message);                                     // Noncompliant
                        logger.{{methodName}}Ext(message);                                  // Secondary
                        ILogExtensions.{{methodName}}Ext(logger, message);                  // Secondary
                        logger.{{methodName}}Format(message);                               // Compliant - Format overloads do not take an exception.
                        logger.{{methodName}}Format(CultureInfo.CurrentCulture, message);   // Compliant - Format overloads do not take an exception.
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}(message, e);                                  // Compliant
                        logger.{{methodName}}Ext(message, e);                               // Compliant
                        ILogExtensions.{{methodName}}Ext(logger, message, e);               // Compliant
                    }
                }
            }
            """)
            .AddReferences(NuGetMetadataReference.Log4Net("3.0.1", "netstandard2.0")) // After 3.0.2 they removed ILogExtensions.
            .Verify();

    [DataTestMethod]
    [DataRow("ILogger", "Debug")]
    [DataRow("ILogger", "Error")]
    [DataRow("ILogger", "Fatal")]
    [DataRow("ILogger", "Info")]
    [DataRow("ILogger", "Trace")]
    [DataRow("ILogger", "Warn")]
    [DataRow("Logger", "Debug")]
    [DataRow("Logger", "Error")]
    [DataRow("Logger", "Fatal")]
    [DataRow("Logger", "Info")]
    [DataRow("Logger", "Trace")]
    [DataRow("Logger", "Warn")]
    // https://nlog-project.org/documentation/v5.0.0/html/Methods_T_NLog_Logger.htm
    public void ExceptionsShouldBeLogged_NLog_CS(string type, string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using NLog;

            public class Program
            {
                public void Method({{type}} logger, string message, Object[] parameters)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}("Message");                               // Noncompliant
                        logger.{{methodName}}("Message", parameters);                   // Secondary
                        logger.{{methodName}}(CultureInfo.CurrentCulture, "Message");   // Secondary
                        ILoggerExtensions.{{methodName}}(logger, null, null);           // Secondary
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}(e, "Message");
                        logger.{{methodName}}(e, "Message", parameters);
                        logger.{{methodName}}(e, CultureInfo.CurrentCulture, "Message");
                        ILoggerExtensions.{{methodName}}(logger, null, null);
                    }
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.NLog())
           .Verify();

    [TestMethod]
    public void ExceptionsShouldBeLogged_NLog_ILoggerBase_CS() =>
        builder.AddSnippet("""
            using System;
            using System.Globalization;
            using NLog;

            public class Program
            {
                public void Method(ILoggerBase logger, string message, Object[] parameters)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Debug, "Message");                                          // Noncompliant
                        logger.Log(LogLevel.Debug, CultureInfo.CurrentCulture, "Message");              // Secondary
                        logger.Log<string>(LogLevel.Debug, "Message");                                  // Secondary
                        logger.Log<string>(LogLevel.Debug, CultureInfo.CurrentCulture, "Message");      // Secondary
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Debug, e, "Message");
                        logger.Log(LogLevel.Debug, CultureInfo.CurrentCulture, "Message");
                        logger.Log<string>(LogLevel.Debug, "Message");
                        logger.Log<string>(LogLevel.Debug, CultureInfo.CurrentCulture, "Message");
                    }
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.NLog())
           .Verify();

    [DataTestMethod]
    [DataRow("ConditionalDebug")]
    [DataRow("ConditionalTrace")]
    // https://nlog-project.org/documentation/v5.0.0/html/Methods_T_NLog_Logger.htm
    public void ExceptionsShouldBeLogged_NLog_Conditional_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using System.Globalization;
            using NLog;

            public class Program
            {
                public void Method(Logger logger, string message, Object[] parameters)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}("Message");                               // Noncompliant
                        logger.{{methodName}}(CultureInfo.CurrentCulture, "Message");   // Secondary
                        ILoggerExtensions.{{methodName}}(logger, "Message");            // Secondary
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}(e, "Message");
                        logger.{{methodName}}(e, CultureInfo.CurrentCulture, "Message");
                        ILoggerExtensions.{{methodName}}(logger, e, "Message");
                    }
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.NLog())
           .Verify();
}
