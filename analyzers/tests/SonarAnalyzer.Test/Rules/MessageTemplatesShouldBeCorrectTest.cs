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
    private static readonly IEnumerable<MetadataReference> LoggingReferences =
        NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions()
        .Concat(NuGetMetadataReference.NLog(Constants.NuGetLatestVersion))
        .Concat(NuGetMetadataReference.Serilog(Constants.NuGetLatestVersion));

    private static readonly VerifierBuilder Builder = new VerifierBuilder<MessageTemplatesShouldBeCorrect>()
        .AddReferences(LoggingReferences);

    [TestMethod]
    public void MessageTemplatesShouldBeCorrect_CS() =>
        Builder.AddPaths("MessageTemplatesShouldBeCorrect.cs").Verify();

    [DataTestMethod]
    [DataRow("LogCritical")]
    [DataRow("LogDebug")]
    [DataRow("LogError")]
    [DataRow("LogInformation")]
    [DataRow("LogTrace")]
    [DataRow("LogWarning")]
    public void MessageTemplatesShouldBeCorrect_MicrosoftExtensionsLogging_CS(string methodName) =>
        Builder.AddSnippet($$$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class Program
            {
                public void Method(ILogger logger, string user, int cnt)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    logger.{{{methodName}}}("{", user);                                 // Noncompliant {{Log message template should be syntactically correct.}}
                    logger.{{{methodName}}}("Login failed for {}", user);               // Noncompliant {{Log message template should not contain empty placeholder.}}
                    logger.{{{methodName}}}("Login failed for {%User}", user);          // Noncompliant {{Log message template placeholder '%User' should only contain chars, numbers, and underscore.}}
                    logger.{{{methodName}}}("Retry attempt {Cnt,r}", cnt);              // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of 'r'.}}
                    logger.{{{methodName}}}("Retry attempt {Cnt:}", cnt);               // Noncompliant {{Log message template placeholder 'Cnt' should not have empty format.}}
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
        Builder.AddSnippet($$$"""
            using System;
            using Serilog;
            using Serilog.Events;

            public class Program
            {
                public void Method(ILogger logger, string user, int cnt)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    logger.{{{methodName}}}("{", user);                                 // Noncompliant {{Log message template should be syntactically correct.}}
                    logger.{{{methodName}}}("Login failed for {}", user);               // Noncompliant {{Log message template should not contain empty placeholder.}}
                    logger.{{{methodName}}}("Login failed for {%User}", user);          // Noncompliant {{Log message template placeholder '%User' should only contain chars, numbers, and underscore.}}
                    logger.{{{methodName}}}("Retry attempt {Cnt,r}", cnt);              // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of 'r'.}}
                    logger.{{{methodName}}}("Retry attempt {Cnt:}", cnt);               // Noncompliant {{Log message template placeholder 'Cnt' should not have empty format.}}
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
        Builder.AddSnippet($$$"""
            using System;
            using NLog;

            public class Program
            {
                public void Method(ILogger iLogger, Logger logger, MyLogger myLogger, string user, int cnt)
                {
                    Console.WriteLine("Login failed for {User", user);                  // Compliant
                    logger.{{{methodName}}}("Login failed for {User}", user);           // Compliant

                    iLogger.{{{methodName}}}("{", user);                                 // Noncompliant {{Log message template should be syntactically correct.}}
                    iLogger.{{{methodName}}}("Login failed for {}", user);               // Noncompliant {{Log message template should not contain empty placeholder.}}
                    iLogger.{{{methodName}}}("Login failed for {%User}", user);          // Noncompliant {{Log message template placeholder '%User' should only contain chars, numbers, and underscore.}}
                    iLogger.{{{methodName}}}("Retry attempt {Cnt,r}", cnt);              // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of 'r'.}}
                    iLogger.{{{methodName}}}("Retry attempt {Cnt:}", cnt);               // Noncompliant {{Log message template placeholder 'Cnt' should not have empty format.}}

                    logger.{{{methodName}}}("{", user);                                 // Noncompliant {{Log message template should be syntactically correct.}}
                    logger.{{{methodName}}}("Login failed for {}", user);               // Noncompliant {{Log message template should not contain empty placeholder.}}
                    logger.{{{methodName}}}("Login failed for {%User}", user);          // Noncompliant {{Log message template placeholder '%User' should only contain chars, numbers, and underscore.}}
                    logger.{{{methodName}}}("Retry attempt {Cnt,r}", cnt);              // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of 'r'.}}
                    logger.{{{methodName}}}("Retry attempt {Cnt:}", cnt);               // Noncompliant {{Log message template placeholder 'Cnt' should not have empty format.}}

                    myLogger.{{{methodName}}}("{", user);                                 // Noncompliant {{Log message template should be syntactically correct.}}
                    myLogger.{{{methodName}}}("Login failed for {}", user);               // Noncompliant {{Log message template should not contain empty placeholder.}}
                    myLogger.{{{methodName}}}("Login failed for {%User}", user);          // Noncompliant {{Log message template placeholder '%User' should only contain chars, numbers, and underscore.}}
                    myLogger.{{{methodName}}}("Retry attempt {Cnt,r}", cnt);              // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of 'r'.}}
                    myLogger.{{{methodName}}}("Retry attempt {Cnt:}", cnt);               // Noncompliant {{Log message template placeholder 'Cnt' should not have empty format.}}
                }
            }
            public class MyLogger : Logger { }
            """).Verify();
}
