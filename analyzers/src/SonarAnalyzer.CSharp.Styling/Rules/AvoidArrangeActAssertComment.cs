/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidArrangeActAssertComment : StylingAnalyzer
{
    private static readonly Regex WordRegex = new(@"\b(\w+)\b", RegexOptions.None, Constants.DefaultRegexTimeout);
    private static readonly HashSet<string> ForbiddenComments = ["Arrange", "Act", "Assert"];

    public AvoidArrangeActAssertComment() : base("T0044", "Remove this Arrange, Act, Assert comment.", SourceScope.Tests) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var comments = c.Node
                    .DescendantTrivia()
                    .Where(x => x.IsComment() && !ExtractWords(x).Except(ForbiddenComments, StringComparer.OrdinalIgnoreCase).Any());

                foreach (var comment in comments)
                {
                    c.ReportIssue(Rule, comment.GetLocation());
                }
            },
            SyntaxKind.MethodDeclaration);

    private static string[] ExtractWords(SyntaxTrivia comment) =>
        WordRegex.SafeMatches(comment.ToString())
            .Cast<Match>()
            .Where(x => x.Success)
            .Select(x => x.Groups[1].Value)
            .ToArray();
}
