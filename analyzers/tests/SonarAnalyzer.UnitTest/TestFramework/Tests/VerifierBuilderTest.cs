/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public class VerifierBuilderTest
    {
        private static readonly VerifierBuilder Empty = new();

        [TestMethod]
        public void AddAnalyzer_Concatenates_IsImmutable()
        {
            var one = Empty.AddAnalyzer(() => new DummyAnalyzer());
            var two = one.AddAnalyzer(() => new DummyAnalyzer { DummyProperty = 42 });
            Empty.Analyzers.Should().BeEmpty();
            one.Analyzers.Should().ContainSingle().And.ContainSingle(x => ((DummyAnalyzer)x()).DummyProperty == 0);
            two.Analyzers.Should().HaveCount(2)
                .And.ContainSingle(x => ((DummyAnalyzer)x()).DummyProperty == 0)
                .And.ContainSingle(x => ((DummyAnalyzer)x()).DummyProperty == 42);
        }

        [TestMethod]
        public void AddAnalyzer_Generic_AddAnalyzer()
        {
            var sut = new VerifierBuilder<DummyAnalyzer>();
            sut.Analyzers.Should().ContainSingle().Which().Should().BeOfType<DummyAnalyzer>();
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
        public void WithErrorBehavior_Overrides_IsImmutable()
        {
            var one = Empty.WithErrorBehavior(CompilationErrorBehavior.FailTest);
            var two = Empty.WithErrorBehavior(CompilationErrorBehavior.Ignore);
            Empty.ErrorBehavior.Should().Be(CompilationErrorBehavior.Default);
            one.ErrorBehavior.Should().Be(CompilationErrorBehavior.FailTest);
            two.ErrorBehavior.Should().Be(CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void WithOnlyDiagnostics_Overrides_IsImmutable()
        {
            var one = Empty.WithOnlyDiagnostics(NullPointerDereference.S2259);
            var two = Empty.WithOnlyDiagnostics(PublicMethodArgumentsShouldBeCheckedForNull.S3900, ConditionEvaluatesToConstant.S2583);
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
            var two = Empty.WithOutputKind(OutputKind.NetModule);
            Empty.OutputKind.Should().Be(OutputKind.DynamicallyLinkedLibrary);
            one.OutputKind.Should().Be(OutputKind.WindowsApplication);
            two.OutputKind.Should().Be(OutputKind.NetModule);
        }

        [TestMethod]
        public void WithTopLevelSupport_Overrides_IsImmutable() =>
            Empty.WithTopLevelStatements().OutputKind.Should().Be(OutputKind.ConsoleApplication);

        [TestMethod]
        public void Build_ReturnsVerifier() =>
            new VerifierBuilder<DummyAnalyzer>().AddPaths("File.cs").Build().Should().NotBeNull();
    }
}
