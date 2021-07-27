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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class LogAnalyzerBase : UtilityAnalyzerBase<LogInfo>
    {
        protected const string DiagnosticId = "S9999-log";
        private const string Title = "Log generator";
        private const string MetricsFileName = "log.pb";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetUtilityDescriptor(DiagnosticId, Title);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected sealed override string FileName => MetricsFileName;

        protected sealed override LogInfo CreateAnalysisMessage(SonarAnalysisContext context) =>
            new LogInfo { Severity = LogSeverity.Info, Text = "Roslyn version: " + typeof(SyntaxNode).Assembly.GetName().Version };

        protected sealed override LogInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel) => null;
    }
}
