/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
public sealed class ArrayPassedAsParams : ArrayPassedAsParamsBase<SyntaxKind, ArgumentSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxKind[] ExpressionKinds { get; } =
        [
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.InvocationExpression,
            SyntaxKindEx.ImplicitObjectCreationExpression
        ];

    protected override ArgumentSyntax LastArgumentIfArrayCreation(SyntaxNode expression) =>
        LastArgumentIfArrayCreation(ArgumentList(expression));

    private static ArgumentSyntax LastArgumentIfArrayCreation(BaseArgumentListSyntax argumentList) =>
        argumentList is { Arguments: { Count: > 0 } arguments }
        && arguments.Last() is var lastArgument
        && IsArrayCreation(lastArgument.Expression)
            ? lastArgument
            : null;

    private static BaseArgumentListSyntax ArgumentList(SyntaxNode expression) =>
        expression switch
        {
            ObjectCreationExpressionSyntax { } creation => creation.ArgumentList,
            InvocationExpressionSyntax { } invocation => invocation.ArgumentList,
            _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(expression) => ((ImplicitObjectCreationExpressionSyntaxWrapper)expression).ArgumentList,
            _ => null
        };

    private static bool IsArrayCreation(ExpressionSyntax expression) =>
        expression switch
        {
            ArrayCreationExpressionSyntax { Initializer: not null } => true,
            ImplicitArrayCreationExpressionSyntax => true,
            _ when CollectionExpressionSyntaxWrapper.IsInstance(expression) => !ContainsSpread((CollectionExpressionSyntaxWrapper)expression),
            _ => false
        };

    // [x, y, ..z] is not possible to be passed as params
    private static bool ContainsSpread(CollectionExpressionSyntaxWrapper expression) =>
        expression.Elements.Any(x => x.SyntaxNode.IsKind(SyntaxKindEx.SpreadElement));
}
