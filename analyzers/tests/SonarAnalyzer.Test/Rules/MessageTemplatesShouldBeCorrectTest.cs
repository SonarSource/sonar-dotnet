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
public class MessageTemplatesShouldBeCorrectTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<MessageTemplatesShouldBeCorrect>();

    [TestMethod]
    public void MessageTemplatesShouldBeCorrect_CS() =>
        Builder
        .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
        .AddPaths("MessageTemplatesShouldBeCorrect.cs")
        .Verify();

    [TestMethod]
    [DataRow("LogCritical", "")]
    [DataRow("LogDebug", "")]
    [DataRow("LogError", "")]
    [DataRow("LogInformation", "")]
    [DataRow("LogTrace", "")]
    [DataRow("LogWarning", "")]
    [DataRow("Log", "LogLevel.Information,")]
    public void MessageTemplatesShouldBeCorrect_MicrosoftExtensionsLogging_CS(string methodName, string logLevel) =>
        Builder
        .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
        .AddSnippet($$$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, string user)
                {
                    Console.WriteLine("Login failed for {User", user);                                  // Compliant
                    logger.{{{methodName}}}({{{logLevel}}} "Login failed for {User}", user);            // Compliant

                    logger.{{{methodName}}}({{{logLevel}}} "{", user);                                  // Noncompliant
                    LoggerExtensions.{{{methodName}}}(logger, {{{logLevel}}} "{", user);                // Noncompliant
                }
            }
            """)
        .Verify();

    [TestMethod]
    [DataRow("Debug", "")]
    [DataRow("Error", "")]
    [DataRow("Information", "")]
    [DataRow("Fatal", "")]
    [DataRow("Warning", "")]
    [DataRow("Verbose", "")]
    [DataRow("Write", "LogEventLevel.Verbose,")]
    public void MessageTemplatesShouldBeCorrect_Serilog_CS(string methodName, string logEventLevel) =>
        Builder
        .AddReferences(NuGetMetadataReference.Serilog())
        .AddSnippet($$$"""
            using System;
            using Serilog;
            using Serilog.Events;

            public class Program
            {
                public void Method(ILogger logger, string user)
                {
                    Console.WriteLine("Login failed for {User", user);                                     // Compliant
                    logger.{{{methodName}}}({{{logEventLevel}}} "Login failed for {User}", user);          // Compliant

                    logger.{{{methodName}}}({{{logEventLevel}}} "{", user);                                // Noncompliant
                    Log.{{{methodName}}}({{{logEventLevel}}} "{", user);                                   // Noncompliant
                }
            }
            """)
        .Verify();

    [TestMethod]
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
        .AddReferences(NuGetMetadataReference.NLog())
        .AddSnippet($$$"""
            using System;
            using NLog;

            public class Program
            {
                public void Method(ILogger iLogger, Logger logger, MyLogger myLogger, string user)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    iLogger.{{{methodName}}}("{", user);                                // Noncompliant
                    logger.{{{methodName}}}("{", user);                                 // Noncompliant
                    myLogger.{{{methodName}}}("{", user);                               // Noncompliant
                }
            }
            public class MyLogger : Logger { }
            """)
        .Verify();

    [TestMethod]
    [DataRow("ConditionalDebug")]
    [DataRow("ConditionalTrace")]
    public void MessageTemplatesShouldBeCorrect_NLog_ConditionalExtensions_CS(string methodName) =>
        Builder
        .AddReferences(NuGetMetadataReference.NLog())
        .AddSnippet($$$"""
            using System;
            using NLog;

            public class Program
            {
                public void Method(ILogger iLogger, string user)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant

                    ILoggerExtensions.{{{methodName}}}(iLogger, "{", user);             // Noncompliant
                }
            }
            public class MyLogger : Logger { }
            """)
        .Verify();

#if NET

    [TestMethod]
    public void MessageTemplatesShouldBeCorrect_Latest() =>
        Builder.AddPaths("MessageTemplatesShouldBeCorrect.Latest.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

}
