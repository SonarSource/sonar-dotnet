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

using System.IO;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Test.Rules.Utilities;

[TestClass]
public class TestMethodDeclarationsAnalyzerTest
{
    private const string BasePath = @"Utilities\MethodDeclarationsAnalyzer\";

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void VerifyMethodDeclarations_ShouldGenerateMetrics_AvoidGeneratedFiles() =>
        CreateCSharpBuilder(isTestProject: true, "TestMethodDeclarations.Generated.g.cs")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => x.Should().BeEmpty());

    [TestMethod]
    public void VerifyMethodDeclarations_TestCode_CSharp() =>
        CreateCSharpBuilder(isTestProject: true, "TestMethodDeclarations.cs", "TestMethodDeclarations.Partial.cs")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x =>
            {
                x.Should().HaveCount(2);
                var firstFileDeclarations = x.OrderBy(declaration => declaration.FilePath).First();
                firstFileDeclarations.AssemblyName.Should().Be("project0");
                firstFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "TestMethodDeclarations.cs"));
                firstFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Address", MethodName = "GetZipCode" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.BaseClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.BaseClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Company", MethodName = "GetCompanyName" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.DerivedClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.DerivedClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Employee", MethodName = "GetEmployeeName" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.FileClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.GenericClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.LocalFunctions", MethodName = "Main" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.MultipleLevelInheritance", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.MultipleLevelInheritance", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.MultipleLevelInheritance", MethodName = "MultipleLevelInheritanceMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.IInterfaceWithTestDeclarations", MethodName = "GetZipCode" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.MultipleMethods", MethodName = "Method1" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.MultipleMethods", MethodName = "Method2" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.NoModifiers", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Overloads", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "InFirstFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "PartialMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Person", MethodName = "GetFullName" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "InternalMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "NoAccessModifierMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "PrivateMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "PrivateProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "ProtectedInternalMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "ProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility", MethodName = "PublicMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility.InternalClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.Visibility.PrivateClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.WithGenericMethod", MethodName = "Method" },
                ]);
                var secondFileDeclarations = x.OrderBy(declaration => declaration.FilePath).Skip(1).First();
                secondFileDeclarations.AssemblyName.Should().Be("project0");
                secondFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "TestMethodDeclarations.Partial.cs"));
                secondFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "PartialMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "InSecondFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.CSharp.PartialClass", MethodName = "Method" }
                ]);
            });

    [TestMethod]
    public void VerifyMethodDeclarations_TestCode_GlobalNamespace_CSharp() =>
        CreateCSharpBuilder(isTestProject: true, "TestMethodDeclarations.GlobalNamespace.cs")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x =>
            {
                x.Should().HaveCount(1);
                var fileDeclarations = x.OrderBy(declaration => declaration.FilePath).First();
                fileDeclarations.AssemblyName.Should().Be("project0");
                fileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "TestMethodDeclarations.GlobalNamespace.cs"));
                fileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "TestClass", MethodName = "TestMethod" }
                ]);
            });

    [TestMethod]
    public void VerifyMethodDeclarations_TestCode_GlobalNamespace_VB() =>
        CreateVisualBasicBuilder(isTestProject: true, "TestMethodDeclarations.GlobalNamespace.vb")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x =>
            {
                x.Should().HaveCount(1);
                var fileDeclarations = x.OrderBy(declaration => declaration.FilePath).First();
                fileDeclarations.AssemblyName.Should().Be("project0");
                fileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "TestMethodDeclarations.GlobalNamespace.vb"));
                fileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "TestClass", MethodName = "TestMethod" }
                ]);
            });

    [TestMethod]
    public void VerifyMethodDeclarations_MainCode_CSharp() =>
        CreateCSharpBuilder(isTestProject: false, "TestMethodDeclarations.cs", "TestMethodDeclarations.Partial.cs").VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => { x.Should().BeEmpty(); });

    [TestMethod]
    public void VerifyMethodDeclarations_NoDeclarations_CSharp() =>
        CreateCSharpBuilder(isTestProject: true, "TestMethodDeclarations.NoMethods.cs").VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => { x.Should().BeEmpty(); });

    [TestMethod]
    public void VerifyMethodDeclarations_TestCode_VB() =>
        CreateVisualBasicBuilder(isTestProject: true, "TestMethodDeclarations.vb", "TestMethodDeclarations.Partial.vb")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x =>
            {
                x.Should().HaveCount(2);
                var firstFileDeclarations = x.OrderBy(declaration => declaration.FilePath).First();
                firstFileDeclarations.AssemblyName.Should().Be("project0");
                firstFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "TestMethodDeclarations.Partial.vb"));
                firstFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "InSecondFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "PartialMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "Method" }
                ]);

                var secondFileDeclarations = x.OrderBy(declaration => declaration.FilePath).Skip(1).First();
                secondFileDeclarations.AssemblyName.Should().Be("project0");
                secondFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "TestMethodDeclarations.vb"));
                secondFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Address", MethodName = "GetZipCode" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.BaseClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.BaseClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.DerivedClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.DerivedClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.GenericClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.MultipleLevelInheritance", MethodName = "MultipleLevelInheritanceMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.MultipleLevelInheritance", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.MultipleLevelInheritance", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.IInterfaceWithTestDeclarations", MethodName = "GetZipCode" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.MultipleMethods", MethodName = "Method1" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.MultipleMethods", MethodName = "Method2" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.NoModifiers", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.OverloadedMethods", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "InFirstFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "PartialMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "BaseClassMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.PartialClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Person", MethodName = "GetFullName" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "FriendMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "NoAccessModifierMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "PrivateMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "PrivateProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "ProtectedFriendMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "ProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility", MethodName = "PublicMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility.FriendClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.Visibility.PrivateClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.VB.WithGenericMethod", MethodName = "Method" },
                ]);
            });

    [TestMethod]
    public void VerifyMethodDeclarations_MainCode_VB() =>
        CreateVisualBasicBuilder(isTestProject: false, "TestMethodDeclarations.vb", "TestMethodDeclarations.Partial.vb").VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => { x.Should().BeEmpty(); });

    [TestMethod]
    public void VerifyMethodDeclarations_NoDeclarations_VB() =>
        CreateVisualBasicBuilder(isTestProject: true, "TestMethodDeclarations.NoMethods.vb").VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => { x.Should().BeEmpty(); });

    private VerifierBuilder CreateCSharpBuilder(bool isTestProject, params string[] fileNames) =>
        CreateBuilder(LanguageOptions.CSharpLatest, new TestMetricsAnalyzerCSharp(GetFilePath(), isTestProject), fileNames);

    private VerifierBuilder CreateVisualBasicBuilder(bool isTestProject, params string[] fileNames) =>
        CreateBuilder(LanguageOptions.VisualBasicLatest, new TestMetricsAnalyzerVisualBasic(GetFilePath(), isTestProject), fileNames);

    private VerifierBuilder CreateBuilder(ImmutableArray<ParseOptions> parseOptions, DiagnosticAnalyzer analyzer, string[] fileNames) =>
        new VerifierBuilder()
            .AddAnalyzer(() => analyzer)
            .AddPaths(fileNames)
            .WithBasePath(BasePath)
            .WithOptions(parseOptions)
            .AddTestReference()
            .WithProtobufPath(@$"{GetFilePath()}\test-method-declarations.pb");

    private string GetFilePath() => Path.Combine(BasePath, TestContext.TestName!);

    private sealed class TestMetricsAnalyzerCSharp(string outPath, bool isTestProject) : SonarAnalyzer.Rules.CSharp.TestMethodDeclarationsAnalyzer
    {
        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }

    private sealed class TestMetricsAnalyzerVisualBasic(string outPath, bool isTestProject) : SonarAnalyzer.Rules.VisualBasic.TestMethodDeclarationsAnalyzer
    {
        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }
}
