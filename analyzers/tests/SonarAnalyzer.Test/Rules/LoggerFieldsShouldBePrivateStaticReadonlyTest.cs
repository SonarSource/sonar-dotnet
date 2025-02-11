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
public class LoggerFieldsShouldBePrivateStaticReadonlyTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<LoggerFieldsShouldBePrivateStaticReadonly>();

    [TestMethod]
    public void LoggerFieldsShouldBePrivateStaticReadonly_MicrosoftExtensionsLogging_CS() =>
        Builder
            .AddPaths("LoggerFieldsShouldBePrivateStaticReadonly.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .Verify();

#if NET
    [TestMethod]
    public void LoggerFieldsShouldBePrivateStaticReadonly_CSharp8() =>
        Builder
            .AddSnippet("""
                using Microsoft.Extensions.Logging;

                public class Program
                {
                    private protected static readonly ILogger logger;   // Noncompliant
                    //                                        ^^^^^^
                }

                public interface Service
                {
                    static ILogger StaticLogger;
                    protected internal static ILogger Logger;
                    private static ILogger Test;
                }
                """)
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();
#endif

    [TestMethod]
    public void LoggerFieldsShouldBePrivateStaticReadonly_Serilog_CS() =>
        Builder
            .AddSnippet("""
                using Serilog;

                public class Program
                {
                    ILogger log1, log2;
                    //      ^^^^ {{Make the logger 'log1' private static readonly.}}
                    //            ^^^^ @-1 {{Make the logger 'log2' private static readonly.}}
                }
                """)
            .AddReferences(NuGetMetadataReference.Serilog())
            .Verify();

    [TestMethod]
    public void LoggerFieldsShouldBePrivateStaticReadonly_NLog_CS() =>
        Builder
            .AddSnippet("""
                using NLog;

                public class Program
                {
                    ILogger log1, log2;
                    //      ^^^^ {{Make the logger 'log1' private static readonly.}}
                    //            ^^^^ @-1 {{Make the logger 'log2' private static readonly.}}

                    Logger log3;        // Noncompliant
                    ILoggerBase log4;   // Noncompliant
                    MyLogger log5;      // Noncompliant
                }
                public class MyLogger : Logger { }
                """)
            .AddReferences(NuGetMetadataReference.NLog())
            .Verify();

    [TestMethod]
    public void LoggerFieldsShouldBePrivateStaticReadonly_log4net_CS() =>
        Builder
            .AddSnippet("""
                using log4net;
                using log4net.Core;
                using log4net.Repository.Hierarchy;

                public class Program
                {
                    ILog log1,log2;
                    //   ^^^^ {{Make the logger 'log1' private static readonly.}}
                    //        ^^^^ @-1 {{Make the logger 'log2' private static readonly.}}

                    ILogger log3;                           // Noncompliant
                    Logger log4;                            // Noncompliant
                    MyLogger log5;                          // Noncompliant
                }
                public class MyLogger : Logger
                {
                    public MyLogger(string name) : base(name) { }
                }
                """)
            .AddReferences(NuGetMetadataReference.Log4Net(TestConstants.NuGetLatestVersion, "netstandard2.0"))
            .Verify();

    [TestMethod]
    public void LoggerFieldsShouldBePrivateStaticReadonly_CastleCore_CS() =>
        Builder
            .AddSnippet("""
                using Castle.Core.Logging;

                public class Program
                {
                    ILogger log;                            // Noncompliant {{Make the logger 'log' private static readonly.}}
                    //      ^^^
                }
                """)
            .AddReferences(NuGetMetadataReference.CastleCore())
            .Verify();
}
