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

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class AccessibilityExtensions
{
    /// <summary>
    /// Beware of Accessibility members:
    /// ProtectedOrInternal = C# "protected internal" or VB.NET "Protected Friend" syntax. Accessible from inheriting class OR the same assembly.
    /// ProtectedAndInternal = C# "private protected" or VB.NET "Private Protected" syntax. Accessible only from inheriting class in the same assembly.
    /// </summary>
    public static bool IsAccessibleOutsideTheType(this Accessibility accessibility) =>
        accessibility == Accessibility.Public || accessibility == Accessibility.Internal || accessibility == Accessibility.ProtectedOrInternal;
}
