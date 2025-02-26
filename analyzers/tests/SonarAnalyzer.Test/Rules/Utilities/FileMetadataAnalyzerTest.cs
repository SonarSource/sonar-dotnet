﻿/*
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
using SonarAnalyzer.Core.Rules;
using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class FileMetadataAnalyzerTest
{
    private const string BasePath = @"Utilities\FileMetadataAnalyzer\";

    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Autogenerated(ProjectType projectType)
    {
        var autogeneratedProjectFiles = new[]
        {
            "autogenerated_comment.cs",
            "autogenerated_comment2.cs",
            "class.designer.cs",
            "class.g.cs",
            "class.g.something.cs",
            "class.generated.cs",
            "class_generated.cs",
            "compiler_generated.cs",
            "compiler_generated_attr.cs",
            "debugger_non_user_code.cs",
            "debugger_non_user_code_attr.cs",
            "generated_code_attr.cs",
            "generated_code_attr_local_function.cs",
            "generated_code_attr2.cs",
            "TEMPORARYGENERATEDFILE_class.cs"
        };
        VerifyAllFilesAreGenerated(projectType, autogeneratedProjectFiles, autogeneratedProjectFiles);
    }

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void NotAutogenerated(ProjectType projectType)
    {
        var notAutogeneratedFiles = new[]
        {
            "normal_file.cs",
            "generated_region.cs",
            "generated_region_2.cs"
        };
        CreateBuilder(projectType, notAutogeneratedFiles)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, projectType))
            .VerifyUtilityAnalyzer<FileMetadataInfo>(x =>
                x.Should().BeEquivalentTo(notAutogeneratedFiles.Select(expected => new FileMetadataInfo
                {
                    IsGenerated = false,
                    FilePath = BasePath + expected,
                })));
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void CreateMessage_NoEncoding_SetsEmptyString(bool isTestProject)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns("File.Generated.cs");    // Generated to simplify mocking for GeneratedCodeRecognizer
        tree.Encoding.Returns(x => null);
        var model = TestCompiler.CompileCS(string.Empty).Model;
        var sut = new TestFileMetadataAnalyzer(null, isTestProject);

        sut.TestCreateMessage(UtilityAnalyzerParameters.Default, tree, model).Encoding.Should().BeEmpty();
    }

    [DataTestMethod]
    [DataRow("class.generated.cs", 0)]
    [DataRow("SomethingElse.cs", 1)]
    public void Verify_UnchangedFiles(string unchangedFileName, int expectedFileCount) =>
        CreateBuilder(ProjectType.Product, "class.generated.cs")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName))
            .VerifyUtilityAnalyzer<FileMetadataInfo>(x => x.Should().HaveCount(expectedFileCount));

#if NET

    [DataTestMethod]
    [DataRow("Razor.razor")]
    [DataRow("Razor.cshtml")]
    public void Verify_RazorFilesAreIgnored(string fileName) =>
        CreateBuilder(ProjectType.Product, fileName)
            .VerifyUtilityAnalyzer<FileMetadataInfo>(x =>
                x.Select(fileInfo => Path.GetFileName(fileInfo.FilePath)).Should().BeEmpty());    // There are more files on some PCs: JSExports.g.cs, LibraryImports.g.cs, JSImports.g.cs

#endif

    private void VerifyAllFilesAreGenerated(ProjectType projectType, string[] projectFiles, string[] autogeneratedFiles) =>
        CreateBuilder(projectType, projectFiles)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, projectType))
            .VerifyUtilityAnalyzer<FileMetadataInfo>(x =>
            {
                x.Should().AllBeEquivalentTo(new { IsGenerated = true });
                x.Should().SatisfyRespectively(autogeneratedFiles.Select<string, Action<FileMetadataInfo>>(expected => actual => actual.FilePath.EndsWith(expected)));
            });

    private VerifierBuilder CreateBuilder(ProjectType projectType, params string[] projectFiles)
    {
        var testRoot = BasePath + TestContext.TestName;
        return new VerifierBuilder()
            .AddAnalyzer(() => new TestFileMetadataAnalyzer(testRoot, projectType == ProjectType.Test))
            .AddPaths(projectFiles)
            .WithBasePath(BasePath)
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithProtobufPath(@$"{testRoot}\file-metadata.pb");
    }

    // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
    private sealed class TestFileMetadataAnalyzer : FileMetadataAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestFileMetadataAnalyzer(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };

        public FileMetadataInfo TestCreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model) =>
            CreateMessage(parameters, tree, model);
    }
}
