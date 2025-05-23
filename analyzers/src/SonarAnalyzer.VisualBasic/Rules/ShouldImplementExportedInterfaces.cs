﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ShouldImplementExportedInterfaces : ShouldImplementExportedInterfacesBase<ArgumentSyntax, AttributeSyntax, SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SeparatedSyntaxList<ArgumentSyntax>? GetAttributeArguments(AttributeSyntax attributeSyntax) =>
            attributeSyntax.ArgumentList?.Arguments;

        protected override SyntaxNode GetAttributeName(AttributeSyntax attributeSyntax) =>
            attributeSyntax.Name;

        protected override bool IsClassOrRecordSyntax(SyntaxNode syntaxNode) =>
            syntaxNode.IsKind(SyntaxKind.ClassStatement);

        protected override SyntaxNode GetTypeOfOrGetTypeExpression(SyntaxNode expressionSyntax) =>
            (expressionSyntax as GetTypeExpressionSyntax)?.Type;
    }
}
