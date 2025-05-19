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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IndentInvocation : IndentBase
{
    public IndentInvocation() : base("T0026", "member access") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c =>
            {
                var memberAccess = (MemberAccessExpressionSyntax)c.Node;
                if (ExpectedPosition(memberAccess) is { } expected)
                {
                    Verify(c, expected, memberAccess.OperatorToken, memberAccess.Name);
                }
            },
            SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterNodeAction(c =>
            {
                var memberBinding = (MemberBindingExpressionSyntax)c.Node;
                if (ExpectedPosition(memberBinding) is { } expected)
                {
                    Verify(c, expected, memberBinding.OperatorToken, memberBinding.Name);
                }
            },
            SyntaxKind.MemberBindingExpression);
    }

    protected override SyntaxNode NodeRoot(SyntaxNode node, SyntaxNode current)
    {
        if (current is InvocationExpressionSyntax { Parent: ConditionalAccessExpressionSyntax })
        {
            return null;
        }
        else if (current.Parent is ConditionalExpressionSyntax ternary && (ternary.WhenTrue == current || ternary.WhenFalse == current))
        {
            return current;
        }
        else if (current is BinaryExpressionSyntax binary && binary.OperatorToken.IsFirstTokenOnLine())
        {
            return binary;
        }
        else
        {
            return base.NodeRoot(node, current);
        }
    }
}
