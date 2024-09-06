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

using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Trackers;

public class VisualBasicArgumentTracker : ArgumentTracker<SyntaxKind>
{
    protected override SyntaxKind[] TrackedSyntaxKinds => [SyntaxKind.SimpleArgument];

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode) =>
        argumentNode is ArgumentSyntax { Parent: ArgumentListSyntax { Arguments: { } list } }
            ? list
            : null;

    protected override int? Position(SyntaxNode argumentNode) =>
        argumentNode is ArgumentSyntax { IsNamed: true }
        ? null
        : ArgumentList(argumentNode)?.IndexOf(x => x == argumentNode);

    protected override RefKind? ArgumentRefKind(SyntaxNode argumentNode) =>
        null;

    protected override bool InvocationMatchesMemberKind(SyntaxNode invokedExpression, MemberKind memberKind) =>
        memberKind switch
        {
            MemberKind.Method => invokedExpression is InvocationExpressionSyntax,
            MemberKind.Constructor => invokedExpression is ObjectCreationExpressionSyntax,
            MemberKind.Indexer => invokedExpression is InvocationExpressionSyntax,
            MemberKind.Attribute => invokedExpression is AttributeSyntax,
            _ => false,
        };

    protected override bool InvokedMemberMatches(SemanticModel model, SyntaxNode invokedExpression, MemberKind memberKind, Func<string, bool> invokedMemberNameConstraint) =>
        invokedMemberNameConstraint(invokedExpression.GetName());
}
