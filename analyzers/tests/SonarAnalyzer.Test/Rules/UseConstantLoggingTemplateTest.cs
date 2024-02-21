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

using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseConstantLoggingTemplateTest
{
    private readonly VerifierBuilder builder = CreateVerifier<CS.UseConstantLoggingTemplate>();

    [TestMethod]
    public void UseConstantLoggingTemplate_CS() =>
        builder.AddPaths("UseConstantLoggingTemplate.cs").Verify();

    [DataTestMethod]
    [DataRow("Log", "LogLevel.Warning,")]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void UseConstantLoggingTemplate_MicrosoftExtensionsLogging_CS(string methodName, string extraParameter = "") =>
        builder.AddSnippet($$"""
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}({{extraParameter}} "Message");    // Compliant
                    logger.{{methodName}}({{extraParameter}} $"{arg}");     // Noncompliant
                }
            }
            """).Verify();

    [DataTestMethod]
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

    [DataTestMethod]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Warn")]
    public void UseConstantLoggingTemplate_Log4Net_CS(string methodName) =>
        builder.AddSnippet($$"""
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
            }
            """).Verify();

    [DataTestMethod]
    [DataRow("ConditionalDebug")]
    [DataRow("ConditionalTrace")]
    [DataRow("Debug")]
    [DataRow("Error")]
    [DataRow("Fatal")]
    [DataRow("Info")]
    [DataRow("Trace")]
    [DataRow("Warn")]
    public void UseConstantLoggingTemplate_NLog_CS(string methodName) =>
        builder.AddSnippet($$"""
            using NLog;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}("Message");       // Compliant
                    logger.{{methodName}}($"{arg}");        // Compliant
                }
            }
            """).Verify();

    [DataTestMethod]
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
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(Constants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.CastleCore(Constants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.Serilog(Constants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.Log4Net("2.0.8", "net45-full"))
            .AddReferences(NuGetMetadataReference.NLog(Constants.NuGetLatestVersion));
}
