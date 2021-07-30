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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class LogAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, LogInfo>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S9999-log";
        private const string Title = "Log generator";

        protected abstract string LanguageVersion(Compilation compilation);

        protected sealed override string FileName => "log.pb";
        protected override bool AnalyzeGeneratedCode => true;

        protected LogAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override IEnumerable<LogInfo> CreateAnalysisMessage(CompilationAnalysisContext c) =>
            new[]
            {
                new LogInfo { Severity = LogSeverity.Info, Text = "Roslyn version: " + typeof(SyntaxNode).Assembly.GetName().Version },
                new LogInfo { Severity = LogSeverity.Info, Text = "Language version: " + LanguageVersion(c.Compilation) }
            };

        protected sealed override LogInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel) =>
            DiagnosticAnalyzerContextHelper.IsGenerated(syntaxTree, Language.GeneratedCodeRecognizer, semanticModel.Compilation)
            ? new LogInfo { Severity = LogSeverity.Debug, Text = $"File '{syntaxTree.FilePath}' was recognized as generated" }
            : null;
    }
}
