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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    internal sealed class SymbolicExecutionAnalyzerFactory : IRuleFactory
    {
        private readonly ImmutableArray<ISymbolicExecutionAnalyzer> analyzers;

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public SymbolicExecutionAnalyzerFactory()
        { }

        internal SymbolicExecutionAnalyzerFactory(params ISymbolicExecutionAnalyzer[] analyzers)
        {
            // Symbolic execution analyzers and supported diagnostics will not change at runtime so they can be safely cached.
            this.analyzers = analyzers.ToImmutableArray();
            SupportedDiagnostics = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics).ToImmutableArray();
        }

        public IEnumerable<ISymbolicExecutionAnalyzer> GetEnabledAnalyzers(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun) =>
             // Enabled analyzers can be changed at runtime in the IDE or by using connected mode in SonarLint and because of this they cannot be cached.
             analyzers.Where(x => x.SupportedDiagnostics.Any(descriptor => IsEnabled(context, isTestProject, isScannerRun, descriptor)));

        // We need to rewrite this https://github.com/SonarSource/sonar-dotnet/issues/4824
        public static bool IsEnabled(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun, DiagnosticDescriptor descriptor) =>
            SonarAnalysisContext.IsAnalysisScopeMatching(context.Compilation, isTestProject, isScannerRun, new[] { descriptor })
            && descriptor.GetEffectiveSeverity(context.Compilation.Options) != ReportDiagnostic.Suppress;
    }
}
