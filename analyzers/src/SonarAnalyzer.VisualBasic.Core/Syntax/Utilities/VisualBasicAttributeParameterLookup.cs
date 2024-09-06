/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;

internal class VisualBasicAttributeParameterLookup(SeparatedSyntaxList<ArgumentSyntax> argumentList, IMethodSymbol methodSymbol) : MethodParameterLookupBase<ArgumentSyntax>(argumentList, methodSymbol)
{
    protected override SyntaxNode Expression(ArgumentSyntax argument) =>
        argument.GetExpression();

    protected override SyntaxToken? GetNameColonIdentifier(ArgumentSyntax argument) =>
        null;

    protected override SyntaxToken? GetNameEqualsIdentifier(ArgumentSyntax argument) =>
        argument is SimpleArgumentSyntax { NameColonEquals.Name.Identifier: var identifier }
            ? identifier
            : null;
}
