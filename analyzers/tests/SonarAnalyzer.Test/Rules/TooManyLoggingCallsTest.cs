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
public class TooManyLoggingCallsTest
{
    private readonly VerifierBuilder defaultBuilder = new VerifierBuilder<TooManyLoggingCalls>();
    private readonly VerifierBuilder configuredBuilder = new VerifierBuilder().AddAnalyzer(() => new TooManyLoggingCalls
    {
        DebugThreshold = 1,
    });
    private readonly VerifierBuilder misconfiguredBuilder = new VerifierBuilder().AddAnalyzer(() => new TooManyLoggingCalls
    {
        DebugThreshold = 0,
        ErrorThreshold = -1,
    });

    [TestMethod]
    public void TooManyLoggingCalls_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .AddPaths("TooManyLoggingCalls.cs")
            .Verify();

    [TestMethod]
    public void TooManyLoggingCalls_TopLevelStatements_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithTopLevelStatements()
            .AddSnippet("""
                using Microsoft.Extensions.Logging;
                using Microsoft.Extensions.Logging.Abstractions;

                var logger = NullLogger.Instance;

                logger.LogError("Error 1");         // Noncompliant
                logger.LogError("Error 2");         // Secondary

                logger.LogWarning("Warn 1");        // Compliant
                """).Verify();

    [TestMethod]
    public void TooManyLoggingCalls_CastleCoreLogging_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.CastleCore())
            .AddPaths("TooManyLoggingCalls.Castle.Core.Logging.cs")
            .Verify();

    [TestMethod]
    public void TooManyLoggingCalls_Log4Net_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.Log4Net("2.0.8", "net45-full"))
            .AddPaths("TooManyLoggingCalls.Log4Net.cs")
            .Verify();

    [TestMethod]
    public void TooManyLoggingCalls_MicrosoftExtensionsLogging_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .AddPaths("TooManyLoggingCalls.Microsoft.Extensions.Logging.cs")
            .Verify();

    [TestMethod]
    public void TooManyLoggingCalls_NLog_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.NLog())
        .AddPaths("TooManyLoggingCalls.NLog.cs")
        .Verify();

    [TestMethod]
    public void TooManyLoggingCalls_Serilog_CS() =>
        defaultBuilder.AddReferences(NuGetMetadataReference.Serilog())
            .AddPaths("TooManyLoggingCalls.Serilog.cs")
            .Verify();

    [TestMethod]
    public void TooManyLoggingCalls_ConfiguredThresholds_CS() =>
        configuredBuilder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .AddSnippet("""
                using Microsoft.Extensions.Logging;

                public class Program
                {
                    public void Method(ILogger logger)
                    {
                        // The threshold was set to 1
                        logger.LogDebug("Debug 1");     // Noncompliant {{Reduce the number of Debug logging calls within this code block from 2 to the 1 allowed.}}
                        logger.LogDebug("Debug 2");     // Secondary

                        // The threshold was not configured, so it remains the default 1
                        logger.LogError("Error 1");     // Noncompliant {{Reduce the number of Error logging calls within this code block from 2 to the 1 allowed.}}
                        logger.LogError("Error 2");     // Secondary
                    }
                }
                """).Verify();

    [TestMethod]
    public void TooManyLoggingCalls_MisconfiguredThresholds_CS() =>
        misconfiguredBuilder.AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .AddSnippet("""
                using Microsoft.Extensions.Logging;

                public class Program
                {
                    public void Method(ILogger logger)
                    {
                        // The threshold was set to 0
                        logger.LogDebug("Debug 1");     // Noncompliant {{Reduce the number of Debug logging calls within this code block from 1 to the 0 allowed.}}

                        // The threshold was misconfigured to -1, so it's using 0 as threshold
                        logger.LogError("Error 1");     // Noncompliant {{Reduce the number of Error logging calls within this code block from 1 to the 0 allowed.}}
                    }
                }
                """).Verify();
}
