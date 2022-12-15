/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using static SonarAnalyzer.Helpers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer.Extensions;

public static class DiagnosticDescriptorExtensions
{
    public static bool HasMatchingScope(this DiagnosticDescriptor descriptor, Compilation compilation, bool isTestProject, bool isScannerRun)   // FIXME: Pass context instead?
    {
        if (compilation is null)
        {
            return true;    // We don't know the project type without the compilation so let's run the rule
        }
        // MMF-2297: Test Code as 1st Class Citizen is not ready on server side yet.
        // ScannerRun: Only utility rules and rules with TEST-ONLY scope are executed for test projects for now.
        // SonarLint & Standalone NuGet: Respect the scope as before.
        return isTestProject
            ? ContainsTag(TestSourceScopeTag) && !(isScannerRun && ContainsTag(MainSourceScopeTag) && !ContainsTag(UtilityTag))
            : ContainsTag(MainSourceScopeTag);

        bool ContainsTag(string tag) =>
            descriptor.CustomTags.Contains(tag);
    }

    public static Diagnostic CreateDiagnostic(this DiagnosticDescriptor descriptor,
                                              Compilation compilation,
                                              Location location,
                                              IEnumerable<Location> additionalLocations,
                                              params object[] messageArgs) =>
        Diagnostic.Create(descriptor, location, additionalLocations?.Where(x => IsLocationValid(x, compilation)), messageArgs);

    private static bool IsLocationValid(Location location, Compilation compilation) =>
        location.Kind != LocationKind.SourceFile || compilation.ContainsSyntaxTree(location.SourceTree);


}
