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

#if NET
using CS = SonarAnalyzer.Rules.CSharp;
#else
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;
#endif

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ConstructorArgumentValueShouldExistTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ConstructorArgumentValueShouldExist>();

#if NET

        [TestMethod]
        public void ConstructorArgumentValueShouldExist_CS_Latest() =>
            builderCS.AddPaths("ConstructorArgumentValueShouldExist.Latest.cs", "ConstructorArgumentValueShouldExist.Latest.Partial.cs")
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .Verify();

#else

        [TestMethod]
        public void ConstructorArgumentValueShouldExist_CS() =>
            builderCS.AddPaths("ConstructorArgumentValueShouldExist.cs")
                .AddReferences(MetadataReferenceFacade.SystemXaml)
                .Verify();

        [TestMethod]
        public void ConstructorArgumentValueShouldExist_VB() =>
            new VerifierBuilder<VB.ConstructorArgumentValueShouldExist>().AddPaths("ConstructorArgumentValueShouldExist.vb")
                .WithOptions(ParseOptionsHelper.FromVisualBasic14)
                .AddReferences(MetadataReferenceFacade.SystemXaml)
                .Verify();

#endif

    }
}
