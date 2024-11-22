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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class ObjectCreationExpressionSyntaxExtensions
{
    public static bool IsKnownType(this ObjectCreationExpressionSyntax objectCreation, KnownType knownType, SemanticModel semanticModel) =>
        objectCreation.Type.GetName().EndsWith(knownType.TypeName)
        && ((SyntaxNode)objectCreation).IsKnownType(knownType, semanticModel);

    public static SyntaxToken? GetObjectCreationTypeIdentifier(this ObjectCreationExpressionSyntax objectCreation) =>
        objectCreation?.Type.GetIdentifier();
}
