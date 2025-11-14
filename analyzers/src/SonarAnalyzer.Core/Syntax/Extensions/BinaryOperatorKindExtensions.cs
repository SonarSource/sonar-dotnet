/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class BinaryOperatorKindExtensions
{
    public static bool IsAnyEquality(this BinaryOperatorKind kind) =>
        kind.IsEquals() || kind.IsNotEquals();

    public static bool IsEquals(this BinaryOperatorKind kind) =>
        kind is BinaryOperatorKind.Equals or BinaryOperatorKind.ObjectValueEquals;

    public static bool IsNotEquals(this BinaryOperatorKind kind) =>
        kind is BinaryOperatorKind.NotEquals or BinaryOperatorKind.ObjectValueNotEquals;

    public static bool IsAnyRelational(this BinaryOperatorKind kind) =>
        kind is BinaryOperatorKind.GreaterThan or BinaryOperatorKind.GreaterThanOrEqual or BinaryOperatorKind.LessThan or BinaryOperatorKind.LessThanOrEqual;
}
