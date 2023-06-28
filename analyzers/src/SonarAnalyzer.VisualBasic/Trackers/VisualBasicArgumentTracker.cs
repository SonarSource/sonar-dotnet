/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Helpers.Trackers;

public class VisualBasicArgumentTracker : ArgumentTracker<SyntaxKind>
{
    protected override SyntaxKind[] TrackedSyntaxKinds => new[] { SyntaxKind.SimpleArgument };

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode) =>
        argumentNode switch
        {
            ArgumentSyntax { Parent: ArgumentListSyntax { Arguments: { } list } } => list,
            _ => null,
        };

    protected override int? Position(SyntaxNode argumentNode) =>
        argumentNode is ArgumentSyntax { IsNamed: true }
        ? null
        : ArgumentList(argumentNode).IndexOf(x => x == argumentNode);

    protected override RefKind ArgumentRefKind(SyntaxNode argumentNode) =>
        RefKind.None;

    protected override bool InvocationFitsMemberKind(SyntaxNode argumentNode, InvokedMemberKind memberKind)
    {
        var invocationExpression = argumentNode?.Parent?.Parent;
        return memberKind switch
        {
            InvokedMemberKind.Method => invocationExpression is InvocationExpressionSyntax,
            InvokedMemberKind.Constructor => invocationExpression is ObjectCreationExpressionSyntax,
            InvokedMemberKind.Indexer => invocationExpression is InvocationExpressionSyntax,
            InvokedMemberKind.Attribute => invocationExpression is AttributeSyntax,
            _ => false,
        };
    }
    protected override bool InvokedMemberFits(SemanticModel model, SyntaxNode argumentNode, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint)
    {
        var expression = InvokedExpression(argumentNode);
        var name = expression.GetName();
        return invokedMemberNameConstraint(name);
    }

    protected override SyntaxNode InvokedExpression(SyntaxNode argumentNode) =>
        argumentNode?.Parent?.Parent;

    protected override SyntaxBaseContext CreateContext(SonarSyntaxNodeReportingContext context) => new(context);
}
