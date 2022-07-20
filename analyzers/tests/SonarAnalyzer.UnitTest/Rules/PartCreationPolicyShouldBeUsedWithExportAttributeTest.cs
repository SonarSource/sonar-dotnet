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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class PartCreationPolicyShouldBeUsedWithExportAttributeTest
    {
        [Ignore][TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\PartCreationPolicyShouldBeUsedWithExportAttribute.cs",
                                    new CS.PartCreationPolicyShouldBeUsedWithExportAttribute(),
                                    MetadataReferenceFacade.SystemComponentModelComposition);

        [Ignore][TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_UnresolvedSymbol_CS() =>
            OldVerifier.VerifyCSharpAnalyzer(@"
[UnresolvedAttribute] // Error [CS0246]
class Bar { }",
                                          new CS.PartCreationPolicyShouldBeUsedWithExportAttribute(),
                                          CompilationErrorBehavior.Ignore,
                                          MetadataReferenceFacade.SystemComponentModelComposition);
#if NET
        [Ignore][TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\PartCreationPolicyShouldBeUsedWithExportAttribute.CSharp9.cs",
                                                      new CS.PartCreationPolicyShouldBeUsedWithExportAttribute(),
                                                      MetadataReferenceFacade.SystemComponentModelComposition);
#endif

        [Ignore][TestMethod]
        public void PartCreationPolicyShouldBeUsedWithExportAttribute_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\PartCreationPolicyShouldBeUsedWithExportAttribute.vb",
                                    new VB.PartCreationPolicyShouldBeUsedWithExportAttribute(),
                                    MetadataReferenceFacade.SystemComponentModelComposition);
    }
}
