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
public sealed class IndentRawString : IndentBase
{
    public IndentRawString() : base("T0042", "raw string literal") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var token = c.Node.GetLastToken();
                if (token.Kind() is SyntaxKind.MultiLineRawStringLiteralToken or SyntaxKind.InterpolatedRawStringEndToken or SyntaxKind.Utf8MultiLineRawStringLiteralToken
                    && c.Node.GetLocation() is var location
                    && location.StartLine() != location.EndLine()
                    && ExpectedPosition(c.Node) is { } expected)
                {
                    var line = c.Node.SyntaxTree.GetText().Lines[token.GetLocation().GetLineSpan().EndLinePosition.Line];
                    var actual = line.ToString().IndexOf(@"""");

                    if (actual != expected) // This does not support TAB characters
                    {
                        c.ReportIssue(Rule, Location.Create(token.SyntaxTree, new TextSpan(line.Start + actual, 3)), (expected + 1).ToString());
                    }
                }
            },
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.InterpolatedStringExpression,
            SyntaxKind.Utf8StringLiteralExpression);

    protected override bool IsIgnored(SyntaxNode node) =>
        node is LambdaExpressionSyntax or IfStatementSyntax or WhileStatementSyntax or ForStatementSyntax;

    protected override SyntaxNode NodeRoot(SyntaxNode node, SyntaxNode current)
    {
        if (current is StatementSyntax
            or ObjectCreationExpressionSyntax { Parent: ArrowExpressionClauseSyntax }
            or ImplicitObjectCreationExpressionSyntax { Parent: ArrowExpressionClauseSyntax }
            or AssignmentExpressionSyntax
            or SwitchExpressionArmSyntax)
        {
            return current;
        }
        else if (current is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccessOnRawString } && memberAccessOnRawString.Expression == node)
        {
            return null;    // We don't want anything, when the invocation is on the raw string itself
        }
        else if (current is InvocationExpressionSyntax invocation && invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.OperatorToken.IsFirstTokenOnLine())
        {
            return memberAccess.Name;   // Off by one due to the dot
        }
        else if (current is InvocationExpressionSyntax or CollectionExpressionSyntax && current.GetFirstToken().IsFirstTokenOnLine())
        {
            return current;
        }
        else if (current is ArrowExpressionClauseSyntax)
        {
            return current.Parent;
        }
        else
        {
            return null;
        }
    }
}
