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
public class InvalidCastToInterfaceTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.InvalidCastToInterface>();  // Syntax-based part of the rule, there also exists Sonar SE part
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InvalidCastToInterface>();  // Syntax-based part only

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void InvalidCastToInterface_CS(ProjectType projectType) =>
        builderCS.AddPaths("InvalidCastToInterface.cs")
            .AddReferences(TestHelper.ProjectTypeReference(projectType).Union(MetadataReferenceFacade.NetStandard21))
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void InvalidCastToInterface_VB() =>
        builderVB.AddPaths("InvalidCastToInterface.vb").Verify();

#if NET

    [TestMethod]
    public void InvalidCastToInterface_CSharp9() =>
        builderCS.AddPaths("InvalidCastToInterface.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void InvalidCastToInterface_CSharp10() =>
        builderCS.AddPaths("InvalidCastToInterface.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

#endif
}
