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

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantParenthesesObjectCreationTest
    {
        [Ignore][TestMethod]
        public void RedundantParenthesesObjectCreation() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\RedundantParenthesesObjectCreation.cs", new RedundantParenthesesObjectsCreation());

#if NET
        [Ignore][TestMethod]
        public void RedundantParenthesesObjectCreation_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\RedundantParenthesesObjectCreation.CSharp9.cs", new RedundantParenthesesObjectsCreation());

        [Ignore][TestMethod]
        public void RedundantParenthesesObjectCreation_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(@"TestCases\RedundantParenthesesObjectCreation.CSharp10.cs", new RedundantParenthesesObjectsCreation());

        [Ignore][TestMethod]
        public void RedundantParenthesesObjectCreation_CSharpPreview() =>
            OldVerifier.VerifyAnalyzerCSharpPreviewLibrary(@"TestCases\RedundantParenthesesObjectCreation.CSharp.Preview.cs", new RedundantParenthesesObjectsCreation());
#endif

        [Ignore][TestMethod]
        public void RedundantParenthesesObjectCreation_CodeFix() =>
            OldVerifier.VerifyCodeFix<RedundantParenthesesCodeFix>(
                @"TestCases\RedundantParenthesesObjectCreation.cs",
                @"TestCases\RedundantParenthesesObjectCreation.Fixed.cs",
                new RedundantParenthesesObjectsCreation());
    }
}
