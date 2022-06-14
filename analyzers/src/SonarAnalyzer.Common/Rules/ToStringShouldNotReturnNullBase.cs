/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules;

public abstract class ToStringShouldNotReturnNullBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2225";

    protected override string MessageFormat => "Return empty string instead.";

    protected ToStringShouldNotReturnNullBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (WithinToString(c.Node) && ReturnsNull(c.Node))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.ReturnStatement);

    private bool ReturnsNull(SyntaxNode node)
    {
        var expression = Language.Syntax.NodeExpression(node);
        return Language.Syntax.IsNullLiteral(expression);
    }

    private bool WithinToString(SyntaxNode node) =>
        node.Ancestors()
        .Select(x => Language.Syntax.NodeIdentifier(x)?.ValueText)
        .Any(x => nameof(ToString).Equals(x, Language.NameComparison));
}
