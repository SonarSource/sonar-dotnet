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

namespace SonarAnalyzer.Rules.CSharp.Styling;

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
        if (!arrowToken.IsMissing && arrowToken.Line() > arrowToken.GetPreviousToken().Line())
        {
            context.ReportIssue(Rule, arrowToken);
        }
    }
}
