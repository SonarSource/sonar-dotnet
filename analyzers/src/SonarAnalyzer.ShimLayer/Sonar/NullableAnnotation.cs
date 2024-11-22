/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

public enum NullableAnnotation : byte
{
    /// <summary>
    /// The expression has not been analyzed, or the syntax is not an expression (such as a statement).
    /// </summary>
    None = 0,

    /// <summary>
    /// The expression is not annotated (does not have a ?).
    /// </summary>
    NotAnnotated = 1,

    /// <summary>
    /// The expression is annotated (does have a ?).
    /// </summary>
    Annotated = 2,
}
