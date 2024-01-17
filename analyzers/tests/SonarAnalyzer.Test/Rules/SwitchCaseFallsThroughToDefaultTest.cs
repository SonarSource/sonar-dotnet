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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class SwitchCaseFallsThroughToDefaultTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<SwitchCaseFallsThroughToDefault>();

        [TestMethod]
        public void SwitchCaseFallsThroughToDefault() =>
            builder.AddPaths("SwitchCaseFallsThroughToDefault.cs").Verify();

#if NET

        [TestMethod]
        public void SwitchCaseFallsThroughToDefault_CSharp9() =>
            builder.AddPaths("SwitchCaseFallsThroughToDefault.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

#endif

        [TestMethod]
        public void SwitchCaseFallsThroughToDefault_CodeFix() =>
            builder.AddPaths("SwitchCaseFallsThroughToDefault.cs")
                .WithCodeFix<SwitchCaseFallsThroughToDefaultCodeFix>()
                .WithCodeFixedPaths("SwitchCaseFallsThroughToDefault.Fixed.cs")
                .VerifyCodeFix();
    }
}
