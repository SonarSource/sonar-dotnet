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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LoggersShouldBeNamedForEnclosingTypeTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<LoggersShouldBeNamedForEnclosingType>();

    [TestMethod]
    public void LoggersShouldBeNamedForEnclosingType_CS() =>
        Builder
            .AddPaths("LoggersShouldBeNamedForEnclosingType.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(Constants.NuGetLatestVersion))
            .Verify();

    [TestMethod]
    public void LoggersShouldBeNamedForEnclosingType_TopLevelStatements_CS() =>
        Builder
            .AddSnippet("""
                using Microsoft.Extensions.Logging;

                ILoggerFactory factory = null;

                factory.CreateLogger<int>();                       // Compliant
                factory.CreateLogger(typeof(int).Name);            // Compliant
                factory.CreateLogger(nameof(RandomType));          // Compliant

                class RandomType { }
                """)
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithTopLevelStatements()
            .VerifyNoIssues();

    [TestMethod]
    public void LoggersShouldBeNamedForEnclosingType_NLog_CS() =>
        Builder
            .AddSnippet("""
                using NLog;
                using static NLog.LogManager;

                class EnclosingType
                {
                    void Method(LogFactory factory)
                    {
                        LogManager.GetLogger(nameof(EnclosingType));            // Compliant
                        LogManager.GetLogger(typeof(EnclosingType).Name);       // Compliant

                        LogManager.GetLogger(nameof(AnotherType));              // Noncompliant {{Update this logger to use its enclosing type.}}
                        //                          ^^^^^^^^^^^
                        LogManager.GetLogger(typeof(AnotherType).Name);         // Noncompliant
                        //                          ^^^^^^^^^^^

                        factory.GetLogger(nameof(EnclosingType));               // Compliant
                        factory.GetLogger(typeof(EnclosingType).Name);          // Compliant

                        factory.GetLogger(nameof(AnotherType));                 // Noncompliant {{Update this logger to use its enclosing type.}}
                        //                       ^^^^^^^^^^^
                        factory.GetLogger(typeof(AnotherType).Name);            // Noncompliant
                        //                       ^^^^^^^^^^^

                        GetLogger(typeof(AnotherType).Name);            // Noncompliant
                        //               ^^^^^^^^^^^
                    }
                }

                class AnotherType : EnclosingType { }
                """)
            .AddReferences(NuGetMetadataReference.NLog())
            .Verify();

    [TestMethod]
    public void LoggersShouldBeNamedForEnclosingType_Log4net_CS() =>
        Builder
            .AddSnippet("""
                using System.Reflection;
                using log4net;

                public class EnclosingType
                {
                    void Method(Assembly assembly)
                    {
                        LogManager.GetLogger(nameof(EnclosingType));                    // Compliant
                        LogManager.GetLogger(typeof(EnclosingType).Name);               // Compliant

                        LogManager.GetLogger(nameof(AnotherType));                      // Noncompliant {{Update this logger to use its enclosing type.}}
                        //                          ^^^^^^^^^^^
                        LogManager.GetLogger(typeof(AnotherType).Name);                 // Noncompliant
                        //                          ^^^^^^^^^^^

                        // These methods are not tracked
                        LogManager.GetLogger(nameof(AnotherType), nameof(AnotherType)); // Compliant
                        LogManager.GetLogger(nameof(AnotherType), typeof(AnotherType)); // Compliant
                        LogManager.GetLogger(assembly, nameof(AnotherType));            // Compliant
                        LogManager.GetLogger(assembly, typeof(AnotherType));            // Compliant
                    }
                }

                class AnotherType : EnclosingType { }
                """)
            .AddReferences(NuGetMetadataReference.Log4Net("2.0.8", "net45-full"))
            .Verify();
}
