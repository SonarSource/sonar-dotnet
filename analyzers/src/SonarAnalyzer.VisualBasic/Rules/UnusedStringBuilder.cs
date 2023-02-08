﻿/*
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
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override ILocalSymbol GetSymbol(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) => (ILocalSymbol)semanticModel.GetDeclaredSymbol(declaration.Names.First());

    protected override bool NeedsToTrack(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) =>
        declaration is
        {
            Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: ObjectCreationExpressionSyntax { } objectCreation,
        }
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel);

    protected override SyntaxNode GetScope(VariableDeclaratorSyntax declarator) =>
        declarator.Parent.Parent.Parent;

    protected override bool DescendIntoChildren(SyntaxNode node) => true;

    protected override bool IsStringBuilderRead(SemanticModel model, ILocalSymbol local, SyntaxNode node) =>
        node switch
        {
            InvocationExpressionSyntax invocation =>
                (StringBuilderAccessInvocations.Contains(invocation.GetName()) && IsSameReference(invocation.Expression, local, model))
                || invocation.ArgumentList.Arguments.Any(argument => IsSameReference(argument.GetExpression(), local, model))
                || (IsSameReference(invocation.Expression, local, model) && model.GetOperation(invocation).Kind is OperationKindEx.PropertyReference), // Property reference
            ReturnStatementSyntax returnStatement => IsSameReference(returnStatement.Expression, local, model),
            InterpolationSyntax interpolation => IsSameReference(interpolation.Expression, local, model),
            MemberAccessExpressionSyntax memberAccess => StringBuilderAccessExpressions.Contains(memberAccess.Name.GetName()) && IsSameReference(memberAccess.Expression, local, model),
            _ => false,
        };
}
