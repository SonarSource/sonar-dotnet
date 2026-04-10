/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ArrayCreationLongSyntaxTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ArrayCreationLongSyntax>();

        [TestMethod]
        public void ArrayCreationLongSyntax() =>
            builder.AddPaths("ArrayCreationLongSyntax.vb").Verify();

        [TestMethod]
        public void ArrayCreationLongSyntax_CodeFix() =>
            builder.AddPaths("ArrayCreationLongSyntax.vb").WithCodeFix<ArrayCreationLongSyntaxCodeFix>().WithCodeFixedPaths("ArrayCreationLongSyntax.Fixed.vb").VerifyCodeFix();
    }
}
