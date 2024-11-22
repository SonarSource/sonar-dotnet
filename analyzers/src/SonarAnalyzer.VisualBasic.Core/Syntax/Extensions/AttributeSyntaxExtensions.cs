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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

internal static class AttributeSyntaxExtensions
{
    private const int AttributeLength = 9;

    public static bool IsKnownType(this AttributeSyntax attribute, KnownType knownType, SemanticModel semanticModel) =>
        attribute.Name.GetName().Contains(GetShortNameWithoutAttributeSuffix(knownType))
        && ((SyntaxNode)attribute).IsKnownType(knownType, semanticModel);

    private static string GetShortNameWithoutAttributeSuffix(KnownType knownType) =>
        knownType.TypeName == nameof(Attribute) || !knownType.TypeName.EndsWith(nameof(Attribute))
            ? knownType.TypeName
            : knownType.TypeName.Remove(knownType.TypeName.Length - AttributeLength);
}
