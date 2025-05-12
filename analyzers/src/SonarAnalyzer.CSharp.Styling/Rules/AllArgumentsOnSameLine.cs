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
public sealed class AllArgumentsOnSameLine : StylingAnalyzer
{
    public AllArgumentsOnSameLine() : base("T0028", "All arguments should be on the same line or on separate lines.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => Verify(c, ((BaseArgumentListSyntax)c.Node).Arguments), SyntaxKind.ArgumentList);
        context.RegisterNodeAction(c => Verify(c, ((BracketedArgumentListSyntax)c.Node).Arguments), SyntaxKind.BracketedArgumentList);
        context.RegisterNodeAction(c => Verify(c, ((TypeArgumentListSyntax)c.Node).Arguments), SyntaxKind.TypeArgumentList);
        context.RegisterNodeAction(c => Verify(c, ((AttributeArgumentListSyntax)c.Node).Arguments), SyntaxKind.AttributeArgumentList);
    }

    private void Verify(SonarSyntaxNodeReportingContext context, IEnumerable<SyntaxNode> arguments)
    {
        var args = arguments.ToArray();
        if (args.Length < 2)
        {
            return;
        }
        var multiline = args[0].GetLocation().StartLine() != args.Last().GetLocation().StartLine();
        foreach (var arg in args)
        {
            if (IsOnNewLine(arg) != multiline
                && (arg.ChildNodes().OfType<LambdaExpressionSyntax>().FirstOrDefault() is not { } lambda || !lambda.IsAnalysisContextAction(context.Model)))
            {
                context.ReportIssue(Rule, arg);
            }
        }
    }

    private static bool IsOnNewLine(SyntaxNode node) =>
        node.GetLocation().StartLine() != node.GetFirstToken().GetPreviousToken().GetLocation().StartLine();
}
