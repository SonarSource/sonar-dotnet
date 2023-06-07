/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Text.RegularExpressions;
using SonarAnalyzer.RegularExpressions;

namespace SonarAnalyzer.Rules;

public abstract class RegexShouldNotContainMultipleSpacesBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6326";

    protected sealed override string MessageFormat => "Regular expressions should not contain multiple spaces.";

    protected RegexShouldNotContainMultipleSpacesBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => Analyze(c, RegexContext.FromCtor(Language, c.SemanticModel, c.Node)),
            Language.SyntaxKind.ObjectCreationExpressions);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => Analyze(c, RegexContext.FromMethod(Language, c.SemanticModel, c.Node)),
            Language.SyntaxKind.InvocationExpression);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => Analyze(c, RegexContext.FromAttribute(Language, c.SemanticModel, c.Node)),
            Language.SyntaxKind.Attribute);
    }

    private void Analyze(SonarSyntaxNodeReportingContext c, RegexContext context)
    {
        if (context?.Regex is { }
            && !IgnoresPatternWhitespace(context)
            && context.Pattern.Contains("  "))
        {
            c.ReportIssue(Diagnostic.Create(Rule, context.PatternNode.GetLocation()));
        }
    }

    private bool IgnoresPatternWhitespace(RegexContext context) =>
        context.Options is { } options
        && options.HasFlag(RegexOptions.IgnorePatternWhitespace);
}
