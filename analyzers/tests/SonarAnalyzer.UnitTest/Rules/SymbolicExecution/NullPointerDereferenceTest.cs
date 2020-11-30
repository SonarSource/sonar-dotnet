/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class NullPointerDereferenceTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_ValidatedNotNull() =>
            Verifier.VerifyCSharpAnalyzer(@"
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
", GetAnalyzer());

        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference() =>
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereference.cs", GetAnalyzer());

        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_CSharp6() =>
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereference.CSharp6.cs",
                GetAnalyzer(),
                ParseOptionsHelper.FromCSharp6);

        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_CSharp7() =>
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereference.CSharp7.cs",
                GetAnalyzer(),
                ParseOptionsHelper.FromCSharp7);

        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_CSharp8() =>
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereference.CSharp8.cs",
                GetAnalyzer(),
#if NETFRAMEWORK
                additionalReferences: NuGetMetadataReference.NETStandardV2_1_0,
#endif
                options: ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\NullPointerDereference.CSharp9.cs", GetAnalyzer());
#endif

        private static SonarDiagnosticAnalyzer GetAnalyzer() =>
            new SymbolicExecutionRunner(new NullPointerDereference());
    }
}
