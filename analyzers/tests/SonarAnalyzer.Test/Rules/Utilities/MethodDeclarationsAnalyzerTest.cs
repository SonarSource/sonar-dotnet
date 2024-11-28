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

using System.IO;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Test.Rules.Utilities;

[TestClass]
public class MethodDeclarationsAnalyzerTest
{
    private const string BasePath = @"Utilities\MethodDeclarationsAnalyzer\";

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void VerifyTypeDeclarations_TestCode_CSharp() =>
        CreateCSharpBuilder(isTestProject: true, "MethodDeclarations.cs", "MethodDeclarations.Partial.cs")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x =>
            {
                x.Should().HaveCount(2);
                var firstFileDeclarations = x.OrderBy(declaration => declaration.FilePath).First();
                firstFileDeclarations.AssemblyName.Should().Be("project0");
                firstFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "MethodDeclarations.cs"));
                firstFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.Address", MethodName = "GetZipCode" },
                    new MethodDeclarationInfo { TypeName = "Samples.Person", MethodName = "GetFullName" },
                    new MethodDeclarationInfo { TypeName = "Samples.Company", MethodName = "GetCompanyName" },
                    new MethodDeclarationInfo { TypeName = "Samples.Employee", MethodName = "GetEmployeeName" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "PublicMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "ProtectedInternalMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "ProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "InternalMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "PrivateProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "PrivateMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "NoAccessModifierMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility.InternalClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility.PrivateClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.FileClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.NoModifiers", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.MultipleMethods", MethodName = "Method1" },
                    new MethodDeclarationInfo { TypeName = "Samples.MultipleMethods", MethodName = "Method2" },
                    new MethodDeclarationInfo { TypeName = "Samples.Overloads", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.GenericClass<T>", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.WithGenericMethod", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "InFirstFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "PartialMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.LocalFunctions", MethodName = "Main" }
                ]);
                var secondFileDeclarations = x.OrderBy(declaration => declaration.FilePath).Skip(1).First();
                secondFileDeclarations.AssemblyName.Should().Be("project0");
                secondFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "MethodDeclarations.Partial.cs"));
                secondFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "InSecondFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "PartialMethod" }
                ]);
            });

    [TestMethod]
    public void VerifyTypeDeclarations_MainCode_CSharp() =>
        CreateCSharpBuilder(isTestProject: false, "MethodDeclarations.cs", "MethodDeclarations.Partial.cs").VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => { x.Should().BeEmpty(); });

    [TestMethod]
    public void VerifyTypeDeclarations_TestCode_VB() =>
        CreateVisualBasicBuilder(isTestProject: true, "MethodDeclarations.vb", "MethodDeclarations.Partial.vb")
            .VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x =>
            {
                x.Should().HaveCount(2);
                var firstFileDeclarations = x.OrderBy(declaration => declaration.FilePath).First();
                firstFileDeclarations.AssemblyName.Should().Be("project0");
                firstFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "MethodDeclarations.Partial.vb"));
                firstFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "InSecondFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "PartialMethod" }
                ]);

                var secondFileDeclarations = x.OrderBy(declaration => declaration.FilePath).Skip(1).First();
                secondFileDeclarations.AssemblyName.Should().Be("project0");
                secondFileDeclarations.FilePath.Should().Be(Path.Combine(BasePath, "MethodDeclarations.vb"));
                secondFileDeclarations.MethodDeclarations.Should().BeEquivalentTo([
                    new MethodDeclarationInfo { TypeName = "Samples.Address", MethodName = "GetZipCode" },
                    new MethodDeclarationInfo { TypeName = "Samples.Person", MethodName = "GetFullName" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "PublicMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "ProtectedFriendMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "ProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "FriendMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "PrivateProtectedMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "PrivateMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility", MethodName = "NoAccessModifierMethod" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility.FriendClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.Visibility.PrivateClass", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.NoModifiers", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.MultipleMethods", MethodName = "Method1" },
                    new MethodDeclarationInfo { TypeName = "Samples.MultipleMethods", MethodName = "Method2" },
                    new MethodDeclarationInfo { TypeName = "Samples.OverloadedMethods", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.GenericClass(Of T)", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.WithGenericMethod", MethodName = "Method" },
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "InFirstFile" },
                    new MethodDeclarationInfo { TypeName = "Samples.PartialClass", MethodName = "PartialMethod" }
                ]);
            });

    [TestMethod]
    public void VerifyTypeDeclarations_MainCode_VB() =>
        CreateVisualBasicBuilder(isTestProject: false, "MethodDeclarations.vb", "MethodDeclarations.Partial.vb").VerifyUtilityAnalyzer<MethodDeclarationsInfo>(x => { x.Should().BeEmpty(); });

    private VerifierBuilder CreateCSharpBuilder(bool isTestProject, params string[] fileNames) =>
        CreateBuilder(ParseOptionsHelper.CSharpLatest, new TestMetricsAnalyzerCSharp(GetFilePath(), isTestProject), fileNames);

    private VerifierBuilder CreateVisualBasicBuilder(bool isTestProject, params string[] fileNames) =>
        CreateBuilder(ParseOptionsHelper.VisualBasicLatest, new TestMetricsAnalyzerVisualBasic(GetFilePath(), isTestProject), fileNames);

    private VerifierBuilder CreateBuilder(ImmutableArray<ParseOptions> parseOptions, DiagnosticAnalyzer analyzer, string[] fileNames) =>
        new VerifierBuilder()
            .AddAnalyzer(() => analyzer)
            .AddPaths(fileNames)
            .WithBasePath(BasePath)
            .WithOptions(parseOptions)
            .AddTestReference()
            .WithProtobufPath(@$"{GetFilePath()}\method-declarations.pb");

    private string GetFilePath() => Path.Combine(BasePath, TestContext.TestName!);

    private sealed class TestMetricsAnalyzerCSharp(string outPath, bool isTestProject) : SonarAnalyzer.Rules.CSharp.MethodDeclarationsAnalyzer
    {
        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }

    private sealed class TestMetricsAnalyzerVisualBasic(string outPath, bool isTestProject) : SonarAnalyzer.Rules.VisualBasic.MethodDeclarationsAnalyzer
    {
        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }
}
