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

using Microsoft.CodeAnalysis.Simplification;

namespace StyleCop.Analyzers.Lightup;

public static class SimplifierWrapper
{
    private static readonly Func<SyntaxAnnotation> AddImportsAnnotationAccessor = LightupHelpers.CreateStaticPropertyAccessor<SyntaxAnnotation>(typeof(Simplifier), nameof(AddImportsAnnotation));

    /// <summary>
    /// Marker that tells the code-action cleanup pass (the import adder) where to resolve symbol-annotated nodes and add the missing
    /// <c>using</c>. The property exists on <c>Microsoft.CodeAnalysis.Simplification.Simplifier</c> from Roslyn 2.x onwards; it is
    /// <see langword="null"/> on the 1.x floor (the type ships, the property does not), so callers must guard before using it.
    /// </summary>
    public static SyntaxAnnotation AddImportsAnnotation => AddImportsAnnotationAccessor();
}
