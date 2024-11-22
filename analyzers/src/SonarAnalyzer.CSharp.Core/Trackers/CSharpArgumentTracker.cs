/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Trackers;

internal sealed class CSharpArgumentTracker : ArgumentTracker<SyntaxKind>
{
    protected override SyntaxKind[] TrackedSyntaxKinds =>
        [
            SyntaxKind.AttributeArgument,
            SyntaxKind.Argument,
        ];

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode) =>
        argumentNode switch
        {
            AttributeArgumentSyntax { Parent: AttributeArgumentListSyntax { Arguments: { } list } } => list,
            ArgumentSyntax { Parent: BaseArgumentListSyntax { Arguments: { } list } } => list,
            _ => null,
        };

    protected override int? Position(SyntaxNode argumentNode) =>
        argumentNode is ArgumentSyntax { NameColon: not null }
            or AttributeArgumentSyntax { NameColon: not null }
            or AttributeArgumentSyntax { NameEquals: not null }
            ? null
            : ArgumentList(argumentNode).IndexOf(x => x == argumentNode);

    protected override RefKind? ArgumentRefKind(SyntaxNode argumentNode) =>
        argumentNode switch
        {
            ArgumentSyntax { RefOrOutKeyword: { } refOrOut } => refOrOut.Kind() switch
                                                                {
                                                                    SyntaxKind.OutKeyword => RefKind.Out,
                                                                    SyntaxKind.RefKeyword => RefKind.Ref,
                                                                    SyntaxKind.InKeyword => RefKindEx.In,
                                                                    _ => RefKind.None
                                                                },
            AttributeArgumentSyntax => null, // RefKind is not supported for attributes and there is no way to specify such a constraint for Attributes in ArgumentDescriptor.
            _ => null,
        };

    protected override bool InvocationMatchesMemberKind(SyntaxNode invokedExpression, MemberKind memberKind) =>
        memberKind switch
        {
            MemberKind.Method => invokedExpression is InvocationExpressionSyntax,
            MemberKind.Constructor => invokedExpression is ObjectCreationExpressionSyntax
                or ConstructorInitializerSyntax
                || ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(invokedExpression),
            MemberKind.Indexer => invokedExpression is ElementAccessExpressionSyntax or ElementBindingExpressionSyntax,
            MemberKind.Attribute => invokedExpression is AttributeSyntax,
            _ => false,
        };

    protected override bool InvokedMemberMatches(SemanticModel model, SyntaxNode invokedExpression, MemberKind memberKind, Func<string, bool> invokedMemberNameConstraint) =>
        memberKind switch
        {
            MemberKind.Method => invokedMemberNameConstraint(invokedExpression.GetName()),
            MemberKind.Constructor => invokedExpression switch
            {
                ObjectCreationExpressionSyntax { Type: { } typeName } => invokedMemberNameConstraint(typeName.GetName()),
                ConstructorInitializerSyntax x => FindClassNameFromConstructorInitializerSyntax(x) is not string name || invokedMemberNameConstraint(name),
                { } x when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(x) => invokedMemberNameConstraint(model.GetSymbolInfo(x).Symbol?.ContainingType?.Name),
                _ => false,
            },
            MemberKind.Indexer => invokedExpression switch
            {
                ElementAccessExpressionSyntax { Expression: { } accessedExpression } => invokedMemberNameConstraint(accessedExpression.GetName()),
                ElementBindingExpressionSyntax binding => binding.GetParentConditionalAccessExpression() is { Expression: { } accessedExpression }
                                                          && invokedMemberNameConstraint(accessedExpression.GetName()),
                _ => false,
            },
            MemberKind.Attribute => invokedExpression is AttributeSyntax { Name: { } typeName } && invokedMemberNameConstraint(typeName.GetName()),
            _ => false,
        };

    private static string FindClassNameFromConstructorInitializerSyntax(ConstructorInitializerSyntax initializerSyntax) =>
        initializerSyntax.ThisOrBaseKeyword.Kind() switch
        {
            SyntaxKind.ThisKeyword => initializerSyntax is { Parent: ConstructorDeclarationSyntax { Identifier.ValueText: { } typeName } } ? typeName : null,
            SyntaxKind.BaseKeyword => initializerSyntax is { Parent: ConstructorDeclarationSyntax { Parent: BaseTypeDeclarationSyntax { BaseList.Types: { Count: > 0 } baseListTypes } } }
                ? baseListTypes.First().GetName() // Get the class name of the called constructor from the base types list of the type declaration
                : null,
            _ => null,
        };
}
