/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Runner
{
    public class DiagnosticsRunner
    {
        private readonly Configuration configuration;

        public DiagnosticsRunner(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation)
        {
            var diagnosticAnalyzers = configuration.GetAnalyzers();

            if (diagnosticAnalyzers.IsDefaultOrEmpty)
            {
                return new Diagnostic[0];
            }

            var compilationOptions = compilation.Language == LanguageNames.CSharp
                ? (CompilationOptions)new CS.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                : new VB.VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                diagnosticAnalyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics)
                    .Select(diagnostic =>
                        new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn)));

            var modifiedCompilation = compilation.WithOptions(compilationOptions);

            using (var tokenSource = new CancellationTokenSource())
            {
                var additionalFiles = new[] {
                    new AnalyzerAdditionalFile(configuration.SonarLintAdditionalPath),
                    new AnalyzerAdditionalFile(configuration.ProtoFolderAdditionalPath)}
                    .Where(a => a.Path != null)
                    .ToArray();

                if (!string.IsNullOrEmpty(configuration.ProtoFolderAdditionalPath))
                {
                    var utilityAnalyzers = configuration.GetUtilityAnalyzers();
                    diagnosticAnalyzers = diagnosticAnalyzers.Union(utilityAnalyzers).ToImmutableArray();
                }

                var compilationWithAnalyzer = modifiedCompilation.WithAnalyzers(
                    diagnosticAnalyzers,
                    new AnalyzerOptions(ImmutableArray.Create<AdditionalText>(additionalFiles)),
                    tokenSource.Token);

                return compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync().Result;
            }
        }
    }
}
