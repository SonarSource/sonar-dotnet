/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class NullPointerDereferenceTest
    {
        private static readonly DiagnosticDescriptor[] OnlyDiagnostics = new[] { NullPointerDereference.S2259 };

        [TestMethod]
        public void NullPointerDereference_ValidatedNotNull() =>
            OldVerifier.VerifyCSharpAnalyzer(@"
using System;

public sealed class ValidatedNotNullAttribute : Attribute { }

public static class Guard
{
    public static void NotNull<T>([ValidatedNotNullAttribute] this T value, string name) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(name);
    }
}

public static class Utils
{
    public static string ToUpper(string value)
    {
        Guard.NotNull(value, nameof(value));
        if (value != null)
        {
            return value.ToUpper(); // Compliant
        }
        return value.ToUpper(); // Compliant
    }
}
", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void NullPointerDereference_CS() =>
            OldVerifier.VerifyNonConcurrentAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\NullPointerDereference.cs",
                new SymbolicExecutionRunner(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void NullPointerDereference_DoesNotRaiseIssuesForTestProject() =>
            new VerifierBuilder<SymbolicExecutionRunner>()
                .AddTestReference()
                .AddPaths(@"SymbolicExecution\Sonar\NullPointerDereference.cs")
                .WithOnlyDiagnostics(OnlyDiagnostics)
                .WithConcurrentAnalysis(false)
                .VerifyNoIssueReported();

        [TestMethod]
        public void NullPointerDereference_CSharp6() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\NullPointerDereference.CSharp6.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp6,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void NullPointerDereference_CSharp7() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\NullPointerDereference.CSharp7.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp7,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void NullPointerDereference_CSharp8() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\NullPointerDereference.CSharp8.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.NETStandard21,
                onlyDiagnostics: OnlyDiagnostics);

#if NET
        [TestMethod]
        public void NullPointerDereference_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(
                @"TestCases\SymbolicExecution\Sonar\NullPointerDereference.CSharp9.cs",
                new SymbolicExecutionRunner(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void NullPointerDereference_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(
                @"TestCases\SymbolicExecution\Sonar\NullPointerDereference.CSharp10.cs",
                new SymbolicExecutionRunner(),
                onlyDiagnostics: OnlyDiagnostics);
#endif
    }
}
