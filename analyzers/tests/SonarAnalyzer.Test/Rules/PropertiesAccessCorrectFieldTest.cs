/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

#if NET
    [TestMethod]
    public void PropertiesAccessCorrectField_CS_Latest() =>
        builderCS
            .AddPaths("PropertiesAccessCorrectField.Latest.cs", "PropertiesAccessCorrectField.Partial.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
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
        builderVB.AddPaths("PropertiesAccessCorrectField.vb").AddReferences(AdditionalReferences).WithOptions(LanguageOptions.FromVisualBasic14).Verify();
}
