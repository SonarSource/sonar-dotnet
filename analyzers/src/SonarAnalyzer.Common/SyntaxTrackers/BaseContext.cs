﻿/*
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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Base contxt information with semantic model and secondary location.
    /// </summary>
    public class BaseContext
    {
        private readonly List<Location> secondaryLocations;

        public SemanticModel SemanticModel { get; }
        public IEnumerable<Location> SecondaryLocations => secondaryLocations;

        public BaseContext(SemanticModel semanticModel)
        {
            SemanticModel = semanticModel;
            secondaryLocations = new List<Location>();
        }

        public void AddSecondaryLocation(Location location)
        {
            Debug.Assert(location != null, "Location should not be null.");
            Debug.Assert(location != Location.None, "Location should not be equal to None.");
            secondaryLocations.Add(location);
        }
    }
}
