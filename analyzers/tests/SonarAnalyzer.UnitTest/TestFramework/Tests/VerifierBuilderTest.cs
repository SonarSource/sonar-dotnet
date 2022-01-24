/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
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
            var two = one.AddSnippet("Second");
            Empty.Snippets.Should().BeEmpty();
            one.Snippets.Should().BeEquivalentTo("First");
            two.Snippets.Should().BeEquivalentTo("First", "Second");
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
        public void WithAutogenerateConcurrentFiles_Overrides_IsImmutable()
        {
            var one = Empty.WithAutogenerateConcurrentFiles(false);
            var two = one.WithAutogenerateConcurrentFiles(true);
            Empty.AutogenerateConcurrentFiles.Should().BeTrue();
            one.AutogenerateConcurrentFiles.Should().BeFalse();
            two.AutogenerateConcurrentFiles.Should().BeTrue();
        }

        [TestMethod]
        public void WithBasePath_Overrides_IsImmutable()
        {
            var one = Empty.WithBasePath("Hotspots");
            var two = one.WithBasePath("SymbolicExecution");
            Empty.BasePath.Should().BeNull();
            one.BasePath.Should().Be("Hotspots");
            two.BasePath.Should().Be("SymbolicExecution");
        }

        [TestMethod]
        public void WithCodeFix_Overrides_IsImmutable()
        {
            var one = Empty.WithCodeFix<DummyCodeFixCS>();
            var two = one.WithCodeFix<DummyCodeFixVB>();
            Empty.CodeFix.Should().BeNull();
            one.CodeFix().Should().BeOfType<DummyCodeFixCS>();
            two.CodeFix().Should().BeOfType<DummyCodeFixVB>();
        }

        [TestMethod]
        public void WithCodeFixedPaths_Overrides_IsImmutable()
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
        public void WithConcurrentAnalysis_Overrides_IsImmutable()
        {
            var one = Empty.WithConcurrentAnalysis(false);
            var two = one.WithConcurrentAnalysis(true);
            Empty.ConcurrentAnalysis.Should().BeTrue();
            one.ConcurrentAnalysis.Should().BeFalse();
            two.ConcurrentAnalysis.Should().BeTrue();
        }

        [TestMethod]
        public void WithCodeFixTitle_Overrides_IsImmutable()
        {
            var one = Empty.WithCodeFixTitle("First");
            var two = one.WithCodeFixTitle("Second");
            Empty.CodeFixTitle.Should().BeNull();
            one.CodeFixTitle.Should().Be("First");
            two.CodeFixTitle.Should().Be("Second");
        }

        [TestMethod]
        public void WithErrorBehavior_Overrides_IsImmutable()
        {
            var one = Empty.WithErrorBehavior(CompilationErrorBehavior.FailTest);
            var two = one.WithErrorBehavior(CompilationErrorBehavior.Ignore);
            Empty.ErrorBehavior.Should().Be(CompilationErrorBehavior.Default);
            one.ErrorBehavior.Should().Be(CompilationErrorBehavior.FailTest);
            two.ErrorBehavior.Should().Be(CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void WithLanguageVersion_Overrides_IsImmutable_CS()
        {
            var one = Empty.WithLanguageVersion(CS.LanguageVersion.CSharp10);
            var two = one.WithLanguageVersion(CS.LanguageVersion.CSharp7);
            Empty.ParseOptions.Should().BeEmpty();
            one.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<CS.CSharpParseOptions>().Which.LanguageVersion.Should().Be(CS.LanguageVersion.CSharp10);
            two.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<CS.CSharpParseOptions>().Which.LanguageVersion.Should().Be(CS.LanguageVersion.CSharp7);
        }

        [TestMethod]
        public void WithLanguageVersion_Overrides_IsImmutable_VB()
        {
            var one = Empty.WithLanguageVersion(VB.LanguageVersion.VisualBasic16);
            var two = one.WithLanguageVersion(VB.LanguageVersion.VisualBasic10);
            Empty.ParseOptions.Should().BeEmpty();
            one.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<VB.VisualBasicParseOptions>().Which.LanguageVersion.Should().Be(VB.LanguageVersion.VisualBasic16);
            two.ParseOptions.Should().ContainSingle().Which.Should().BeOfType<VB.VisualBasicParseOptions>().Which.LanguageVersion.Should().Be(VB.LanguageVersion.VisualBasic10);
        }

        [TestMethod]
        public void WithOnlyDiagnostics_Overrides_IsImmutable()
        {
            var one = Empty.WithOnlyDiagnostics(NullPointerDereference.S2259);
            var two = one.WithOnlyDiagnostics(PublicMethodArgumentsShouldBeCheckedForNull.S3900, ConditionEvaluatesToConstant.S2583);
            Empty.OnlyDiagnostics.Should().BeEmpty();
            one.OnlyDiagnostics.Should().BeEquivalentTo(NullPointerDereference.S2259);
            two.OnlyDiagnostics.Should().BeEquivalentTo(PublicMethodArgumentsShouldBeCheckedForNull.S3900, ConditionEvaluatesToConstant.S2583);
        }

        [TestMethod]
        public void WithOptions_Overrides_IsImmutable()
        {
            var only7 = Empty.WithOptions(ParseOptionsHelper.OnlyCSharp7);
            var from8 = only7.WithOptions(ParseOptionsHelper.FromCSharp8);
            Empty.ParseOptions.Should().BeEmpty();
            only7.ParseOptions.Should().BeEquivalentTo(ParseOptionsHelper.OnlyCSharp7);
            from8.ParseOptions.Should().BeEquivalentTo(ParseOptionsHelper.FromCSharp8);
        }

        [TestMethod]
        public void WithOutputKind_Overrides_IsImmutable()
        {
            var one = Empty.WithOutputKind(OutputKind.WindowsApplication);
            var two = one.WithOutputKind(OutputKind.NetModule);
            Empty.OutputKind.Should().Be(OutputKind.DynamicallyLinkedLibrary);
            one.OutputKind.Should().Be(OutputKind.WindowsApplication);
            two.OutputKind.Should().Be(OutputKind.NetModule);
        }

        [TestMethod]
        public void WithProtobufPath_Overrides_IsImmutable()
        {
            var one = Empty.WithProtobufPath("First");
            var two = one.WithProtobufPath("Second");
            Empty.ProtobufPath.Should().BeNull();
            one.ProtobufPath.Should().Be("First");
            two.ProtobufPath.Should().Be("Second");
        }

        [TestMethod]
        public void WithSonarProjectConfig_Overrides_IsImmutable()
        {
            var one = Empty.WithSonarProjectConfigPath("First");
            var two = one.WithSonarProjectConfigPath("Second");
            var three = two.WithSonarProjectConfigPath(null);
            Empty.SonarProjectConfigPath.Should().BeNull();
            one.SonarProjectConfigPath.Should().Be("First");
            two.SonarProjectConfigPath.Should().Be("Second");
            three.SonarProjectConfigPath.Should().BeNull();
        }

        [TestMethod]
        public void WithTopLevelSupport_Overrides_IsImmutable()
        {
            var sut = Empty.WithTopLevelStatements();
            Empty.OutputKind.Should().Be(OutputKind.DynamicallyLinkedLibrary);
            Empty.ParseOptions.Should().BeEmpty();
            sut.OutputKind.Should().Be(OutputKind.ConsoleApplication);
            sut.ParseOptions.Should().BeEquivalentTo(ParseOptionsHelper.FromCSharp9);
        }

        [TestMethod]
        public void WithTopLevelSupport_PreservesParseOptions()
        {
            var sut = Empty.WithOptions(ParseOptionsHelper.FromCSharp10).WithTopLevelStatements();
            sut.OutputKind.Should().Be(OutputKind.ConsoleApplication);
            sut.ParseOptions.Should().BeEquivalentTo(ParseOptionsHelper.FromCSharp10);
        }

        [TestMethod]
        public void WithTopLevelSupport_ForVisualBasicOptions_NotSupported() =>
            Empty.WithOptions(ParseOptionsHelper.FromVisualBasic15).Invoking(x => x.WithTopLevelStatements()).Should().Throw<InvalidOperationException>()
                .WithMessage("WithTopLevelStatements is not supported with VisualBasicParseOptions.");

        [TestMethod]
        public void WithTopLevelSupport_ForOldCSharp_NotSupported() =>
            Empty.WithOptions(ParseOptionsHelper.FromCSharp8).Invoking(x => x.WithTopLevelStatements()).Should().Throw<InvalidOperationException>()
                .WithMessage("WithTopLevelStatements is supported from CSharp9.");

        [TestMethod]
        public void Build_ReturnsVerifier() =>
            new VerifierBuilder<DummyAnalyzerCS>().AddPaths("File.cs").Build().Should().NotBeNull();
    }
}
