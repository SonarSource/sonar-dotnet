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
public class LoggerMembersNamesShouldComplyTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<LoggerMembersNamesShouldComply>();

    [DataTestMethod]
    [DataRow("log")]
    [DataRow("_log")]
    [DataRow("Log")]
    [DataRow("_Log")]
    [DataRow("logger")]
    [DataRow("_logger")]
    [DataRow("Logger")]
    [DataRow("_Logger")]
    [DataRow("instance")]
    [DataRow("Instance")]
    public void LoggerMembersNamesShouldComply_Compliant_CS(string name) =>
        Builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class One
            {
                ILogger {{name}};                       // Compliant
            }
            public class Two
            {
                ILogger<string> {{name}} { get; set; }  // Compliant
            }
            """)
           .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
           .VerifyNoIssues();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_MicrosoftExtensionsLogging_CS() =>
        Builder.AddSnippet("""
            using System;
            using Microsoft.Extensions.Logging;

            public class Program
            {
                string mylogger;                        // Compliant

                ILogger _log2;                          // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                ILogger<Program> mylog { get; set; }    // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //               ^^^^^

                ILogger myLogger, _Log2, _logger;
                //      ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //                ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
            }
            """)
       .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
       .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_Serilog_CS() =>
        Builder.AddSnippet("""
            using Serilog;

            public class Program
            {
                string mylogger;                        // Compliant
                ILogger _Logger;                        // Compliant
                ILogger log;                            // Compliant

                ILogger _log2;                          // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                ILogger mylog { get; set; }             // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //      ^^^^^

                ILogger myLogger, _Log2, _logger;
                //      ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //                ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
            }
            """)
        .AddReferences(NuGetMetadataReference.Serilog())
        .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_NLog_CS() =>
        Builder.AddSnippet("""
            using NLog;

            public class Program
            {
                string mylogger;                        // Compliant
                ILogger _Logger;                        // Compliant
                ILoggerBase log;                        // Compliant

                MyLogger _log2;                         // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                ILoggerBase my_logger;                  // Noncompliant {{Rename this field 'my_logger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                Logger mylog { get; set; }              // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //     ^^^^^

                ILogger myLogger, _Log2, _logger;
                //      ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //                ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
            }
            public class MyLogger : Logger { }
            """)
        .AddReferences(NuGetMetadataReference.NLog())
        .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_log4net_CS() =>
        Builder.AddSnippet("""
            using log4net;
            using log4net.Core;
            using log4net.Repository.Hierarchy;

            public class Program
            {
                string mylogger;                        // Compliant
                ILog log;                               // Compliant
                ILogger _Logger;                        // Compliant
                Logger _log;                            // Compliant

                ILogger _log2;                         // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                ILog my_logger;                  // Noncompliant {{Rename this field 'my_logger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                Logger mylog { get; set; }              // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //     ^^^^^

                MyLogger myLogger, _Log2, _logger;
                //       ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //                 ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
            }
            public class MyLogger : Logger
            {
                public MyLogger(string name) : base(name) { }
            }
            """)
        .AddReferences(NuGetMetadataReference.Log4Net(TestConstants.NuGetLatestVersion, "netstandard2.0"))
        .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_CastleCore_CS() =>
        Builder.AddSnippet("""
            using Castle.Core.Logging;

            public class Program
            {
                string mylogger;                        // Compliant
                ILogger log;                            // Compliant

                ILogger _log2;                          // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                ILogger mylog { get; set; }             // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //      ^^^^^

                ILogger myLogger, _Log2, _logger;
                //      ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
                //                ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
            }
            """)
        .AddReferences(NuGetMetadataReference.CastleCore())
        .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_Parameterized_CS() =>
        new VerifierBuilder()
            .AddAnalyzer(() => new LoggerMembersNamesShouldComply { Format = "^chocolate$" })
            .AddSnippet("""
                    using System;
                    using Microsoft.Extensions.Logging;

                    public class Program
                    {
                        ILogger chocolate;                      // Compliant
                        ILogger running;                        // Noncompliant {{Rename this field 'running' to match the regular expression '^chocolate$'.}}
                    }
                    """)
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .Verify();
}
