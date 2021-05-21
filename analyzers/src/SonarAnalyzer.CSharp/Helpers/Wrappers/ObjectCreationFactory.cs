/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Helpers.Wrappers
{
    public interface IObjectCreation
    {
        InitializerExpressionSyntax Initializer { get; }
        ArgumentListSyntax ArgumentList { get; }
        ExpressionSyntax Expression { get; }
        string GetTypeAsString(SemanticModel semanticModel);
    }

    public class ObjectCreationFactory
    {
        public static IObjectCreation Create(SyntaxNode node) =>
            node is ObjectCreationExpressionSyntax objectCreationExpressionSyntax
                ? (IObjectCreation)new ObjectCreation(objectCreationExpressionSyntax)
                : new ImplicitObjectCreation((ImplicitObjectCreationExpressionSyntaxWrapper)node);

        private class ObjectCreation : IObjectCreation
        {
            private readonly ObjectCreationExpressionSyntax objectCreation;
            public InitializerExpressionSyntax Initializer => objectCreation.Initializer;
            public ArgumentListSyntax ArgumentList => objectCreation.ArgumentList;
            public ExpressionSyntax Expression => objectCreation;

            public ObjectCreation(ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                objectCreation = objectCreationExpressionSyntax;
            }

            public string GetTypeAsString(SemanticModel semanticModel) =>
                objectCreation.Type.ToString();
        }

        private class ImplicitObjectCreation : IObjectCreation
        {
            private readonly ImplicitObjectCreationExpressionSyntaxWrapper objectCreation;
            public InitializerExpressionSyntax Initializer => objectCreation.Initializer;
            public ArgumentListSyntax ArgumentList => objectCreation.ArgumentList;
            public ExpressionSyntax Expression => objectCreation.SyntaxNode;

            public ImplicitObjectCreation(ImplicitObjectCreationExpressionSyntaxWrapper wrapper)
            {
                objectCreation = wrapper;
            }

            public string GetTypeAsString(SemanticModel semanticModel) =>
                semanticModel.GetTypeInfo(objectCreation).Type.Name;
        }
    }
}
