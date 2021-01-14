/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Common
{
    public class SecondaryLocation
    {
        public SecondaryLocation(Location location, string message)
        {
            Location = location;
            Message = message;
        }

        public Location Location { get; }
        public string Message { get; }
    }

    public static class SecondaryLocationHelper
    {
        public static IEnumerable<Location> ToAdditionalLocations(this IEnumerable<SecondaryLocation> secondaryLocations) =>
            secondaryLocations.Select(x => x.Location);

        public static ImmutableDictionary<string, string> ToProperties(this IEnumerable<SecondaryLocation> secondaryLocations) =>
            secondaryLocations
                .Select((item, index) => new { item.Message, Index = index.ToString() })
                .ToDictionary(i => i.Index, i => i.Message)
                .ToImmutableDictionary();

        public static ImmutableDictionary<string, string> ToProperties(this IEnumerable<Location> secondaryLocations, string message) =>
              secondaryLocations
                .Select((item, index) => new { message, Index = index.ToString() })
                .ToDictionary(i => i.Index, i => message)
                .ToImmutableDictionary();

        public static SecondaryLocation GetSecondaryLocation(this Diagnostic diagnostic, int index) =>
            diagnostic.AdditionalLocations.Count <= index
                    ? throw new ArgumentOutOfRangeException(nameof(index))
                    : new SecondaryLocation(diagnostic.AdditionalLocations[index],
                                            diagnostic.Properties.GetValueOrDefault(index.ToString()));
    }
}
