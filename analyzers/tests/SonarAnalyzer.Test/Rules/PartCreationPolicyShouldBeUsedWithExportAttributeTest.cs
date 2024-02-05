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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class PartCreationPolicyShouldBeUsedWithExportAttributeTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.PartCreationPolicyShouldBeUsedWithExportAttribute>()
                .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition);

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CS() =>
            builderCS.AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.cs").WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_UnresolvedSymbol_CS() =>
            builderCS.AddSnippet(@"
[UnresolvedAttribute] // Error [CS0246, CS0246]
class Bar { }")
                .Verify();

#if NET

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CSharp9() =>
            builderCS.AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CSharp12() =>
            builderCS.AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();

#endif

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_VB() =>
            new VerifierBuilder<VB.PartCreationPolicyShouldBeUsedWithExportAttribute>().AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.vb")
                .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
                .Verify();
    }
}
