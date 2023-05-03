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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFind : UseFindBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool TryGetAccessInfo(SyntaxNode node, out AccessInfo accessInfo)
    {
        var (syntaxNode, left, right) = node switch
        {
            ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax inv } condAccess => ((SyntaxNode)inv, condAccess.Expression, inv.Expression),
            MemberAccessExpressionSyntax memberAccess => (memberAccess, memberAccess.Expression, memberAccess.Name),
            _ => (null, null, null)
        };

        if (syntaxNode is null)
        {
            return false;
        }

        accessInfo.Node = syntaxNode;
        accessInfo.LeftExpression = left;
        accessInfo.RightExpression = right;

        return true;
    }

    protected override bool IsInvocationNamedFirstOrDefault(SyntaxNode syntaxNode) => syntaxNode.NameIs(nameof(Enumerable.FirstOrDefault));
    protected override Location GetIssueLocation(SyntaxNode syntaxNode) => syntaxNode.GetIdentifier()?.GetLocation();
}
