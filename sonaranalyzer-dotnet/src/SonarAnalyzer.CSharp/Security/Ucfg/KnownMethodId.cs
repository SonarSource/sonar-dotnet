/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

namespace SonarAnalyzer.Security.Ucfg
{
    public static class KnownMethodId
    {
        /// <summary>
        /// The method ID that the Security Engine uses for assignments. It accepts one argument
        /// and returns one value. For example, `a = x` generates `a = __id(x)`.
        /// </summary>
        public static readonly string Assignment = "__id";

        /// <summary>
        /// The method ID that the Security Engine uses for concatenations. It accepts two
        /// arguments and returns one value. For example, `x + y` generates `%0 = __concat(x, y)`
        /// </summary>
        public static readonly string Concatenation = "__concat";

        /// <summary>
        /// The method ID used by the UCFG Builder to represent a method that has no symbol.
        /// The instructions with this method ID are removed from UCFG.
        /// </summary>
        public static readonly string Unknown = "__unknown";
    }
}
