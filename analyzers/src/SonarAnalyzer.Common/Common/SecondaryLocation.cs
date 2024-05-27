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

namespace SonarAnalyzer.Common;

public record SecondaryLocation(Location Location, string Message);

public static class SecondaryLocationExtensions
{
    [Obsolete("Use ReportIssue overload with IEnumerable<SecondaryLocation> parameter instead.")]
    public static IEnumerable<Location> ToAdditionalLocations(this IEnumerable<SecondaryLocation> secondaryLocations) =>
        secondaryLocations.Select(x => x.Location);

    [Obsolete("Use ReportIssue overload with IEnumerable<SecondaryLocation> parameter instead.")]
    public static ImmutableDictionary<string, string> ToProperties(this IEnumerable<SecondaryLocation> secondaryLocations) =>
        secondaryLocations
            .Select((item, index) => new { item.Message, Index = index.ToString() })
            .ToDictionary(x => x.Index, x => x.Message)
            .ToImmutableDictionary();

    [Obsolete("Use ReportIssue overload with IEnumerable<SecondaryLocation> parameter instead.")]
    public static ImmutableDictionary<string, string> ToProperties(this IEnumerable<Location> secondaryLocations, string message) =>
          secondaryLocations
            .Select((item, index) => new { message, Index = index.ToString() })
            .ToDictionary(x => x.Index, x => message)
            .ToImmutableDictionary();
}
