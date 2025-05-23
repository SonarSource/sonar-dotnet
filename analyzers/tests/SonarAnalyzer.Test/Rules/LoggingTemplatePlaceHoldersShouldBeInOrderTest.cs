﻿/*
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
public class LoggingTemplatePlaceHoldersShouldBeInOrderTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<MessageTemplateAnalyzer>().WithOnlyDiagnostics(LoggingTemplatePlaceHoldersShouldBeInOrder.S6673);

    [TestMethod]
    public void LoggingTemplatePlaceHoldersShouldBeInOrder_CS() =>
        Builder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .AddPaths("LoggingTemplatePlaceHoldersShouldBeInOrder.cs")
            .Verify();

    [DataTestMethod]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void LoggingTemplatePlaceHoldersShouldBeInOrder_MicrosoftExtensionsLogging_CS(string methodName) =>
        Builder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .AddSnippet($$"""
                using System;
                using Microsoft.Extensions.Logging;

                public class Program
                {
                    public void Method(ILogger logger, int first, int second)
                    {
                        logger.{{methodName}}("{First} {Second}", first, second);   // Compliant
                        logger.{{methodName}}("{First} {Second}", second, first);   // Noncompliant
                                                                                    // Secondary @-1
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
    public void LoggingTemplatePlaceHoldersShouldBeInOrder_Serilog_CS(string methodName) =>
        Builder.AddReferences(NuGetMetadataReference.Serilog(Constants.NuGetLatestVersion))
            .AddSnippet($$"""
                using Serilog;
                using Serilog.Events;

                public class Program
                {
                    public void Method(ILogger logger, int first, int second)
                    {
                        logger.{{methodName}}("{First} {Second}", first, second);   // Compliant
                        logger.{{methodName}}("{First} {Second}", second, first);   // Noncompliant
                                                                                    // Secondary @-1
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
    public void LoggingTemplatePlaceHoldersShouldBeInOrder_NLog_CS(string methodName) =>
        Builder.AddReferences(NuGetMetadataReference.NLog(Constants.NuGetLatestVersion))
            .AddSnippet($$"""
                using NLog;

                public class Program
                {
                    public void Method(ILogger iLogger, Logger logger, MyLogger myLogger, int first, int second)
                    {
                        iLogger.{{methodName}}("{First} {Second}", first, second);  // Compliant
                        iLogger.{{methodName}}("{First} {Second}", second, first);  // Noncompliant
                                                                                    // Secondary @-1

                        logger.{{methodName}}("{First} {Second}", first, second);   // Compliant
                        logger.{{methodName}}("{First} {Second}", second, first);   // Noncompliant
                                                                                    // Secondary @-1

                        myLogger.{{methodName}}("{First} {Second}", first, second); // Compliant
                        myLogger.{{methodName}}("{First} {Second}", second, first); // Noncompliant
                                                                                    // Secondary @-1
                    }
                }

                public class MyLogger : Logger { }
                """).Verify();

    [TestMethod]
    public void LoggingTemplatePlaceHoldersShouldBeInOrder_FakeLoggerWithSameName() =>
        Builder.AddSnippet("""
            public class Program
            {
                public void Method(ILogger logger, int first, int second)
                {
                    logger.Info("{First} {Second}", first, second);     // Compliant
                    logger.Info("{First} {Second}", second, first);     // Compliant - the method is not from any of the known logging frameworks
                }
            }

            public interface ILogger
            {
                void Info(string message, params object[] args);
            }
            """).VerifyNoIssues();
}
