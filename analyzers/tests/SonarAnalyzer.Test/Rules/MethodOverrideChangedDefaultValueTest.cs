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
    public class MethodOverrideChangedDefaultValueTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MethodOverrideChangedDefaultValue>();

        [TestMethod]
        public void MethodOverrideChangedDefaultValue() =>
            builder.AddPaths("MethodOverrideChangedDefaultValue.cs")
                .AddReferences(MetadataReferenceFacade.NetStandard21)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CSharp9() =>
            builder.AddPaths("MethodOverrideChangedDefaultValue.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CSharp11() =>
            builder.AddPaths("MethodOverrideChangedDefaultValue.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CSharp11_CodeFix() =>
            builder.AddPaths("MethodOverrideChangedDefaultValue.CSharp11.cs")
                .WithCodeFix<MethodOverrideChangedDefaultValueCodeFix>()
                .WithCodeFixedPaths("MethodOverrideChangedDefaultValue.CSharp11.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CodeFix_Synchronize() =>
            builder.AddPaths("MethodOverrideChangedDefaultValue.cs")
                .WithCodeFix<MethodOverrideChangedDefaultValueCodeFix>()
                .WithCodeFixedPaths("MethodOverrideChangedDefaultValue.Synchronize.Fixed.cs", "MethodOverrideChangedDefaultValue.Synchronize.Fixed.Batch.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .WithCodeFixTitle(MethodOverrideChangedDefaultValueCodeFix.TitleGeneral)
                .VerifyCodeFix();

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CodeFix_Remove() =>
            builder.AddPaths("MethodOverrideChangedDefaultValue.cs")
                .WithCodeFix<MethodOverrideChangedDefaultValueCodeFix>()
                .WithCodeFixedPaths("MethodOverrideChangedDefaultValue.Remove.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .WithCodeFixTitle(MethodOverrideChangedDefaultValueCodeFix.TitleExplicitInterface)
                .VerifyCodeFix();
    }
}
