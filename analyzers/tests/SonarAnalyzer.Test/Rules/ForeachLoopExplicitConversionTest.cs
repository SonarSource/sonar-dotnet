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
    public class ForeachLoopExplicitConversionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ForeachLoopExplicitConversion>();

        [TestMethod]
        public void ForeachLoopExplicitConversion() =>
            builder.AddPaths("ForeachLoopExplicitConversion.cs").Verify();

        [TestMethod]
        public void ForeachLoopExplicitConversion_CodeFix() =>
            builder.WithCodeFix<ForeachLoopExplicitConversionCodeFix>()
                .AddPaths("ForeachLoopExplicitConversion.cs")
                .WithCodeFixedPaths("ForeachLoopExplicitConversion.Fixed.cs")
                .VerifyCodeFix();
#if NET

        [TestMethod]
        public void ForeachLoopExplicitConversion_CSharp10() =>
            builder.AddPaths("ForeachLoopExplicitConversion.CSharp10.cs")
                .WithAutogenerateConcurrentFiles(false)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void ForeachLoopExplicitConversion_CSharp10_CodeFix() =>
            builder.WithCodeFix<ForeachLoopExplicitConversionCodeFix>()
                .AddPaths("ForeachLoopExplicitConversion.CSharp10.cs")
                .WithCodeFixedPaths("ForeachLoopExplicitConversion.CSharp10.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                .VerifyCodeFix();

#endif

    }
}
