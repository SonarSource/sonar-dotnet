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

namespace SonarAnalyzer.Extensions;

public static class DiagnosticDescriptorExtensions
{
    public static bool IsEnabled(this DiagnosticDescriptor descriptor, SonarSyntaxNodeReportingContext context)
    {
        if (context.HasMatchingScope(descriptor))
        {
            // Roslyn calls an analyzer if any of the diagnostics is active. We need to remove deactivated rules from execution to improve overall performance.
            // This is a reproduction of Roslyn activation logic:
            // https://github.com/dotnet/roslyn/blob/0368609e1467563247e9b5e4e3fe8bff533d59b6/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalyzerDriver.cs#L1316-L1327
            var options = CompilationOptionsWrapper.FromObject(context.Compilation.Options).SyntaxTreeOptionsProvider;
            var severity = options.TryGetDiagnosticValue(context.Tree, descriptor.Id, default, out var severityFromOptions)
                || options.TryGetGlobalDiagnosticValue(descriptor.Id, default, out severityFromOptions)
                ? severityFromOptions                                               // .editorconfig for a specific tree
                : descriptor.GetEffectiveSeverity(context.Compilation.Options);     // RuleSet file or .globalconfig
            return severity switch
            {
                ReportDiagnostic.Default => descriptor.IsEnabledByDefault,
                ReportDiagnostic.Suppress => false,
                _ => true
            };
        }
        else
        {
            return false;
        }
    }

    [Obsolete("Use ReportIssue overload with DiagnosticDescriptor and IEnumerable<SecondaryLocations> parameters instead.")]
    public static Diagnostic CreateDiagnostic(this DiagnosticDescriptor descriptor,
                                              Compilation compilation,
                                              Location location,
                                              IEnumerable<Location> additionalLocations,
                                              ImmutableDictionary<string, string> properties,
                                              params object[] messageArgs) =>
        Diagnostic.Create(descriptor, location, additionalLocations?.Where(y => y.IsValid(compilation)), properties, messageArgs);
}
