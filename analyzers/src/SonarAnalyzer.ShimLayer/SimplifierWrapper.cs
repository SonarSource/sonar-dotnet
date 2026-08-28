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

namespace SonarAnalyzer.ShimLayer;

// This one is from Microsoft.CodeAnalysis.Workspaces assembly. We don't want to shim it all for one member. This should get removed if we need more things from there.
public static class SimplifierWrapper
{
    private static readonly Func<SyntaxAnnotation> AddImportsAnnotationAccessor = AccessorFactory.CreateStaticProperty<Func<SyntaxAnnotation>>(typeof(Simplifier), "AddImportsAnnotation");

    /// <summary>
    /// Marker that tells the code-action cleanup pass (the import adder) where to resolve symbol-annotated nodes and add the missing <c>using</c>.
    /// </summary>
    public static SyntaxAnnotation AddImportsAnnotation => AddImportsAnnotationAccessor();
}
