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

namespace SonarAnalyzer.Helpers
{
    public class CSharpBuilderPatternCondition : BuilderPatternCondition<SyntaxKind, InvocationExpressionSyntax>
    {
        public CSharpBuilderPatternCondition(bool constructorIsSafe, params BuilderPatternDescriptor<SyntaxKind, InvocationExpressionSyntax>[] descriptors)
            : base(constructorIsSafe, descriptors, new CSharpAssignmentFinder()) { }

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxNode GetExpression(InvocationExpressionSyntax node) =>
            node.Expression;

        protected override string GetIdentifierName(InvocationExpressionSyntax node) =>
            node.Expression.GetName();

        protected override bool IsMemberAccess(SyntaxNode node, out SyntaxNode memberAccessExpression)
        {
            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                memberAccessExpression = memberAccess.Expression;
                return true;
            }
            memberAccessExpression = null;
            return false;
        }

        protected override bool IsObjectCreation(SyntaxNode node) =>
            node.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

        protected override bool IsIdentifier(SyntaxNode node, out string identifierName)
        {
            if (node is IdentifierNameSyntax identifier)
            {
                identifierName = identifier.Identifier.ValueText;
                return true;
            }
            identifierName = null;
            return false;
        }
    }
}
