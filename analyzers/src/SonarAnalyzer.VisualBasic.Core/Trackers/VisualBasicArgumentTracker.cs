/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

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
