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

namespace StyleCop.Analyzers.Lightup;

public enum UnaryOperatorKind
{
    /// <summary>
    /// Represents unknown or error operator kind.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Represents the C# '~' operator.
    /// </summary>
    BitwiseNegation = 0x1,

    /// <summary>
    /// Represents the C# '!' operator and VB 'Not' operator.
    /// </summary>
    Not = 0x2,

    /// <summary>
    /// Represents the unary '+' operator.
    /// </summary>
    Plus = 0x3,

    /// <summary>
    /// Represents the unary '-' operator.
    /// </summary>
    Minus = 0x4,

    /// <summary>
    /// Represents the C# 'true' operator and VB 'IsTrue' operator.
    /// </summary>
    True = 0x5,

    /// <summary>
    /// Represents the C# 'false' operator and VB 'IsFalse' operator.
    /// </summary>
    False = 0x6,

    /// <summary>
    /// Represents the C# '^' operator.
    /// </summary>
    Hat = 0x7,
}
