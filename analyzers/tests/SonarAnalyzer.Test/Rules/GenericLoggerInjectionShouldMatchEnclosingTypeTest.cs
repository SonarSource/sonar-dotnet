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
public class GenericLoggerInjectionShouldMatchEnclosingTypeTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<GenericLoggerInjectionShouldMatchEnclosingType>()
        .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions());

    [TestMethod]
    public void GenericLoggerInjectionShouldMatchEnclosingTypeTest_CS() =>
        Builder.AddSnippet("""
            using System;
            using Microsoft.Extensions.Logging;

            public class Correct : Base
            {
                Correct(ILogger<Correct> logger) { }                             // Compliant
                Correct(ILogger<Correct> logger, ILogger<Correct> logger2) { }   // Compliant

                Correct(Wrapper<Correct> logger) { }                             // Compliant
                Correct(Wrapper<Wrong> logger) { }                               // Compliant
                Correct(Wrapper<Correct> logger, Wrapper<Wrong> logger2) { }     // Compliant

                Correct(ILogger<string> logger) { }                              // Noncompliant {{Update this logger to use its enclosing type.}}
                //              ^^^^^^
                Correct(ILogger<Wrong> logger) { }                               // Noncompliant
                //              ^^^^^
                Correct(ILogger<Base> logger) { }                                // Noncompliant
                //              ^^^^

                Correct(ILogger<Wrapper<Correct>> logger) { }                    // Noncompliant
                //              ^^^^^^^^^^^^^^^^
                Correct(ILogger<ILogger<Correct>> logger) { }                    // Noncompliant
                //              ^^^^^^^^^^^^^^^^

                Correct(ILogger<string> logger, ILogger<Wrong> logger2) { }
                //              ^^^^^^ {{Update this logger to use its enclosing type.}}
                //                                      ^^^^^ @-1 {{Update this logger to use its enclosing type.}}

                Correct(ILogger<string> logger, ILogger<Correct> logger2, ILogger<ILogger<Correct>> logger3) { }
                //              ^^^^^^ {{Update this logger to use its enclosing type.}}
                //                                                                ^^^^^^^^^^^^^^^^ @-1 {{Update this logger to use its enclosing type.}}

                Correct(Logger<Correct> logger, Logger<Wrong> logger2) { }
                //                                     ^^^^^ {{Update this logger to use its enclosing type.}}

                Correct(Logger logger) {} // Compliant FP , Logger is not a generic type
            }

            public class Base { }
            public class Wrong { }
            public class Wrapper<T> { }

            public class Logger : Logger<int> { }

            public class Logger<T> : ILogger<T>
            {
                public IDisposable BeginScope<TState>(TState x) => null;
                public bool IsEnabled(LogLevel x) => false;
                void ILogger.Log<TState>(LogLevel x1, EventId x2, TState x3, Exception x4, Func<TState, Exception, string> x5) { }
            }
            """)
        .Verify();
}
