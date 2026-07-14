/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    extension(BinaryOperatorKind kind)
    {
        public bool IsAnyEquality => kind is { IsEquals: true } or { IsNotEquals: true };

        public bool IsEquals => kind is BinaryOperatorKind.Equals or BinaryOperatorKind.ObjectValueEquals;

        public bool IsNotEquals => kind is BinaryOperatorKind.NotEquals or BinaryOperatorKind.ObjectValueNotEquals;

        public bool IsAnyRelational => kind is BinaryOperatorKind.GreaterThan or BinaryOperatorKind.GreaterThanOrEqual or BinaryOperatorKind.LessThan or BinaryOperatorKind.LessThanOrEqual;
    }
}
