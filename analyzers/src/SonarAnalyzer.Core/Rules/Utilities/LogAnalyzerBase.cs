/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules
{
    public abstract class LogAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, LogInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-log";
        private const string Title = "Log generator";

        protected abstract string LanguageVersion(Compilation compilation);

        protected sealed override string FileName => "log.pb";
        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { AnalyzeGeneratedCode = true };

        protected LogAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override IEnumerable<LogInfo> CreateAnalysisMessages(SonarCompilationReportingContext c) =>
            new[]
            {
                new LogInfo { Severity = LogSeverity.Info, Text = "Roslyn version: " + typeof(SyntaxNode).Assembly.GetName().Version },
                new LogInfo { Severity = LogSeverity.Info, Text = "Language version: " + LanguageVersion(c.Compilation) },
                new LogInfo { Severity = LogSeverity.Info, Text = "Concurrent execution: " + (IsConcurrentExecutionEnabled() ? "enabled" : "disabled") }
            };

        protected sealed override LogInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model) =>
            tree.IsGenerated(Language.GeneratedCodeRecognizer)
            ? CreateMessage(tree)
            : null;

        private static LogInfo CreateMessage(SyntaxTree tree) =>
            GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
                ? new LogInfo { Severity = LogSeverity.Debug, Text = $"File '{tree.FilePath}' was recognized as razor generated" }
                : new LogInfo { Severity = LogSeverity.Debug, Text = $"File '{tree.FilePath}' was recognized as generated" };
    }
}
