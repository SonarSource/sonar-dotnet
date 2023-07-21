/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
    public static Diagnostic CreateDiagnostic(this DiagnosticDescriptor descriptor,
                                              Compilation compilation,
                                              Location location,
                                              IEnumerable<Location> additionalLocations,
                                              params object[] messageArgs) =>
        Diagnostic.Create(descriptor, location.EnsureMappedLocation(), additionalLocations?.Where(x => IsLocationValid(x, compilation)), messageArgs);

    private static bool IsLocationValid(Location location, Compilation compilation) =>
        location.Kind != LocationKind.SourceFile || compilation.ContainsSyntaxTree(location.SourceTree);
}
