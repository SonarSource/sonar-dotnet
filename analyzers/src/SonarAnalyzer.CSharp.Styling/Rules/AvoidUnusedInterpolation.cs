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
public sealed class AvoidUnusedInterpolation : StylingAnalyzer
{
    private const string ReduceMessage = "Reduce the number of $ in this string.";
    private const string RemoveMessage = "Remove unused interpolation from this string.";

    public AvoidUnusedInterpolation() : base("T0040", "{0}") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var interpolated = (InterpolatedStringExpressionSyntax)c.Node;
                if (interpolated.Contents.OfType<InterpolationSyntax>().IsEmpty())
                {
                    c.ReportIssue(Rule, interpolated.StringStartToken, RemoveMessage);
                }
                else if (DollarCount(interpolated) > CurlyBraceCount(interpolated) + 1)
                {
                    c.ReportIssue(Rule, interpolated.StringStartToken, ReduceMessage);
                }
            }, SyntaxKind.InterpolatedStringExpression);

    private static int DollarCount(InterpolatedStringExpressionSyntax interpolated) =>
        interpolated.StringStartToken.ValueText.Count(x => x == '$');

    private static int CurlyBraceCount(InterpolatedStringExpressionSyntax interpolated)
    {
        var max = 0;
        foreach (var text in interpolated.Contents.OfType<InterpolatedStringTextSyntax>())
        {
            max = Math.Max(max, CurlyBraceCount(text.TextToken.ValueText));
        }
        return max;
    }

    private static int CurlyBraceCount(string text)
    {
        var max = 0;
        var i = 0;
        while (i < text.Length)
        {
            max = Math.Max(max, Count('{'));
            max = Math.Max(max, Count('}'));
            if (i < text.Length && text[i] != '{')
            {
                i++;
            }
        }
        return max;

        int Count(char c)
        {
            var count = 0;
            while (i < text.Length && text[i] == c)
            {
                count++;
                i++;
            }
            return count;
        }
    }
}
