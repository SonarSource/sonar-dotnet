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

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ForeachLoopExplicitConversionTest
    {
        [TestMethod]
        public void ForeachLoopExplicitConversion() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\ForeachLoopExplicitConversion.cs", new ForeachLoopExplicitConversion());

        [TestMethod]
        public void ForeachLoopExplicitConversion_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(@"TestCases\ForeachLoopExplicitConversion.CSharp10.cs", new ForeachLoopExplicitConversion());

        [TestMethod]
        public void ForeachLoopExplicitConversion_CodeFix() =>
            OldVerifier.VerifyCodeFix<ForeachLoopExplicitConversionCodeFix>(
                @"TestCases\ForeachLoopExplicitConversion.cs",
                @"TestCases\ForeachLoopExplicitConversion.Fixed.cs",
                new ForeachLoopExplicitConversion());

#if NET
        [TestMethod]
        public void ForeachLoopExplicitConversion_CSharp10_CodeFix() =>
            OldVerifier.VerifyCodeFix<ForeachLoopExplicitConversionCodeFix>(
                @"TestCases\ForeachLoopExplicitConversion.CSharp10.cs",
                @"TestCases\ForeachLoopExplicitConversion.CSharp10.Fixed.cs",
                new ForeachLoopExplicitConversion(),
                ParseOptionsHelper.FromCSharp10,
                OutputKind.DynamicallyLinkedLibrary);
#endif

    }
}
