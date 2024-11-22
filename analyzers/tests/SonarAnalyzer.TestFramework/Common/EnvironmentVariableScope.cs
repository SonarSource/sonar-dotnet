/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.TestFramework.Common;

/// <summary>
/// Defines a scope inside which new environment variables can be set.
/// The variables will be cleared when the scope is disposed.
/// </summary>
public sealed class EnvironmentVariableScope : IDisposable
{
    private readonly bool setOnlyInAzureDevOpsContext;
    private IDictionary<string, string> originalValues = new Dictionary<string, string>();

#pragma warning disable S2376 // Write-only properties should not be used
    public bool EnableConcurrentAnalysis
    {
        set => SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, value.ToString());
    }
#pragma warning restore S2376

    public EnvironmentVariableScope(bool setVariablesOnlyInAzureDevOpsContext = true) =>
        setOnlyInAzureDevOpsContext = setVariablesOnlyInAzureDevOpsContext;

    public void SetVariable(string name, string value)
    {
        if (setOnlyInAzureDevOpsContext && !TestContextHelper.IsAzureDevOpsContext)
        {
            return;
        }
        // Store the original value, or null if there isn't one
        if (!originalValues.ContainsKey(name))
        {
            originalValues.Add(name, Environment.GetEnvironmentVariable(name));
        }
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    public void Dispose()
    {
        if (originalValues != null)
        {
            foreach (var kvp in originalValues)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
            originalValues = null;
        }
    }
}
