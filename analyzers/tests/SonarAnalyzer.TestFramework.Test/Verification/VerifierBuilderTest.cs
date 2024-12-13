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

using SonarAnalyzer.TestFramework.Verification;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class VerifierBuilderTest
{
    private static readonly VerifierBuilder Empty = new();

    [TestMethod]
    public void AddAnalyzer_Concatenates_IsImmutable()
    {
        var one = Empty.AddAnalyzer(() => new DummyAnalyzerCS());
        var two = one.AddAnalyzer(() => new DummyAnalyzerCS { DummyProperty = 42 });
        Empty.Analyzers.Should().BeEmpty();
        one.Analyzers.Should().ContainSingle().And.ContainSingle(x => ((DummyAnalyzerCS)x()).DummyProperty == 0);
        two.Analyzers.Should().HaveCount(2)
            .And.ContainSingle(x => ((DummyAnalyzerCS)x()).DummyProperty == 0)
            .And.ContainSingle(x => ((DummyAnalyzerCS)x()).DummyProperty == 42);
    }

    [TestMethod]
    public void AddAnalyzer_Generic_AddAnalyzer()
    {
        var sut = new VerifierBuilder<DummyAnalyzerCS>();
        sut.Analyzers.Should().ContainSingle().Which().Should().BeOfType<DummyAnalyzerCS>();
    }

    [TestMethod]
    public void AddPaths_Concatenates_IsImmutable()
    {
        var one = Empty.AddPaths("First");
        var three = one.AddPaths("Second", "Third");
        Empty.Paths.Should().BeEmpty();
        one.Paths.Should().ContainSingle().Which.Should().Be("First");
        three.Paths.Should().BeEquivalentTo("First", "Second", "Third");
    }

    [TestMethod]
    public void AddReferences_Concatenates_IsImmutable()
    {
        var one = Empty.AddReferences(MetadataReferenceFacade.MsCorLib);
        var two = one.AddReferences(MetadataReferenceFacade.SystemData);
        Empty.References.Should().BeEmpty();
        one.References.Should().BeEquivalentTo(MetadataReferenceFacade.MsCorLib).And.HaveCount(1);
        two.References.Should().BeEquivalentTo(MetadataReferenceFacade.MsCorLib.Concat(MetadataReferenceFacade.SystemData)).And.HaveCount(2);
    }

    [TestMethod]
    public void AddSnippet_Appends_IsImmutable()
    {
        var one = Empty.AddSnippet("First");
        var two = one.AddSnippet("Second", "WithFileName.cs");
        Empty.Snippets.Should().BeEmpty();
        one.Snippets.Should().Equal(new Snippet("First", null));
        two.Snippets.Should().Equal(new Snippet("First", null), new Snippet("Second", "WithFileName.cs"));
    }

    [TestMethod]
    public void AddTestReference_Concatenates_IsImmutable()
    {
        var one = Empty.AddReferences(MetadataReferenceFacade.MsCorLib);
        var two = one.AddTestReference();
        Empty.References.Should().BeEmpty();
        one.References.Should().BeEquivalentTo(MetadataReferenceFacade.MsCorLib).And.HaveCount(1);
        two.References.Should().BeEquivalentTo(MetadataReferenceFacade.MsCorLib.Concat(NuGetMetadataReference.MSTestTestFrameworkV1));
    }

    [TestMethod]
    public void WithAutogenerateConcurrentFiles_Overwrites_IsImmutable()
    {
        var one = Empty.WithAutogenerateConcurrentFiles(false);
        var two = one.WithAutogenerateConcurrentFiles(true);
        Empty.AutogenerateConcurrentFiles.Should().BeTrue();
        one.AutogenerateConcurrentFiles.Should().BeFalse();
        two.AutogenerateConcurrentFiles.Should().BeTrue();
    }

    [TestMethod]
    public void WithBasePath_Overwrites_IsImmutable()
    {
        var one = Empty.WithBasePath("Hotspots");
        var two = one.WithBasePath("SymbolicExecution");
        Empty.BasePath.Should().BeNull();
        one.BasePath.Should().Be("Hotspots");
        two.BasePath.Should().Be("SymbolicExecution");
    }

    [TestMethod]
    public void WithCodeFix_Overwrites_IsImmutable()
    {
        var one = Empty.WithCodeFix<DummyCodeFixCS>();
        var two = one.WithCodeFix<DummyCodeFixVB>();
        Empty.CodeFix.Should().BeNull();
        one.CodeFix().Should().BeOfType<DummyCodeFixCS>();
        two.CodeFix().Should().BeOfType<DummyCodeFixVB>();
    }

    [TestMethod]
    public void WithCodeFixedPaths_Overwrites_IsImmutable()
    {
        var one = Empty.WithCodeFixedPaths("First");
        var two = one.WithCodeFixedPaths("Second");
        var withBatch = one.WithCodeFixedPaths("Third", "Batch");
        Empty.CodeFixedPath.Should().BeNull();
        one.CodeFixedPath.Should().Be("First");
        two.CodeFixedPath.Should().Be("Second");
        withBatch.CodeFixedPath.Should().Be("Third");
        // Batch version should not be modified:
        Empty.CodeFixedPathBatch.Should().BeNull();
        one.CodeFixedPathBatch.Should().BeNull();
        two.CodeFixedPathBatch.Should().BeNull();
        withBatch.CodeFixedPathBatch.Should().Be("Batch");
    }

    [TestMethod]
    public void WithConcurrentAnalysis_Overwrites_IsImmutable()
    {
        var one = Empty.WithConcurrentAnalysis(false);
        var two = one.WithConcurrentAnalysis(true);
        Empty.ConcurrentAnalysis.Should().BeTrue();
        one.ConcurrentAnalysis.Should().BeFalse();
        two.ConcurrentAnalysis.Should().BeTrue();
    }

    [TestMethod]
    public void WithCodeFixTitle_Overwrites_IsImmutable()
    {
        var one = Empty.WithCodeFixTitle("First");
        var two = one.WithCodeFixTitle("Second");
        Empty.CodeFixTitle.Should().BeNull();
        one.CodeFixTitle.Should().Be("First");
        two.CodeFixTitle.Should().Be("Second");
    }

    [TestMethod]
    public void WithErrorBehavior_Overwrites_IsImmutable()
    {
        var one = Empty.WithErrorBehavior(CompilationErrorBehavior.FailTest);
        var two = one.WithErrorBehavior(CompilationErrorBehavior.Ignore);
        Empty.ErrorBehavior.Should().Be(CompilationErrorBehavior.Default);
        one.ErrorBehavior.Should().Be(CompilationErrorBehavior.FailTest);
        two.ErrorBehavior.Should().Be(CompilationErrorBehavior.Ignore);
    }

    [TestMethod]
    public void WithLanguageVersion_Overwrites_IsImmutable_CS()
    {
        var one = Empty.WithLanguageVersion(CS.LanguageVersion.CSharp10);
        var two = one.WithLanguageVersion(CS.LanguageVersion.CSharp7);
        Empty.ParseOptions.Should().BeEmpty();
        one.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<CS.CSharpParseOptions>().Which.LanguageVersion.Should().Be(CS.LanguageVersion.CSharp10);
        two.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<CS.CSharpParseOptions>().Which.LanguageVersion.Should().Be(CS.LanguageVersion.CSharp7);
    }

    [TestMethod]
    public void WithLanguageVersion_Overwrites_IsImmutable_VB()
    {
        var one = Empty.WithLanguageVersion(VB.LanguageVersion.VisualBasic16);
        var two = one.WithLanguageVersion(VB.LanguageVersion.VisualBasic10);
        Empty.ParseOptions.Should().BeEmpty();
        one.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<VB.VisualBasicParseOptions>().Which.LanguageVersion.Should().Be(VB.LanguageVersion.VisualBasic16);
        two.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<VB.VisualBasicParseOptions>().Which.LanguageVersion.Should().Be(VB.LanguageVersion.VisualBasic10);
    }

    [TestMethod]
    public void WithOnlyDiagnostics_Overwrites_IsImmutable()
    {
        var s1111 = AnalysisScaffolding.CreateDescriptor("S1111");
        var s2222 = AnalysisScaffolding.CreateDescriptor("S2222");
        var s2223 = AnalysisScaffolding.CreateDescriptor("S2223");
        var one = Empty.WithOnlyDiagnostics(s1111);
        var two = one.WithOnlyDiagnostics(s2222, s2223);
        Empty.OnlyDiagnostics.Should().BeEmpty();
        one.OnlyDiagnostics.Should().BeEquivalentTo(new[] { s1111 });
        two.OnlyDiagnostics.Should().BeEquivalentTo(new[] { s2222, s2223 });
    }

    [TestMethod]
    public void WithOptions_Overwrites_IsImmutable()
    {
        var only7 = Empty.WithOptions(LanguageOptions.OnlyCSharp7);
        var from8 = only7.WithOptions(LanguageOptions.FromCSharp8);
        Empty.ParseOptions.Should().BeEmpty();
        only7.ParseOptions.Should().BeEquivalentTo(LanguageOptions.OnlyCSharp7);
        from8.ParseOptions.Should().BeEquivalentTo(LanguageOptions.FromCSharp8);
    }

    [TestMethod]
    public void WithOutputKind_Overwrites_IsImmutable()
    {
        var one = Empty.WithOutputKind(OutputKind.WindowsApplication);
        var two = one.WithOutputKind(OutputKind.NetModule);
        Empty.OutputKind.Should().Be(OutputKind.DynamicallyLinkedLibrary);
        one.OutputKind.Should().Be(OutputKind.WindowsApplication);
        two.OutputKind.Should().Be(OutputKind.NetModule);
    }

    [TestMethod]
    public void WithProtobufPath_Overwrites_IsImmutable()
    {
        var one = Empty.WithProtobufPath("First");
        var two = one.WithProtobufPath("Second");
        Empty.ProtobufPath.Should().BeNull();
        one.ProtobufPath.Should().Be("First");
        two.ProtobufPath.Should().Be("Second");
    }

    [TestMethod]
    public void WithSonarProjectConfig_Overwrites_IsImmutable()
    {
        var one = Empty.WithAdditionalFilePath("First");
        var two = one.WithAdditionalFilePath("Second");
        var three = two.WithAdditionalFilePath(null);
        Empty.AdditionalFilePath.Should().BeNull();
        one.AdditionalFilePath.Should().Be("First");
        two.AdditionalFilePath.Should().Be("Second");
        three.AdditionalFilePath.Should().BeNull();
    }

    [TestMethod]
    public void WithTopLevelSupport_Overwrites_IsImmutable()
    {
        var sut = Empty.WithTopLevelStatements();
        Empty.OutputKind.Should().Be(OutputKind.DynamicallyLinkedLibrary);
        Empty.ParseOptions.Should().BeEmpty();
        sut.OutputKind.Should().Be(OutputKind.ConsoleApplication);
        sut.ParseOptions.Should().BeEquivalentTo(LanguageOptions.FromCSharp9);
    }

    [TestMethod]
    public void WithTopLevelSupport_PreservesParseOptions()
    {
        var sut = Empty.WithOptions(LanguageOptions.FromCSharp10).WithTopLevelStatements();
        sut.OutputKind.Should().Be(OutputKind.ConsoleApplication);
        sut.ParseOptions.Should().BeEquivalentTo(LanguageOptions.FromCSharp10);
    }

    [TestMethod]
    public void WithTopLevelSupport_ForVisualBasicOptions_NotSupported() =>
        Empty.WithOptions(LanguageOptions.FromVisualBasic15).Invoking(x => x.WithTopLevelStatements()).Should().Throw<InvalidOperationException>()
            .WithMessage("WithTopLevelStatements is not supported with VisualBasicParseOptions.");

    [TestMethod]
    public void WithTopLevelSupport_ForOldCSharp_NotSupported() =>
        Empty.WithOptions(LanguageOptions.FromCSharp8).Invoking(x => x.WithTopLevelStatements()).Should().Throw<InvalidOperationException>()
            .WithMessage("WithTopLevelStatements is supported from CSharp9.");

    [TestMethod]
    public void Build_ReturnsVerifier() =>
        new VerifierBuilder<DummyAnalyzerCS>().AddPaths("File.cs").Build().Should().NotBeNull();
}
