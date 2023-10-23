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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParametersCorrectOrder : ParametersCorrectOrderBase<SyntaxKind>
    {
        protected override SyntaxKind[] InvocationKinds => new[]
            {
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ThisConstructorInitializer,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKindEx.PrimaryConstructorBaseType,
                SyntaxKindEx.ImplicitObjectCreationExpression,
            };

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override Location PrimaryLocation(SyntaxNode node) =>
            node switch
            {
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: { } name } } => name.GetLocation(),
                InvocationExpressionSyntax {  Expression: { } expression } => expression.GetLocation(),
                ObjectCreationExpressionSyntax { Type: QualifiedNameSyntax { Right: { } right } } => right.GetLocation(),
                ObjectCreationExpressionSyntax { Type: { } type } => type.GetLocation(),
                ConstructorInitializerSyntax { ThisOrBaseKeyword: { } keyword } => keyword.GetLocation(),
                _ when PrimaryConstructorBaseTypeSyntaxWrapper.IsInstance(node) && ((PrimaryConstructorBaseTypeSyntaxWrapper)node).Type is { } type =>
                    type is QualifiedNameSyntax { Right: { } right } ? right.GetLocation() : type.GetLocation(),
                _ => base.PrimaryLocation(node),
            };
    }
}
