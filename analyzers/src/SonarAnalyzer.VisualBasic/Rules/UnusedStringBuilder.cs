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

using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, IdentifierNameSyntax, ConditionalAccessExpressionSyntax>
{
    private static readonly SyntaxKind[] SkipChildren = { };

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override string GetName(SyntaxNode declaration) =>
        declaration is VariableDeclaratorSyntax variableDeclarator
            ? variableDeclarator.Names[0].Identifier.ValueText ?? string.Empty
            : declaration.GetName();

    protected override SyntaxNode GetScope(VariableDeclaratorSyntax declarator) => declarator.Parent.Parent.Parent;

    protected override ILocalSymbol RetrieveStringBuilderObject(VariableDeclaratorSyntax declarator, SemanticModel semanticModel) =>
        declarator is
        {
            Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: ObjectCreationExpressionSyntax { } objectCreation,
        }
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel)
            ? semanticModel.GetDeclaredSymbol(declarator.Names.First()) as ILocalSymbol
            : null;

    protected override bool IsStringBuilderRead(string name, ILocalSymbol symbol, SyntaxNode node, SemanticModel model) =>
        node switch
        {
            InvocationExpressionSyntax invocation =>
                (StringBuilderAccessInvocations.Contains(invocation.GetName()) && IsSameReference(invocation.Expression, name, symbol, model))
                || (IsSameReference(invocation.Expression, name, symbol, model) && model.GetOperation(invocation).Kind is OperationKindEx.PropertyReference),
            ReturnStatementSyntax returnStatement => IsSameReference(returnStatement.Expression, name, symbol, model),
            InterpolationSyntax interpolation => IsSameReference(interpolation.Expression, name, symbol, model),
            ArgumentSyntax argument => IsSameReference(argument.GetExpression(), name, symbol, model),
            MemberAccessExpressionSyntax memberAccess => StringBuilderAccessExpressions.Contains(memberAccess.Name.GetName()) && IsSameReference(memberAccess.Expression, name, symbol, model),
            _ => false,
        };

    protected override bool DescendIntoChildren(SyntaxNode node) => !node.IsAnyKind(SkipChildren);
}
