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

namespace SonarAnalyzer.Helpers
{
    public enum NetFrameworkVersion
    {
        /// <summary>Unknown (cannot tell)</summary>
        Unknown,
        /// <summary>Probably .NET 3.5</summary>
        Probably35,
        /// <summary>Between .NET 4.0 (inclusive) and .NET 4.5.1 (inclusive)</summary>
        Between4And451,
        /// <summary>Is .NET 4.5.2</summary>
        Is452,
        /// <summary>After .NET 4.6 (inclusive)</summary>
        After46,
    }
}
