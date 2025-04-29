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

namespace SonarAnalyzer.CSharp.Rules;

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
        ArgumentList(expression) is { Arguments: { Count: > 0 } arguments }
        && arguments.Last() is var lastArgument
        && IsArrayCreation(lastArgument.Expression)
            ? lastArgument
            : null;

    protected override ITypeSymbol ArrayElementType(ArgumentSyntax argument, SemanticModel model) =>
        argument.Expression switch
        {
            ArrayCreationExpressionSyntax arrayCreation => model.GetTypeInfo(arrayCreation.Type.ElementType).Type,
            ImplicitArrayCreationExpressionSyntax implicitArrayCreation => (model.GetTypeInfo(implicitArrayCreation).Type as IArrayTypeSymbol)?.ElementType,
            _ => null
        };

    private static BaseArgumentListSyntax ArgumentList(SyntaxNode expression) =>
        expression switch
        {
            ObjectCreationExpressionSyntax { } creation => creation.ArgumentList,
            InvocationExpressionSyntax { } invocation => invocation.ArgumentList,
            _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(expression) =>
                ((ImplicitObjectCreationExpressionSyntaxWrapper)expression).ArgumentList,
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
