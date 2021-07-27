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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class MetricsAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, MetricsInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-metrics";
        private const string Title = "Metrics calculator";

        protected abstract MetricsBase GetMetrics(SyntaxTree syntaxTree, SemanticModel semanticModel);

        protected sealed override string FileName => "metrics.pb";
        protected sealed override bool AnalyzeTestProjects => false;

        protected MetricsAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override MetricsInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var metrics = GetMetrics(syntaxTree, semanticModel);
            var complexity = metrics.Complexity;
            var metricsInfo = new MetricsInfo
            {
                FilePath = syntaxTree.FilePath,
                ClassCount = metrics.ClassCount,
                StatementCount = metrics.StatementCount,
                FunctionCount = metrics.FunctionCount,
                Complexity = complexity,
                CognitiveComplexity = metrics.CognitiveComplexity,
            };

            var comments = metrics.GetComments(IgnoreHeaderComments);
            metricsInfo.NoSonarComment.AddRange(comments.NoSonar);
            metricsInfo.NonBlankComment.AddRange(comments.NonBlank);
            metricsInfo.CodeLine.AddRange(metrics.CodeLines);
            metricsInfo.ExecutableLines.AddRange(metrics.ExecutableLines);

            return metricsInfo;
        }
    }
}
