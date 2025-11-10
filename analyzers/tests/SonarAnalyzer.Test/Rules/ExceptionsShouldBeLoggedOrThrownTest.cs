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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExceptionsShouldBeLoggedOrThrownTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ExceptionsShouldBeLoggedOrThrown>();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_CS() =>
        builder
            .AddPaths("ExceptionsShouldBeLoggedOrThrown.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .Verify();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_Coalesce_CS() =>
        builder
            .AddSnippet("""
                using System;
                using Microsoft.Extensions.Logging;

                public class Program
                {
                    public void Method(ILogger logger, object x)
                    {
                        try { }
                        catch (SystemException e)
                        {
                            logger.LogError(e, "Message!");
                            x = x ?? throw e;
                        }
                    }
                }
                """)
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithOptions(LanguageOptions.FromCSharp8)
            .VerifyNoIssues();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_Log4net_CS() =>
        builder
            .AddSnippet("""
                using System;
                using log4net;
                using log4net.Util;

                public class Program
                {
                    public void Method(ILog logger, string message)
                    {
                        try { }
                        catch (AggregateException e)                                // Noncompliant
                        {
                            ILogExtensions.DebugExt(logger, "Message", e);          // Secondary
                            throw;                                                  // Secondary
                        }
                        catch (Exception e)                                         // Noncompliant
                        {
                            logger.Debug(message, e);                               // Secondary
                            throw;                                                  // Secondary
                        }
                    }
                }
                """)
            .AddReferences(NuGetMetadataReference.Log4Net("2.0.8", "net45-full"))
            .Verify();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_NLog_CS() =>
        builder
            .AddSnippet("""
                using System;
                using NLog;

                public class Program
                {
                    public void Method(ILogger logger, string message)
                    {
                        try { }
                        catch (ArgumentException e)                     // Noncompliant
                        {
                            logger.Debug(e, message);                   // Secondary
                            throw;                                      // Secondary
                        }
                        catch (Exception e)                             // Noncompliant
                        {
                            ILoggerExtensions.Warn(logger, e, null);    // Secondary
                            throw;                                      // Secondary
                        }
                    }
                }
                """)
            .AddReferences(NuGetMetadataReference.NLog())
            .Verify();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_CastleCore_CS() =>
        builder
            .AddSnippet("""
                using System;
                using Castle.Core.Logging;

                public class Program
                {
                    public void Method(ILogger logger, string message)
                    {
                        try { }
                        catch (Exception e)                 // Noncompliant
                        {
                            logger.Debug(message, e);       // Secondary
                            throw;                          // Secondary
                        }
                    }
                }
                """)
            .AddReferences(NuGetMetadataReference.CastleCore())
            .Verify();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_CommonLogging_CS() =>
        builder
            .AddSnippet("""
                using System;
                using Common.Logging;

                public class Program
                {
                    public void Method(ILog logger, string message)
                    {
                        try { }
                        catch (Exception e)                 // Noncompliant
                        {
                            logger.Debug(message, e);       // Secondary
                            throw;                          // Secondary
                        }
                    }
                }
                """)
            .AddReferences(NuGetMetadataReference.CommonLoggingCore())
            .Verify();

    [TestMethod]
    public void ExceptionsShouldBeLoggedOrThrown_Serilog_CS() =>
        builder
            .AddSnippet("""
                using System;
                using Serilog;

                public class Program
                {
                    public void Method(ILogger logger, string message)
                    {
                        try { }
                        catch (AggregateException e)        // Noncompliant
                        {
                            Log.Debug(e, message);          // Secondary
                            throw;                          // Secondary
                        }
                        catch (Exception e)                 // Noncompliant
                        {
                            logger.Debug(e, message);       // Secondary
                            throw;                          // Secondary
                        }
                    }
                }
                """)
            .AddReferences(NuGetMetadataReference.Serilog())
            .Verify();
}
