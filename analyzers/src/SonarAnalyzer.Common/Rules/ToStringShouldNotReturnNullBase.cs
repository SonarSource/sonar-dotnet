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

namespace SonarAnalyzer.Rules;

public abstract class ToStringShouldNotReturnNullBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2225";

    protected override string MessageFormat => "Return an empty string instead.";

    protected abstract TSyntaxKind MethodKind { get; }

    protected abstract bool IsLocalOrLambda(SyntaxNode node);

    protected abstract IEnumerable<SyntaxNode> Conditionals(SyntaxNode expression);

    protected ToStringShouldNotReturnNullBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => ToStringReturnsNull(c, c.Node),
            Language.SyntaxKind.ReturnStatement);

    protected void ToStringReturnsNull(SonarSyntaxNodeReportingContext context, SyntaxNode node)
    {
        if (node is not null && ReturnsNull(Language.Syntax.NodeExpression(node)) && WithinToString(node))
        {
            context.ReportIssue(CreateDiagnostic(Rule, node.GetLocation()));
        }
    }

    private bool ReturnsNull(SyntaxNode node) =>
        Language.Syntax.IsNullLiteral(node)
        || Conditionals(node).Select(Language.Syntax.RemoveParentheses).Any(ReturnsNull);

    private bool WithinToString(SyntaxNode node) =>
        node.Ancestors()
            .TakeWhile(x => !IsLocalOrLambda(x))
            .Any(x => Language.Syntax.IsKind(x, MethodKind)
                && nameof(ToString).Equals(Language.Syntax.NodeIdentifier(x)?.ValueText, Language.NameComparison)
                && !Language.Syntax.IsStatic(x));
}
