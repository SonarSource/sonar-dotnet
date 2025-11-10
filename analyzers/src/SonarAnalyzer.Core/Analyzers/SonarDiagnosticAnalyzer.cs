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

using RoslynAnalysisContext = Microsoft.CodeAnalysis.Diagnostics.AnalysisContext;

namespace SonarAnalyzer.Core.Analyzers;

#pragma warning disable T0038 // Use field instead of protected auto-property. We want these class to provide uniform experience.

public abstract class SonarDiagnosticAnalyzer : DiagnosticAnalyzer
{
    public static readonly string EnableConcurrentExecutionVariable = "SONAR_DOTNET_ENABLE_CONCURRENT_EXECUTION";

    protected abstract void Initialize(SonarAnalysisContext context);

    protected virtual bool EnableConcurrentExecution => IsConcurrentExecutionEnabled();

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
        return value is null || !bool.TryParse(value, out var result) || result;
    }
}

public abstract class SonarDiagnosticAnalyzer<TSyntaxKind> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
{
    protected abstract string MessageFormat { get; }
    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    protected DiagnosticDescriptor Rule { get; }

    protected SonarDiagnosticAnalyzer(string diagnosticId) =>
       Rule = Language.CreateDescriptor(diagnosticId, MessageFormat);
}
