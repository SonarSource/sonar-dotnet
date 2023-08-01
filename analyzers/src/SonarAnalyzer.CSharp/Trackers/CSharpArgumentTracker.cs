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

    protected override RefKind? ArgumentRefKind(SyntaxNode argumentNode) =>
        argumentNode switch
        {
            AttributeArgumentSyntax => null,
            ArgumentSyntax { RefOrOutKeyword: { } refOrOut } => refOrOut.Kind() switch { SyntaxKind.OutKeyword => RefKind.Out, SyntaxKind.RefKeyword => RefKind.Ref, _ => RefKind.None },
            _ => null,
        };

    protected override bool InvocationFitsMemberKind(SyntaxNode invokedExpression, InvokedMemberKind memberKind) =>
        memberKind switch
        {
            InvokedMemberKind.Method => invokedExpression is InvocationExpressionSyntax,
            InvokedMemberKind.Constructor => invokedExpression is ObjectCreationExpressionSyntax
                or ConstructorInitializerSyntax
                || ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(invokedExpression),
            InvokedMemberKind.Indexer => invokedExpression is ElementAccessExpressionSyntax or ElementBindingExpressionSyntax,
            InvokedMemberKind.Attribute => invokedExpression is AttributeSyntax,
            _ => false,
        };

    protected override bool InvokedMemberFits(SemanticModel model, SyntaxNode invokedExpression, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint) =>
        memberKind switch
        {
            InvokedMemberKind.Method => invokedMemberNameConstraint(invokedExpression.GetName()),
            InvokedMemberKind.Constructor => invokedExpression switch
            {
                ObjectCreationExpressionSyntax { Type: { } typeName } => invokedMemberNameConstraint(typeName.GetName()),
                ConstructorInitializerSyntax x => FindClassNameFromConstructorInitializerSyntax(x) is not string name || invokedMemberNameConstraint(name),
                { } ex when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(ex) => invokedMemberNameConstraint(model.GetSymbolInfo(ex).Symbol?.ContainingType?.Name),
                _ => false,
            },
            InvokedMemberKind.Indexer => invokedExpression switch
            {
                ElementAccessExpressionSyntax { Expression: { } accessedExpression } => invokedMemberNameConstraint(accessedExpression.GetName()),
                ElementBindingExpressionSyntax { } binding => binding.GetParentConditionalAccessExpression() is
                { Expression: { } accessedExpression } && invokedMemberNameConstraint(accessedExpression.GetName()),
                _ => false,
            },
            InvokedMemberKind.Attribute => invokedExpression is AttributeSyntax { Name: { } typeName } && invokedMemberNameConstraint(typeName.GetName()),
            _ => false,
        };

    private string FindClassNameFromConstructorInitializerSyntax(ConstructorInitializerSyntax initializerSyntax) =>
        initializerSyntax.ThisOrBaseKeyword.Kind() switch
        {
            SyntaxKind.ThisKeyword => initializerSyntax is { Parent: ConstructorDeclarationSyntax { Identifier.ValueText: { } typeName } } ? typeName : null,
            SyntaxKind.BaseKeyword => initializerSyntax is { Parent: ConstructorDeclarationSyntax { Parent: BaseTypeDeclarationSyntax { BaseList.Types: { Count: > 0 } baseListTypes } } }
                ? baseListTypes.First().GetName() // Get the class name of the called constructor from the base types list of the type declaration
                : null,
            _ => null,
        };

    protected override ArgumentContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        new(context);
}
