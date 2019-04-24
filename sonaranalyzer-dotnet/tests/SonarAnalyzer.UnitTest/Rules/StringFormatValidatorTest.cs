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

using System.Collections.Generic;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class StringFormatValidatorTest
    {
        private static readonly IEnumerable<ParseOptions> workingOptions =
            new[]
            {
                new CSharpParseOptions(LanguageVersion.CSharp5),
                new CSharpParseOptions(LanguageVersion.CSharp6),
                new CSharpParseOptions(LanguageVersion.CSharp7),
                new CSharpParseOptions(LanguageVersion.CSharp7_1),
                new CSharpParseOptions(LanguageVersion.CSharp7_2)
            };

        [TestMethod]
        [TestCategory("Rule")]
        public void StringFormatRuntimeExceptionFreeValidator()
        {
            Verifier.VerifyAnalyzer(@"TestCases\StringFormatRuntimeExceptionFreeValidator.cs",
                new StringFormatValidator(),
                workingOptions);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void StringFormatTypoFreeValidator()
        {
            Verifier.VerifyAnalyzer(@"TestCases\StringFormatTypoFreeValidator.cs",
                new StringFormatValidator(),
                workingOptions);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void StringFormatEdgeCasesValidator()
        {
            Verifier.VerifyAnalyzer(@"TestCases\StringFormatEdgeCasesValidator.cs",
                new StringFormatValidator(),
                workingOptions);
        }
    }
}
