/*
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

using SonarAnalyzer.Common;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules
{
    public abstract class MetricsAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, MetricsInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-metrics";
        private const string Title = "Metrics calculator";

        protected abstract MetricsBase GetMetrics(SyntaxTree syntaxTree, SemanticModel semanticModel);

        protected sealed override string FileName => "metrics.pb";
        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { AnalyzeTestProjects = false };

        protected MetricsAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override MetricsInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
        {
            var metrics = GetMetrics(tree, model);
            var complexity = metrics.Complexity;
            var metricsInfo = new MetricsInfo
            {
                FilePath = tree.GetRoot().GetMappedFilePathFromRoot(),
                ClassCount = metrics.ClassCount,
                StatementCount = metrics.StatementCount,
                FunctionCount = metrics.FunctionCount,
                Complexity = complexity,
                CognitiveComplexity = metrics.CognitiveComplexity,
            };

            var comments = metrics.GetComments(parameters.IgnoreHeaderComments);
            metricsInfo.NoSonarComment.AddRange(comments.NoSonar);
            metricsInfo.NonBlankComment.AddRange(comments.NonBlank);
            metricsInfo.CodeLine.AddRange(metrics.CodeLines);
            metricsInfo.ExecutableLines.AddRange(metrics.ExecutableLines);

            return metricsInfo;
        }
    }
}
