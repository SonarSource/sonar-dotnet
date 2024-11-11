﻿/*
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

using Microsoft.CodeAnalysis.Shared.Extensions;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DebuggerDisplayUsesExistingMembers : DebuggerDisplayUsesExistingMembersBase<AttributeArgumentSyntax, SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxNode AttributeTarget(AttributeArgumentSyntax attribute) =>
        attribute.GetAncestor<AttributeListSyntax>()?.Parent;

    protected override ImmutableArray<SyntaxNode> ResolvableIdentifiers(SyntaxNode expression)
    {
        return expression.DescendantNodesAndSelf().Any(x => x.Kind() is SyntaxKindEx.SingleVariableDesignation)
            ? ImmutableArray<SyntaxNode>.Empty // A variable was declared inside the expression. This would result in FPs and needs more advanced handling.
            : MostLeftIdentifier(expression);

        static ImmutableArray<SyntaxNode> MostLeftIdentifier(SyntaxNode node) =>
            node is ExpressionSyntax expression
            ? expression.ExtractMemberIdentifier().Select(x => x.GetLeftMostInMemberAccess()).OfType<IdentifierNameSyntax>().ToImmutableArray<SyntaxNode>()
            : ImmutableArray<SyntaxNode>.Empty;
    }
}
