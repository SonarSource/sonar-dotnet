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
public class PropertiesAccessCorrectFieldTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.PropertiesAccessCorrectField>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.PropertiesAccessCorrectField>();

    private static IEnumerable<MetadataReference> AdditionalReferences =>
        NuGetMetadataReference.MvvmLightLibs("5.4.1.1")
                              .Concat(MetadataReferenceFacade.WindowsBase)
                              .Concat(MetadataReferenceFacade.PresentationFramework);

    [TestMethod]
    public void PropertiesAccessCorrectField_CS() =>
        builderCS.AddPaths("PropertiesAccessCorrectField.cs").AddReferences(AdditionalReferences).Verify();

    [TestMethod]
    public void PropertiesAccessCorrectField_CSharp8() =>
        builderCS.AddPaths("PropertiesAccessCorrectField.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

    [TestMethod]
    public void PropertiesAccessCorrectField_CSharp9() =>
        builderCS.AddPaths("PropertiesAccessCorrectField.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

    [TestMethod]
    public void PropertiesAccessCorrectField_CSharp12() =>
        builderCS.AddPaths("PropertiesAccessCorrectField.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).VerifyNoIssues();

#else

    [TestMethod]
    public void PropertiesAccessCorrectField_CS_NetFramework() =>
        builderCS.AddPaths("PropertiesAccessCorrectField.NetFramework.cs").AddReferences(AdditionalReferences).VerifyNoIssues();

    [TestMethod]
    public void PropertiesAccessCorrectField_VB_NetFramework() =>
        builderVB.AddPaths("PropertiesAccessCorrectField.NetFramework.vb").AddReferences(AdditionalReferences).VerifyNoIssues();

#endif

    [TestMethod]
    public void PropertiesAccessCorrectField_VB() =>
        builderVB.AddPaths("PropertiesAccessCorrectField.vb").AddReferences(AdditionalReferences).WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();
}
