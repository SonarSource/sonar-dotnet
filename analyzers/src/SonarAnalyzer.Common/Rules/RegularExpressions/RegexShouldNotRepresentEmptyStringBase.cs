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

using SonarAnalyzer.RegularExpressions;

namespace SonarAnalyzer.Rules;

public abstract class RegexShouldNotRepresentEmptyStringBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S101"; //"S5842";

    protected sealed override string MessageFormat => "The regular expression should not match an empty string.";

    protected RegexShouldNotRepresentEmptyStringBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c => Analyze(c, RegexNode.FromCtor(c.Node, c.SemanticModel, Language)),
            Language.SyntaxKind.ObjectCreationExpressions);

        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c => Analyze(c, RegexNode.FromMethod(c.Node, c.SemanticModel, Language)),
            Language.SyntaxKind.InvocationExpression);

        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c => Analyze(c, RegexNode.FromAttribute(c.Node, c.SemanticModel, Language)),
            Language.SyntaxKind.Attribute);
    }

    private void Analyze(SyntaxNodeAnalysisContext c, RegexNode regex)
    {
        if (regex is { }
            && regex.Pattern.Value is { } pattern
            && RegexTree.Parse(pattern, regex.Options.Value).MatchesEmptyString())
        {
            c.ReportIssue(Diagnostic.Create(Rule, regex.Pattern.Node.GetLocation()));
        }
    }
}
