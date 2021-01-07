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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class SymbolicExecutionAnalyzerFactoryTest
    {
        private readonly List<string> symbolicExecutionRuleIds = new ()
        {
            EmptyNullableValueAccess.DiagnosticId,
            ObjectsShouldNotBeDisposedMoreThanOnce.DiagnosticId,
            PublicMethodArgumentsShouldBeCheckedForNull.DiagnosticId,
            EmptyCollectionsShouldNotBeEnumerated.DiagnosticId,
            ConditionEvaluatesToConstant.S2583DiagnosticId,
            ConditionEvaluatesToConstant.S2589DiagnosticId,
            InvalidCastToInterfaceRuleConstants.DiagnosticId,
            NullPointerDereference.DiagnosticId,
            RestrictDeserializedTypes.DiagnosticId,
            InitializationVectorShouldBeRandom.DiagnosticId,
            HashesShouldHaveUnpredictableSalt.DiagnosticId
        };

        [TestMethod]
        public void SupportedDiagnostics_ReturnsSymbolicExecutionRuleDescriptors()
        {
            var sut = new SymbolicExecutionAnalyzerFactory();
            var supportedDiagnostics = sut.SupportedDiagnostics.Select(descriptor => descriptor.Id).ToList();

            CollectionAssert.AreEquivalent(supportedDiagnostics, symbolicExecutionRuleIds);
        }

        [DataTestMethod]
        [DataRow(ReportDiagnostic.Default)]
        [DataRow(ReportDiagnostic.Error)]
        [DataRow(ReportDiagnostic.Info)]
        [DataRow(ReportDiagnostic.Warn)]
        // According to the doc of Hidden: Non-visible to user. The diagnostic is reported to the IDE diagnostic engine, however.
        // https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019#rule-severity
        [DataRow(ReportDiagnostic.Hidden)]
        public void GetEnabledAnalyzers_ReturnsDiagnostic_WhenEnabled(ReportDiagnostic reportDiagnostic)
        {
            var sut = new SymbolicExecutionAnalyzerFactory();
            var diagnostics = symbolicExecutionRuleIds.ToImmutableDictionary(v => v, v => v == EmptyNullableValueAccess.DiagnosticId ? reportDiagnostic : ReportDiagnostic.Suppress);

            var context = CreateSyntaxNodeAnalysisContext(diagnostics);
            var analyzers = sut.GetEnabledAnalyzers(context).ToList();
            var enabledAnalyzers =
                analyzers
                    .SelectMany(analyzer => analyzer.SupportedDiagnostics.Select(descriptor => descriptor.Id))
                    .ToList();

            CollectionAssert.AreEquivalent(enabledAnalyzers, new[] {EmptyNullableValueAccess.DiagnosticId});
        }

        [TestMethod]
        public void GetEnabledAnalyzers_ReturnsEmptyList_WhenDiagnosticsAreDisabled()
        {
            var sut = new SymbolicExecutionAnalyzerFactory();
            var diagnostics = symbolicExecutionRuleIds.ToImmutableDictionary(v => v, _ => ReportDiagnostic.Suppress);
            var context = CreateSyntaxNodeAnalysisContext(diagnostics);
            var analyzers = sut.GetEnabledAnalyzers(context).ToList();
            var enabledAnalyzers =
                analyzers
                    .SelectMany(analyzer => analyzer.SupportedDiagnostics.Select(descriptor => descriptor.Id))
                    .ToList();

            CollectionAssert.AreEquivalent(new List<string>(), enabledAnalyzers);
        }

        private static SyntaxNodeAnalysisContext CreateSyntaxNodeAnalysisContext(ImmutableDictionary<string, ReportDiagnostic> diagnostics)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"public class Empty { }", new CSharpParseOptions());

            return new SyntaxNodeAnalysisContext(syntaxTree.GetRoot(),
                                                 CreateCSharpCompilation(diagnostics, syntaxTree).GetSemanticModel(syntaxTree),
                                                 new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty),
                                                 _ => { },
                                                 _ => true,
                                                 CancellationToken.None);
        }

        private static CSharpCompilation CreateCSharpCompilation(ImmutableDictionary<string, ReportDiagnostic> diagnostics, SyntaxTree syntaxTree) =>
            CSharpCompilation
                .Create("Assembly.dll", new[] {syntaxTree}, null, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(diagnostics));
    }
}
