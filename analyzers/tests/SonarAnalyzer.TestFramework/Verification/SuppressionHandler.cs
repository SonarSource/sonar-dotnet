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
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Test.TestFramework;

public static class SuppressionHandler
{
    private static readonly ConcurrentDictionary<string, int> Counters = new();

    public static void HookSuppression() =>
        SonarAnalysisContext.ShouldDiagnosticBeReported = (_, d) =>
            {
                IncrementReportCount(d.Id);
                return true;
            };

    public static void UnHookSuppression() => SonarAnalysisContext.ShouldDiagnosticBeReported = null;

    public static void IncrementReportCount(string ruleId) =>
        Counters.AddOrUpdate(ruleId, _ => 1, (_, count) => count + 1);

    public static bool ExtensionMethodsCalledForAllDiagnostics(IEnumerable<DiagnosticAnalyzer> analyzers) =>
        // In general this check is not very precise, because when the tests are run in parallel
        // we cannot determine which diagnostic was reported from which analyzer instance. In other
        // words, we cannot distinguish between diagnostics reported from different tests. That's
        // why we require each diagnostic to be reported through the extension methods at least once.
        analyzers.SelectMany(x => x.SupportedDiagnostics).Any(x => Counters.TryGetValue(x.Id, out var count) && count > 0);
}
