/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class MetricsAnalyzerBase : UtilityAnalyzerBase<MetricsInfo>
    {
        protected const string DiagnosticId = "S9999-metrics";
        protected const string Title = "Metrics calculator";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetUtilityDescriptor(DiagnosticId, Title);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        internal const string MetricsFileName = "metrics.pb";

        protected sealed override string FileName => MetricsFileName;

        protected abstract MetricsBase GetMetrics(SyntaxTree syntaxTree, SemanticModel semanticModel);

        protected sealed override MetricsInfo GetMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var metrics = GetMetrics(syntaxTree, semanticModel);
            return CalculateMetrics(metrics, syntaxTree.FilePath,
                syntaxTree.GetRoot().Language == LanguageNames.CSharp
                ? IgnoreHeaderComments[IgnoreHeaderCommentsCSharp]
                : IgnoreHeaderComments[IgnoreHeaderCommentsVisualBasic]);
        }

        internal /* for MsBuild12 support */ static MetricsInfo CalculateMetrics(MetricsBase metrics, string filePath, bool ignoreHeaderComments)
        {
            var complexity = metrics.Complexity;

            var metricsInfo = new MetricsInfo
            {
                FilePath = filePath,
                ClassCount = metrics.ClassCount,
                StatementCount = metrics.StatementCount,
                FunctionCount = metrics.FunctionCount,

                Complexity = complexity,

                CognitiveComplexity = metrics.CognitiveComplexity,
            };

            var comments = metrics.GetComments(ignoreHeaderComments);
            metricsInfo.NoSonarComment.AddRange(comments.NoSonar);
            metricsInfo.NonBlankComment.AddRange(comments.NonBlank);
            metricsInfo.CodeLine.AddRange(metrics.CodeLines);

            metricsInfo.ExecutableLines.AddRange(metrics.ExecutableLines);

            return metricsInfo;
        }
    }
}
