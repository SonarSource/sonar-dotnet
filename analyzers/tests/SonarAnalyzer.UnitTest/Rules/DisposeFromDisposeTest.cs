/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DisposeFromDisposeTest
    {
        [TestMethod]
        public void DisposeFromDispose_CSharp7_2() =>
            // Readonly structs have been introduced in C# 7.2.
            // In C# 8, readonly structs can be disposed of, and the behavior is different.
            Verifier.VerifyAnalyzer(@"TestCases\DisposeFromDispose.CSharp7_2.cs", new DisposeFromDispose(),
                ImmutableArray.Create(new CSharpParseOptions(LanguageVersion.CSharp7_2)));

        [TestMethod]
        public void DisposeFromDispose_CSharp8() =>
            Verifier.VerifyAnalyzer(@"TestCases\DisposeFromDispose.CSharp8.cs", new DisposeFromDispose(), ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        public void DisposeFromDispose_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(new[] { @"TestCases\DisposeFromDispose.CSharp9.Part1.cs", @"TestCases\DisposeFromDispose.CSharp9.Part2.cs", }, new DisposeFromDispose());
#endif
        [TestMethod]
        public void DisposeFromDispose_CSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Console(@"TestCases\DisposeFromDispose.CSharp10.cs", new DisposeFromDispose());

    }
}
