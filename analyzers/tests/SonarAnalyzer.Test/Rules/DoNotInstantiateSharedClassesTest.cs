/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DoNotInstantiateSharedClassesTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.DoNotInstantiateSharedClasses>().AddReferences(MetadataReferenceFacade.SystemComponentModelComposition);
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.DoNotInstantiateSharedClasses>().AddReferences(MetadataReferenceFacade.SystemComponentModelComposition);

    [TestMethod]
    public void DoNotInstantiateSharedClasses_CS() =>
        builderCS.AddPaths("DoNotInstantiateSharedClasses.cs").Verify();

    [TestMethod]
    public void DoNotInstantiateSharedClasses_CS_InTest() =>
        builderCS.AddPaths("DoNotInstantiateSharedClasses.cs")
            .AddTestReference()
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void DoNotInstantiateSharedClasses_VB() =>
        builderVB.AddPaths("DoNotInstantiateSharedClasses.vb").Verify();

    [TestMethod]
    public void DoNotInstantiateSharedClasses_VB_InTest() =>
        builderVB.AddPaths("DoNotInstantiateSharedClasses.vb")
            .AddTestReference()
            .VerifyNoIssuesIgnoreErrors();
}
