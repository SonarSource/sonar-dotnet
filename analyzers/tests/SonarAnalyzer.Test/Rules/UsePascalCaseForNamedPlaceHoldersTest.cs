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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.CSharp.Rules.MessageTemplates;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UsePascalCaseForNamedPlaceHoldersTest
{
    private static readonly IEnumerable<MetadataReference> LoggingReferences =
        NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions()
        .Concat(NuGetMetadataReference.NLog())
        .Concat(NuGetMetadataReference.Serilog());

    private static readonly VerifierBuilder Builder = new VerifierBuilder<MessageTemplateAnalyzer>()
        .AddReferences(LoggingReferences)
        .WithOnlyDiagnostics(UsePascalCaseForNamedPlaceHolders.S6678);

    [TestMethod]
    public void UsePascalCaseForNamedPlaceHolders_CS() =>
        Builder.AddPaths("UsePascalCaseForNamedPlaceHolders.cs").Verify();

#if NET

    [TestMethod]
    public void UsePascalCaseForNamedPlaceHolders_Latest_CS() =>
        Builder.AddPaths("UsePascalCaseForNamedPlaceHolders.Latest.cs").WithLanguageVersion(LanguageVersion.Latest).VerifyNoIssues();

#endif

    [DataTestMethod]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void UsePascalCaseForNamedPlaceHolders_MicrosoftExtensionsLogging_CS(string methodName) =>
        Builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}("Arg: {Arg}", arg);       // Compliant
                    logger.{{methodName}}("Arg: {arg}", arg);       // Noncompliant
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
    public void UsePascalCaseForNamedPlaceHolders_Serilog_CS(string methodName) =>
        Builder.AddSnippet($$"""
            using Serilog;
            using Serilog.Events;

            public class Program
            {
                public void Method(ILogger logger, int arg)
                {
                    logger.{{methodName}}("Arg: {Arg}", arg);       // Compliant
                    logger.{{methodName}}("Arg: {arg}", arg);       // Noncompliant
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
    public void UsePascalCaseForNamedPlaceHolders_NLog_CS(string methodName) =>
        Builder.AddSnippet($$"""
            using NLog;

            public class Program
            {
                public void Method(ILogger iLogger, Logger logger, MyLogger myLogger, int arg)
                {
                    logger.{{methodName}}("Arg: {Arg}", arg);       // Compliant
                    logger.{{methodName}}("Arg: {arg}", arg);       // Noncompliant
                                                                    // Secondary @-1
                }
            }
            public class MyLogger : Logger { }
            """).Verify();
}
