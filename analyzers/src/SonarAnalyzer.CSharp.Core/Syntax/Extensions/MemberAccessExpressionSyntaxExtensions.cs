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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class MemberAccessExpressionSyntaxExtensions
{
    public static bool IsMemberAccessOnKnownType(this MemberAccessExpressionSyntax memberAccess, string name, KnownType knownType, SemanticModel semanticModel) =>
        memberAccess.NameIs(name)
        && semanticModel.GetSymbolInfo(memberAccess).Symbol is {} symbol
        && symbol.ContainingType.DerivesFrom(knownType);

    public static bool IsPropertyInvocation(this MemberAccessExpressionSyntax expression, ImmutableArray<KnownType> types, string propertyName, SemanticModel semanticModel) =>
        expression.NameIs(propertyName) &&
        semanticModel.GetSymbolInfo(expression).Symbol is IPropertySymbol propertySymbol &&
        propertySymbol.IsInType(types);
}
