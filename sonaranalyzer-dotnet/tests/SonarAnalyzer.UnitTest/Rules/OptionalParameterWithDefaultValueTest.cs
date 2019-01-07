/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class OptionalParameterWithDefaultValueTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void OptionalParameterWithDefaultValue()
        {
            Verifier.VerifyAnalyzer(@"TestCases\OptionalParameterWithDefaultValue.cs",
                new OptionalParameterWithDefaultValue());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void OptionalParameterWithDefaultValue_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\OptionalParameterWithDefaultValue.cs",
                @"TestCases\OptionalParameterWithDefaultValue.Fixed.cs",
                new OptionalParameterWithDefaultValue(),
                new OptionalParameterWithDefaultValueCodeFixProvider());
        }
    }
}
