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
public sealed class ArrowLocation : StylingAnalyzer
{
    public ArrowLocation() : base("T0002", "Place the arrow at the end of the previous line.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => Verify(c, ((LambdaExpressionSyntax)c.Node).ArrowToken), SyntaxKind.SimpleLambdaExpression);
        context.RegisterNodeAction(c => Verify(c, ((LambdaExpressionSyntax)c.Node).ArrowToken), SyntaxKind.ParenthesizedLambdaExpression);
        context.RegisterNodeAction(c => Verify(c, ((ArrowExpressionClauseSyntax)c.Node).ArrowToken), SyntaxKind.ArrowExpressionClause);
        context.RegisterNodeAction(c => Verify(c, ((SwitchExpressionArmSyntax)c.Node).EqualsGreaterThanToken), SyntaxKind.SwitchExpressionArm);
    }

    private void Verify(SonarSyntaxNodeReportingContext context, SyntaxToken arrowToken)
    {
        if (!arrowToken.IsMissing && arrowToken.IsFirstTokenOnLine())
        {
            context.ReportIssue(Rule, arrowToken);
        }
    }
}
