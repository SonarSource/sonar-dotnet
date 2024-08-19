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
using SonarAnalyzer.Rules.MessageTemplates;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class NamedPlaceholdersShouldBeUniqueTest
{
    private static readonly IEnumerable<MetadataReference> LoggingReferences =
        NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions()
        .Concat(NuGetMetadataReference.NLog())
        .Concat(NuGetMetadataReference.Serilog());

    private static readonly VerifierBuilder Builder = new VerifierBuilder<MessageTemplateAnalyzer>()
        .AddReferences(LoggingReferences)
        .WithOnlyDiagnostics(NamedPlaceholdersShouldBeUnique.S6677);

    [TestMethod]
    public void NamedPlaceholdersShouldBeUnique_CS() =>
        Builder.AddPaths("NamedPlaceholdersShouldBeUnique.cs").Verify();

    [DataTestMethod]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void NamedPlaceholdersShouldBeUnique_MicrosoftExtensionsLogging_CS(string methodName) =>
        Builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, MyLogger myLogger, int arg)
                {
                    logger.{{methodName}}("Hey {foo} and {bar}", arg, arg);                       // Compliant
                    logger.{{methodName}}("Hey {foo} and {foo}", arg, arg);                       // Noncompliant
                                                                                                  // Secondary @-1

                    myLogger.{{methodName}}("Hey {foo} and {bar}", arg, arg);                     // Compliant
                    myLogger.{{methodName}}("Hey {foo} and {foo}", arg, arg);                     // Noncompliant
                                                                                                  // Secondary @-1
                }
            }

            public class MyLogger : ILogger
            {
                public IDisposable BeginScope<TState>(TState state) => null;
                public bool IsEnabled(LogLevel logLevel) => true;
                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
            }
            """).Verify();

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Information")]
    [DataRow("Fatal")]
    [DataRow("Warning")]
    [DataRow("Verbose")]
    public void NamedPlaceholdersShouldBeUnique_Serilog_CS(string methodName) =>
        Builder.AddSnippet($$"""
            using Serilog;
            using Serilog.Events;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}("Hey {foo} and {bar}", arg, arg);                       // Compliant
                    logger.{{methodName}}("Hey {foo} and {foo}", arg, arg);                       // Noncompliant
                                                                                                  // Secondary @-1

                    Log.{{methodName}}("Hey {foo} and {bar}", arg, arg);                          // Compliant
                    Log.{{methodName}}("Hey {foo} and {foo}", arg, arg);                          // Noncompliant
                                                                                                  // Secondary @-1

                    Log.Logger.{{methodName}}("Hey {foo} and {bar}", arg, arg);                   // Compliant
                    Log.Logger.{{methodName}}("Hey {foo} and {foo}", arg, arg);                   // Noncompliant
                                                                                                  // Secondary @-1
                }
            }
            """).Verify();

#if NET
    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Information")]
    [DataRow("Fatal")]
    [DataRow("Warning")]
    [DataRow("Verbose")]
    public void NamedPlaceholdersShouldBeUnique_Serilog_Derived_CS(string methodName) =>
    Builder.AddSnippet($$"""
            using Serilog;
            using Serilog.Events;
            using Serilog.Core;

            public class Program
            {
                public void Method(Logger logger, int arg)
                {
                    logger.{{methodName}}("Hey {foo} and {bar}", arg, arg);                       // Compliant
                    logger.{{methodName}}("Hey {foo} and {foo}", arg, arg);                       // Noncompliant
                                                                                                  // Secondary @-1
                }
            }
            """).Verify();
#endif

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("ConditionalDebug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("ConditionalTrace")]
    [DataRow("Warn")]
    public void NamedPlaceholdersShouldBeUnique_NLog_CS(string methodName) =>
        Builder.AddSnippet($$"""
            using NLog;

            public class Program
            {
                public void Method(ILogger iLogger, Logger logger, MyLogger myLogger, int arg)
                {
                    iLogger.{{methodName}}("Hey {foo} and {bar}", arg, arg);      // Compliant
                    iLogger.{{methodName}}("Hey {foo} and {foo}", arg, arg);      // Noncompliant
                                                                                  // Secondary @-1

                    logger.{{methodName}}("Hey {foo} and {bar}", arg, arg);       // Compliant
                    logger.{{methodName}}("Hey {foo} and {foo}", arg, arg);       // Noncompliant
                                                                                  // Secondary @-1

                    myLogger.{{methodName}}("Hey {foo} and {bar}", arg, arg);     // Compliant
                    myLogger.{{methodName}}("Hey {foo} and {foo}", arg, arg);     // Noncompliant
                                                                                  // Secondary @-1
                }
            }
            public class MyLogger : Logger { }
            """).Verify();
}
