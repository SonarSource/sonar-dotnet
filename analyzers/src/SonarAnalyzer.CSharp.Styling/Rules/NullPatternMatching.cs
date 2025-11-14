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
public sealed class NullPatternMatching : StylingAnalyzer
{
    public NullPatternMatching() : base("T0007", "Use 'is {0}null' pattern matching.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => Validate(c, null), SyntaxKind.EqualsExpression);
        context.RegisterNodeAction(c => Validate(c, "not "), SyntaxKind.NotEqualsExpression);
        context.RegisterNodeAction(c =>
            {
                if (((RecursivePatternSyntax)c.Node) is { Designation: null, PropertyPatternClause.Subpatterns.Count: 0 })
                {
                    c.ReportIssue(Rule, c.Node, "not ");
                }
            },
            SyntaxKind.RecursivePattern);
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
