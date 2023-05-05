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
public sealed class UseIndexingInsteadOfLinqMethods : UseIndexingInsteadOfLinqMethodsBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override int GetArgumentCount(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments.Count;

    protected override SyntaxToken? GetIdentifier(InvocationExpressionSyntax invocation) =>
        invocation.GetIdentifier();

    protected override bool TryGetOperands(InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right) =>
        invocation.TryGetOperands(out left, out right);
}

public static class InvocationExtensions // TO BE DELETED
{
    public static bool TryGetOperands(this InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax access)
        {
            left = access.Expression;
            right = access.Name;
            return true;
        }
        else if (invocation.Expression is MemberBindingExpressionSyntax binding)
        {
            left = invocation.GetParentConditionalAccessExpression().Expression;
            right = binding.Name;
            return true;
        }

        left = right = null;
        return false;
    }
}
