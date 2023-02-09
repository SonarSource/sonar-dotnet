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
    private static readonly SyntaxKind[] SkipChildren =
    {
        SyntaxKind.ClassDeclaration,
    };

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override string GetName(SyntaxNode declaration) => declaration.GetName();

    protected override SyntaxNode GetScope(VariableDeclaratorSyntax declarator) =>
        declarator.Parent.Parent is { Parent: GlobalStatementSyntax { Parent: CompilationUnitSyntax } }
        ? declarator.Parent.Parent.Parent.Parent
        : declarator.Parent.Parent.Parent;

    protected override ILocalSymbol RetrieveStringBuilderObject(VariableDeclaratorSyntax declarator, SemanticModel semanticModel) =>
        declarator is
        {
            Parent.Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: { } expression,
        }
        && IsStringBuilderObjectCreation(expression, semanticModel)
            ? semanticModel.GetDeclaredSymbol(declarator) as ILocalSymbol
            : null;

    protected override bool IsStringBuilderRead(string name, ILocalSymbol symbol, SyntaxNode node, SemanticModel model) =>
        node switch
        {
            InvocationExpressionSyntax invocation => StringBuilderAccessInvocations.Contains(invocation.Expression.GetName()) && IsSameReference(invocation.Expression, name, symbol, model),
            ReturnStatementSyntax returnStatement => IsSameReference(returnStatement.Expression, name, symbol, model),
            InterpolationSyntax interpolation => IsSameReference(interpolation.Expression, name, symbol, model),
            ElementAccessExpressionSyntax elementAccess => IsSameReference(elementAccess.Expression, name, symbol, model),
            ArgumentSyntax argument => IsSameReference(argument.Expression, name, symbol, model),
            MemberAccessExpressionSyntax memberAccess => StringBuilderAccessExpressions.Contains(memberAccess.Name.GetName()) && IsSameReference(memberAccess.Expression, name, symbol, model),
            _ => false,
        };

    protected override bool DescendIntoChildren(SyntaxNode node) =>
        !(node.IsAnyKind(SkipChildren) && node.IsKind(SyntaxKindEx.LocalFunctionStatement) && ((LocalFunctionStatementSyntaxWrapper)node).Modifiers.Any(SyntaxKind.StaticKeyword));

    private static bool IsStringBuilderObjectCreation(ExpressionSyntax expression, SemanticModel semanticModel) =>
        ObjectCreationFactory.TryCreate(expression) is { } creation
        && creation.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel);
}
