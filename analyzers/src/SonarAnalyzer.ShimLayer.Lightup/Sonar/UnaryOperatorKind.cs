/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
