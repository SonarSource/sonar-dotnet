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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ClassAndMethodNameTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassAndMethodName>();

    [TestMethod]
    public void ClassAndMethodName_CS() =>
        builderCS.AddPaths("ClassAndMethodName.cs", "ClassAndMethodName.Partial.cs")
            .AddReferences(MetadataReferenceFacade.NetStandard21)
            .WithConcurrentAnalysis(false)
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void ClassAndMethodName_InTestProject_CS() =>
        builderCS.AddPaths("ClassAndMethodName.Tests.cs").AddTestReference().WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

    [TestMethod]
    public void ClassAndMethodName_TopLevelStatement_CS() =>
        builderCS.AddPaths("ClassAndMethodName.TopLevelStatement.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void ClassAndMethodName_TopLevelStatement_InTestProject_CS() =>
        builderCS.AddPaths("ClassAndMethodName.TopLevelStatement.Test.cs").AddTestReference().WithTopLevelStatements().Verify();

    [TestMethod]
    public void ClassAndMethodName_MethodName_CS_Latest() =>
        builderCS.AddPaths("ClassAndMethodName.MethodName.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void ClassAndMethodName_MethodName_InTestProject_CS_Latest() =>
        builderCS.AddPaths("ClassAndMethodName.MethodName.Latest.cs").AddTestReference().WithOptions(LanguageOptions.CSharpLatest).Verify();

#endif

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void ClassAndMethodName_VB(ProjectType projectType) =>
        new VerifierBuilder<VB.ClassName>().AddPaths("ClassAndMethodName.vb").AddReferences(TestCompiler.ProjectTypeReference(projectType)).Verify();

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void ClassAndMethodName_MethodName(ProjectType projectType) =>
        builderCS.AddPaths("ClassAndMethodName.MethodName.cs", "ClassAndMethodName.MethodName.Partial.cs")
            .AddReferences(TestCompiler.ProjectTypeReference(projectType))
            .WithOptions(LanguageOptions.FromCSharp8).Verify();

    [DataTestMethod]
    [DataRow("foo", "foo")]
    [DataRow("Foo", "Foo")]
    [DataRow("FFF", "FFF")]
    [DataRow("FfF", "Ff", "F")]
    [DataRow("Ff9F", "Ff", "9", "F")]
    [DataRow("你好", "你", "好")]
    [DataRow("FFf", "F", "Ff")]
    [DataRow("")]
    [DataRow("FF9d", "FF", "9", "d")]
    [DataRow("y2x5__w7", "y", "2", "x", "5", "_", "_", "w", "7")]
    [DataRow("3%c#account", "3", "%", "c", "#", "account")]
    public void TestSplitToParts(string name, params string[] expectedParts) =>
        CS.ClassAndMethodName.SplitToParts(name).Should().BeEquivalentTo(expectedParts);
}
