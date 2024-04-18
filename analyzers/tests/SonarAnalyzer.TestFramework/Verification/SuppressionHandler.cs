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

using System.Collections.Concurrent;
using System.Reflection;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.TestFramework.Verification;

public static class SuppressionHandler
{
    private static readonly ConcurrentDictionary<string, int> Counters = new();
    private static readonly PropertyInfo[] ShouldDiagnosticBeReportedProperties = LoadProperties();

    public static void HookSuppression()
    {
        var handler = HandleShouldDiagnosticBeReported;
        foreach (var property in ShouldDiagnosticBeReportedProperties)
        {
            property.SetValue(null, handler);
        }
    }

    public static void UnHookSuppression()
    {
        foreach (var property in ShouldDiagnosticBeReportedProperties)
        {
            property.SetValue(null, null);
        }
    }

    public static void IncrementReportCount(string ruleId) =>
        Counters.AddOrUpdate(ruleId, _ => 1, (_, count) => count + 1);

    public static bool ExtensionMethodsCalledForAllDiagnostics(IEnumerable<DiagnosticAnalyzer> analyzers) =>
        // In general this check is not very precise, because when the tests are run in parallel
        // we cannot determine which diagnostic was reported from which analyzer instance. In other
        // words, we cannot distinguish between diagnostics reported from different tests. That's
        // why we require each diagnostic to be reported through the extension methods at least once.
        analyzers.SelectMany(x => x.SupportedDiagnostics).Any(x => Counters.TryGetValue(x.Id, out var count) && count > 0);

    private static bool HandleShouldDiagnosticBeReported(SyntaxTree tree, Diagnostic diagnostic)
    {
        IncrementReportCount(diagnostic.Id);
        return true;
    }

    // We need to do this dynamically, becuase during UT run for SonarAnalyzer.CSharp.Styling.Test, there are two separate static classes SonarAnalysisContext:
    // - Public SonarAnalysisContext from TestFramework's dependency on SonarAnalyzer.Common, that is accessed by this supression class.
    // - Internal SonarAnalysisContext from ILMerged Internal.SonarAnalyzer.CSharp.Styling.dll, that is accessed by ReportIssue logic and is actually invoked.
    private static PropertyInfo[] LoadProperties() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.FullName.Contains(nameof(SonarAnalyzer)))
            .SelectMany(x => x.GetTypes())
            .Where(x => x.Name == nameof(SonarAnalysisContext))
            .Select(x => x.GetProperty(nameof(SonarAnalysisContext.ShouldDiagnosticBeReported)))
            .ToArray();
}
