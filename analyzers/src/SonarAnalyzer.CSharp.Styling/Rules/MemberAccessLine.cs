/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MemberAccessLine : StylingAnalyzer
{
    public MemberAccessLine() : base("T0027", "Move this member access to the next line.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var memberAccess = (MemberAccessExpressionSyntax)c.Node;
                if (!memberAccess.HasSameStartLineAs(memberAccess.Name)
                    && memberAccess.OperatorToken.GetPreviousToken().GetLocation().EndLine() == memberAccess.Name.GetLocation().StartLine()
                    && FindStartLineMemberAccess(memberAccess) is { } startLineMemberAccess
                    && !IsIgnored(startLineMemberAccess, memberAccess))
                {
                    c.ReportIssue(Rule, Location.Create(c.Tree, TextSpan.FromBounds(memberAccess.OperatorToken.SpanStart, memberAccess.Span.End)));
                }
            },
            SyntaxKind.SimpleMemberAccessExpression);

    private static MemberAccessExpressionSyntax FindStartLineMemberAccess(SyntaxNode node) =>
        node switch
        {
            MemberAccessExpressionSyntax memberAccess when memberAccess.OperatorToken.Line() != memberAccess.OperatorToken.GetPreviousToken().Line() => memberAccess,
            MemberAccessExpressionSyntax memberAccess => FindStartLineMemberAccess(memberAccess.Expression),
            InvocationExpressionSyntax invocation => FindStartLineMemberAccess(invocation.Expression),
            ElementAccessExpressionSyntax elementAccess => FindStartLineMemberAccess(elementAccess.Expression),
            _ => null
        };

    private static bool IsIgnored(MemberAccessExpressionSyntax startLine, MemberAccessExpressionSyntax current) =>
        (startLine.Name is IdentifierNameSyntax { Identifier.ValueText: "And" or "Which" or "Subject" } && startLine.OperatorToken.IsFirstTokenOnLine())
        || (current.Expression is InvocationExpressionSyntax invocation && invocation.NameIs("Should"));
}
