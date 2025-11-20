/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

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

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CSharp9() =>
            builderCS.AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.CSharp9.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .Verify();

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CSharp12() =>
            builderCS.AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.CSharp12.cs")
                .WithOptions(LanguageOptions.FromCSharp12)
                .Verify();

        [TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_VB() =>
            new VerifierBuilder<VB.PartCreationPolicyShouldBeUsedWithExportAttribute>().AddPaths("PartCreationPolicyShouldBeUsedWithExportAttribute.vb")
                .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
                .Verify();
    }
}
