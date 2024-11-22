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

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

internal class CSharpAttributeParameterLookup(AttributeSyntax attribute, IMethodSymbol methodSymbol) :
    MethodParameterLookupBase<AttributeArgumentSyntax>(attribute.ArgumentList?.Arguments ?? default, methodSymbol)
{
    protected override SyntaxNode Expression(AttributeArgumentSyntax argument) =>
        argument.Expression;

    protected override SyntaxToken? GetNameColonIdentifier(AttributeArgumentSyntax argument) =>
        argument.NameColon?.Name.Identifier;

    protected override SyntaxToken? GetNameEqualsIdentifier(AttributeArgumentSyntax argument) =>
        argument.NameEquals?.Name.Identifier;
}
