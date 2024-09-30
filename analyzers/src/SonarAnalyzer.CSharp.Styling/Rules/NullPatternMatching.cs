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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullPatternMatching : StylingAnalyzer
{
    public NullPatternMatching() : base("T0007", "Use 'is {0}null' pattern matching.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => Validate(c, null), SyntaxKind.EqualsExpression);
        context.RegisterNodeAction(c => Validate(c, "not "), SyntaxKind.NotEqualsExpression);
    }

    private void Validate(SonarSyntaxNodeReportingContext context, string messageInfix)
    {
        var binary = (BinaryExpressionSyntax)context.Node;
        if ((binary.Left.IsKind(SyntaxKind.NullLiteralExpression) || binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
            && !context.IsInExpressionTree())
        {
            context.ReportIssue(Rule, binary, messageInfix);
        }
    }
}
