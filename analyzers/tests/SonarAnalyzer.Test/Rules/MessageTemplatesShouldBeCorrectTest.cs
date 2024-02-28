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
public class MessageTemplatesShouldBeCorrectTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<MessageTemplatesShouldBeCorrect>();

    [TestMethod]
    public void MessageTemplatesShouldBeCorrect_CS() =>
        Builder
        .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
        .AddPaths("MessageTemplatesShouldBeCorrect.cs")
        .Verify();

    [DataTestMethod]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void MessageTemplatesShouldBeCorrect_MicrosoftExtensionsLogging_CS(string methodName) =>
        Builder
        .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
        .AddSnippet($$$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, string user, int count)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    logger.{{{methodName}}}("{", user);                                 // Noncompliant
                    logger.Log(LogLevel.Information, "{", user);                        // Noncompliant
                    LoggerExtensions.{{{methodName}}}(logger, "{", user);               // Noncompliant
                }
            }
            """).Verify();

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Information")]
    [DataRow("Fatal")]
    [DataRow("Warning")]
    [DataRow("Verbose")]
    public void MessageTemplatesShouldBeCorrect_Serilog_CS(string methodName) =>
        Builder
        .AddReferences(NuGetMetadataReference.Serilog(Constants.NuGetLatestVersion))
        .AddSnippet($$$"""
            using System;
            using Serilog;
            using Serilog.Events;

            public class Program
            {
                public void Method(ILogger logger, string user, int count)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    logger.{{{methodName}}}("{", user);                                 // Noncompliant
                    Log.{{{methodName}}}("{", user);                                    // Noncompliant
                    Log.Write(LogEventLevel.Verbose, "{", user);                        // Noncompliant
                }
            }
            """).Verify();

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("ConditionalDebug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("ConditionalTrace")]
    [DataRow("Warn")]
    public void MessageTemplatesShouldBeCorrect_NLog_CS(string methodName) =>
        Builder
        .AddReferences(NuGetMetadataReference.NLog(Constants.NuGetLatestVersion))
        .AddSnippet($$$"""
            using System;
            using NLog;

            public class Program
            {
                public void Method(ILogger iLogger, Logger logger, MyLogger myLogger, string user, int count)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    iLogger.{{{methodName}}}("{", user);                                // Noncompliant
                    logger.{{{methodName}}}("{", user);                                 // Noncompliant
                    myLogger.{{{methodName}}}("{", user);                               // Noncompliant
                    ILoggerExtensions.ConditionalDebug(iLogger, "{", user);             // Noncompliant
                }
            }
            public class MyLogger : Logger { }
            """).Verify();
}
