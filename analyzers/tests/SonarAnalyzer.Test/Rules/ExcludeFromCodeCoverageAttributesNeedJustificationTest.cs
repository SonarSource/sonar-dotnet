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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExcludeFromCodeCoverageAttributesNeedJustificationTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ExcludeFromCodeCoverageAttributesNeedJustification>();

#if NET

    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ExcludeFromCodeCoverageAttributesNeedJustification>();

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_OnAssembly_CS() =>
        builderCS.AddSnippet("[assembly:System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // Noncompliant").Verify();

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_OnAssembly_VB() =>
        builderVB.AddSnippet("<Assembly:System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage> ' Noncompliant").Verify();

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_CS() =>
       builderCS.AddPaths("ExcludeFromCodeCoverageAttributesNeedJustification.cs").Verify();

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_CSharp9() =>
        builderCS.AddPaths("ExcludeFromCodeCoverageAttributesNeedJustification.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_CSharp10() =>
        builderCS.AddPaths("ExcludeFromCodeCoverageAttributesNeedJustification.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_VB() =>
        builderVB.AddPaths("ExcludeFromCodeCoverageAttributesNeedJustification.vb").Verify();

#else

    [TestMethod]
    public void ExcludeFromCodeCoverageAttributesNeedJustification_IgnoredForNet48() =>
        builderCS.AddPaths("ExcludeFromCodeCoverageAttributesNeedJustification.Net48.cs").Verify();

#endif

}
