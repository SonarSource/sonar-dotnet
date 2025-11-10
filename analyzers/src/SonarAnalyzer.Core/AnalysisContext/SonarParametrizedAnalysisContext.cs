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

namespace SonarAnalyzer.Core.AnalysisContext;

public sealed class SonarParametrizedAnalysisContext : SonarAnalysisContext
{
    private readonly List<Action<SonarCompilationStartAnalysisContext>> postponedActions = new();

    internal SonarParametrizedAnalysisContext(SonarAnalysisContext context) : base(context) { }

    /// <summary>
    /// Register CompilationStart action that will be executed once rule parameters are set.
    /// </summary>
    public override void RegisterCompilationStartAction(Action<SonarCompilationStartAnalysisContext> action) =>
        postponedActions.Add(action);

    /// <summary>
    /// Execution of postponed registration actions. This should be called once all rule parameters are set.
    /// </summary>
    public void ExecutePostponedActions(SonarCompilationStartAnalysisContext context)
    {
        foreach (var action in postponedActions)
        {
            action(context);
        }
    }
}
