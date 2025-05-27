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
public sealed class ArrayPassedAsParams : ArrayPassedAsParamsBase<SyntaxKind, CSharpSyntaxNode>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxKind[] ExpressionKinds { get; } =
        [
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.InvocationExpression,
            SyntaxKindEx.ImplicitObjectCreationExpression,
            SyntaxKind.Attribute
        ];

    protected override CSharpSyntaxNode LastArgumentIfArrayCreation(SyntaxNode expression)
    {
        CSharpSyntaxNode lastArgument = expression switch
        {
             AttributeSyntax { ArgumentList.Arguments: { Count: > 0 } args } when args.Last().NameEquals is null => args.Last(),
            _ when ArgumentList(expression) is { Arguments: { Count: > 0 } args } => args.Last(),
            _ => null
        };

        return IsArrayCreation(lastArgument)
            ? lastArgument
            : null;
    }

    protected override ITypeSymbol ArrayElementType(CSharpSyntaxNode argument, SemanticModel model) =>
        ArgumentExpression(argument) switch
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

    private static bool IsArrayCreation(CSharpSyntaxNode argument)
    {
        var expression = ArgumentExpression(argument);
        return expression switch
        {
            ArrayCreationExpressionSyntax { Initializer: not null } => true,
            ImplicitArrayCreationExpressionSyntax => true,
            _ when CollectionExpressionSyntaxWrapper.IsInstance(expression) => !ContainsSpread((CollectionExpressionSyntaxWrapper)expression),
            _ => false
        };
    }

    // [x, y, ..z] is not possible to be passed as params
    private static bool ContainsSpread(CollectionExpressionSyntaxWrapper expression) =>
        expression.Elements.Any(x => x.SyntaxNode.IsKind(SyntaxKindEx.SpreadElement));

    private static ExpressionSyntax ArgumentExpression(CSharpSyntaxNode node) =>
        node switch
        {
            ArgumentSyntax arg => arg.Expression,
            AttributeArgumentSyntax arg => arg.Expression,
            _ => null
        };
}
