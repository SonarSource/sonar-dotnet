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

namespace SonarAnalyzer.Helpers.Trackers;

internal class CSharpArgumentTracker : ArgumentTracker<SyntaxKind>
{
    protected override SyntaxKind[] TrackedSyntaxKinds => new[]
        {
            SyntaxKind.AttributeArgument,
            SyntaxKind.Argument,
        };

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode) =>
        argumentNode switch
        {
            AttributeArgumentSyntax { Parent: AttributeArgumentListSyntax { Arguments: { } list } } => list,
            ArgumentSyntax { Parent: BaseArgumentListSyntax { Arguments: { } list } } => list,
            _ => null,
        };

    protected override int? Position(SyntaxNode argumentNode) =>
        argumentNode is ArgumentSyntax { NameColon: not null } or AttributeArgumentSyntax { NameColon: not null } or AttributeArgumentSyntax { NameEquals: not null }
            ? null
            : ArgumentList(argumentNode).IndexOf(x => x == argumentNode);

    protected override RefKind ArgumentRefKind(SyntaxNode argumentNode)
    {
        return argumentNode switch
        {
            AttributeArgumentSyntax => RefKind.None,
            ArgumentSyntax { RefOrOutKeyword: { } refOrOut } => refOrOut.Kind() switch { SyntaxKind.OutKeyword => RefKind.Out, SyntaxKind.RefKeyword => RefKind.Ref, _ => RefKind.None },
            _ => RefKind.None,
        };
    }

    protected override bool InvocationFitsMemberKind(SyntaxNode argumentNode, InvokedMemberKind memberKind)
    {
        var invocationExpression = InvokedExpression(argumentNode);
        return memberKind switch
        {
            InvokedMemberKind.Method => invocationExpression is InvocationExpressionSyntax,
            InvokedMemberKind.Constructor => invocationExpression is ObjectCreationExpressionSyntax
                or ConstructorInitializerSyntax
                || ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(invocationExpression),
            InvokedMemberKind.Indexer => invocationExpression is ElementAccessExpressionSyntax or ElementBindingExpressionSyntax,
            InvokedMemberKind.Attribute => invocationExpression is AttributeSyntax,
            _ => false,
        };
    }

    protected override bool InvokedMemberFits(SemanticModel model, SyntaxNode argumentNode, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint)
    {
        var expression = InvokedExpression(argumentNode);
        return memberKind switch
        {
            InvokedMemberKind.Method => invokedMemberNameConstraint(expression.GetName()),
            InvokedMemberKind.Constructor => expression switch
            {
                ObjectCreationExpressionSyntax { Type: { } typeName } => invokedMemberNameConstraint(typeName.GetName()),
                ConstructorInitializerSyntax x => FindClassNameFromConstructorInitializerSyntax(x) is string name
                    ? invokedMemberNameConstraint(name)
                    : true,
                { } ex when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(ex) => invokedMemberNameConstraint(model.GetSymbolInfo(ex).Symbol?.ContainingType?.Name),
                _ => false,
            },
            InvokedMemberKind.Indexer => true,
            InvokedMemberKind.Attribute => expression is AttributeSyntax { Name: { } typeName } && invokedMemberNameConstraint(typeName.GetName()),
            _ => false,
        };
    }

    private string FindClassNameFromConstructorInitializerSyntax(ConstructorInitializerSyntax initializerSyntax) =>
        initializerSyntax.ThisOrBaseKeyword.Kind() switch
        {
            SyntaxKind.ThisKeyword => initializerSyntax is { Parent: ConstructorDeclarationSyntax { Identifier.ValueText: { } typeName } } ? typeName : null,
            SyntaxKind.BaseKeyword => initializerSyntax is { Parent: ConstructorDeclarationSyntax { Parent: BaseTypeDeclarationSyntax { BaseList.Types: { Count: > 0 } baseListTypes } } }
                ? baseListTypes[0].GetName() // Get the class name of the called constructor from the base types list of the type declaration
                : null,
            _ => null,
        };

    protected override SyntaxNode InvokedExpression(SyntaxNode argumentNode) =>
        argumentNode?.Parent?.Parent;

    protected override SyntaxBaseContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        new(context);
}
