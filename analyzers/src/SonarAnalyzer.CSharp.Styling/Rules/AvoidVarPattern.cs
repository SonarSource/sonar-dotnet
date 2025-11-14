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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidVarPattern : StylingAnalyzer
{
    public AvoidVarPattern() : base("T0034", "Avoid embedding this var pattern. Declare it in var statement instead.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (c.Node.Parent is IsPatternExpressionSyntax && CanBeExtracted(c.Node.Parent))
                {
                    c.ReportIssue(Rule, c.Node);
                }
            },
            SyntaxKind.VarPattern);

    private static bool CanBeExtracted(SyntaxNode node)
    {
        if (node.Parent is BinaryExpressionSyntax { Parent: not BinaryExpressionSyntax } binaryLast && binaryLast.Right == node)
        {
            node = node.Parent;
        }
        else
        {
            while (node.Parent is BinaryExpressionSyntax binaryFirst && binaryFirst.Left == node)
            {
                node = node.Parent;
            }
        }
        return node.Parent is IfStatementSyntax { Parent: not ElseClauseSyntax } or WhileStatementSyntax or ForStatementSyntax;
    }
}
