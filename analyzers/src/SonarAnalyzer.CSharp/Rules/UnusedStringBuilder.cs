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
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, IdentifierNameSyntax, ConditionalAccessExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override ILocalSymbol GetSymbol(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) => (ILocalSymbol)semanticModel.GetDeclaredSymbol(declaration);

    protected override string GetName(SyntaxNode declaration) => declaration.GetName();

    protected override SyntaxNode GetScope(VariableDeclaratorSyntax declarator) =>
        declarator.IsTopLevel()
        ? declarator.Parent.Parent.Parent.Parent
        : declarator.Parent.Parent.Parent;

    protected override bool NeedsToTrack(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) =>
        declaration is
        {
            Parent.Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: { } expression,
        }
        && IsStringBuilderObjectCreation(expression, semanticModel);

    protected override bool IsStringBuilderRead(string name, ILocalSymbol symbol, SyntaxNode node, SemanticModel model) =>
        node switch
        {
            InvocationExpressionSyntax invocation =>
                (StringBuilderAccessInvocations.Contains(invocation.Expression.GetName()) && IsSameReference(invocation.Expression, name, symbol, model))
                || invocation.ArgumentList.Arguments.Any(argument => IsSameReference(argument.Expression, name, symbol, model)),
            ReturnStatementSyntax returnStatement => IsSameReference(returnStatement.Expression, name, symbol, model),
            InterpolationSyntax interpolation => IsSameReference(interpolation.Expression, name, symbol, model),
            ElementAccessExpressionSyntax elementAccess => IsSameReference(elementAccess.Expression, name, symbol, model),
            MemberAccessExpressionSyntax memberAccess => StringBuilderAccessExpressions.Contains(memberAccess.Name.GetName()) && IsSameReference(memberAccess.Expression, name, symbol, model),
            _ => false,
        };

    protected override bool DescendIntoChildren(SyntaxNode node) => true;

    private static bool IsStringBuilderObjectCreation(ExpressionSyntax expression, SemanticModel semanticModel) =>
        expression.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression)
        && ObjectCreationFactory.Create(expression).IsKnownType(KnownType.System_Text_StringBuilder, semanticModel);
}
