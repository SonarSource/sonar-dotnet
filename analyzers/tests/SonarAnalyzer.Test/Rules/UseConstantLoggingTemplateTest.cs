/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using CS = SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseConstantLoggingTemplateTest
{
    private readonly VerifierBuilder builder = CreateVerifier<CS.UseConstantLoggingTemplate>();

    [TestMethod]
    public void UseConstantLoggingTemplate_CS() =>
        builder.AddPaths("UseConstantLoggingTemplate.cs").Verify();

    [TestMethod]
    [DataRow("Debug")]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("Warn")]
    public void UseConstantLoggingTemplate_CastleCoreLogging_CS(string methodName) =>
        builder.AddSnippet($$"""
            using Castle.Core.Logging;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}("Message");            // Compliant
                    logger.{{methodName}}($"{arg}");             // Noncompliant
                    logger.{{methodName}}Format("{Arg}", arg);   // Compliant
                    logger.{{methodName}}Format($"{arg}");       // Noncompliant
                }
            }
            """).Verify();

    [TestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Warn")]
    public void UseConstantLoggingTemplate_Log4Net_CS(string methodName) =>
        builder.AddSnippet($$"""
            using System;
            using log4net;

            public class Program
            {
                public void Method(ILog logger, int arg)
                {
                    logger.{{methodName}}("Message");               // Compliant
                    logger.{{methodName}}($"{arg}");                // Noncompliant
                    logger.{{methodName}}Format("Arg: {0}", arg);   // Compliant
                    logger.{{methodName}}Format($"{arg}");          // Noncompliant
                }

                //https://github.com/SonarSource/sonar-dotnet/issues/9547
                void Repro_9547(ILog logger, string filePath, Exception ex)
                {
                  logger.{{methodName}}($"Error while loading file '{filePath}'!", ex); // Compliant
                }
            }
            """).Verify();

    [TestMethod]
    [DataRow("Log", "LogLevel.Warning,")]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void UseConstantLoggingTemplate_MicrosoftExtensionsLogging_CS(string methodName, string logLevel = "") =>
        builder.AddSnippet($$"""
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}({{logLevel}} "Message");    // Compliant
                    logger.{{methodName}}({{logLevel}} $"{arg}");     // Noncompliant
                }
            }
            """).Verify();

    [TestMethod]
    [DataRow("ConditionalDebug")]
    [DataRow("ConditionalTrace")]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Log", "LogLevel.Warn,")]
    [DataRow("Trace")]
    [DataRow("Warn")]
    public void UseConstantLoggingTemplate_NLog_CS(string methodName, string logLevel = "") =>
        builder.AddSnippet($$"""
            using NLog;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}({{logLevel}} "Message");       // Compliant
                    logger.{{methodName}}({{logLevel}} $"{arg}");        // Noncompliant
                }
            }
            """).Verify();

    public void UseConstantLoggingTemplate_NLog_AdditionalLoggers_CS() =>
        builder.AddSnippet("""
            using NLog;

            public class Program
            {
                public void Method(ILoggerBase logger, NullLogger nullLogger, int arg)
                {
                    logger.Log(LogLevel.Warn, "Message");       // Compliant
                    logger.Log(LogLevel.Warn, $"{arg}");        // Noncompliant

                    nullLogger.Log(LogLevel.Warn, "Message");   // Compliant
                    nullLogger.Log(LogLevel.Warn, $"{arg}");    // Noncompliant
                }
            }
            """).Verify();

    [TestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Information")]
    [DataRow("Verbose")]
    [DataRow("Warning")]
    public void UseConstantLoggingTemplate_Serilog_CS(string methodName) =>
        builder.AddSnippet($$"""
            using Serilog;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}("Message without argument");             // Compliant
                    logger.{{methodName}}("The argument is {@Argument}", arg);     // Compliant
                    logger.{{methodName}}($"The argument is {arg}");               // Noncompliant

                    Log.{{methodName}}("Message without argument");                // Compliant
                    Log.{{methodName}}("The argument is {@Argument}", arg);        // Compliant
                    Log.{{methodName}}($"The argument is {arg}");                  // Noncompliant
                }
            }
            """).Verify();

    private static VerifierBuilder CreateVerifier<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        new VerifierBuilder<TAnalyzer>()
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.CastleCore(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.Serilog())
            .AddReferences(NuGetMetadataReference.Log4Net("2.0.8", "net45-full"))
            .AddReferences(NuGetMetadataReference.NLog());
}
