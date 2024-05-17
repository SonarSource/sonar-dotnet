/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using RoslynAnalysisContext = Microsoft.CodeAnalysis.Diagnostics.AnalysisContext;

namespace SonarAnalyzer.Analyzers
{
    public abstract class SonarDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public static readonly string EnableConcurrentExecutionVariable = "SONAR_DOTNET_ENABLE_CONCURRENT_EXECUTION";

        protected virtual bool EnableConcurrentExecution => IsConcurrentExecutionEnabled();

        protected abstract void Initialize(SonarAnalysisContext context);

        public sealed override void Initialize(RoslynAnalysisContext context)
        {
            // The default values are Analyze | ReportDiagnostics. We do this call to make sure it will be still enabled even if the default values changed. (Needed for the razor analysis)
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            if (EnableConcurrentExecution)
            {
                context.EnableConcurrentExecution();
            }
            Initialize(new SonarAnalysisContext(context, SupportedDiagnostics));
        }

        protected static bool IsConcurrentExecutionEnabled()
        {
            var value = Environment.GetEnvironmentVariable(EnableConcurrentExecutionVariable);

            if (value != null && bool.TryParse(value, out var result))
            {
                return result;
            }
            return true;
        }
    }

    public abstract class SonarDiagnosticAnalyzer<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected abstract string MessageFormat { get; }
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected DiagnosticDescriptor Rule { get; }
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected SonarDiagnosticAnalyzer(string diagnosticId) =>
           Rule = Language.CreateDescriptor(diagnosticId, MessageFormat);
    }
}
