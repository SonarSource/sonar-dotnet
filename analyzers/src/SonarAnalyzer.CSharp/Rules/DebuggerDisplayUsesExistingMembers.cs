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

using Microsoft.CodeAnalysis.Shared.Extensions;

namespace SonarAnalyzer.CSharp.Rules;

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
