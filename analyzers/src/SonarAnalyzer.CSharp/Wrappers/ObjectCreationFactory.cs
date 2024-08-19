/*
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

namespace SonarAnalyzer.Wrappers
{
    internal interface IObjectCreation
    {
        InitializerExpressionSyntax Initializer { get; }
        ArgumentListSyntax ArgumentList { get; }
        ExpressionSyntax Expression { get; }
        IEnumerable<ExpressionSyntax> InitializerExpressions { get; }

        bool IsKnownType(KnownType knownType, SemanticModel semanticModel);
        string TypeAsString(SemanticModel semanticModel);
        ITypeSymbol TypeSymbol(SemanticModel semanticModel);
        IMethodSymbol MethodSymbol(SemanticModel semanticModel);
    }

    internal class ObjectCreationFactory
    {
        public static IObjectCreation Create(SyntaxNode node) =>
            node switch
            {
                null => throw new ArgumentNullException(nameof(node)),
                ObjectCreationExpressionSyntax objectCreation => new ObjectCreation(objectCreation),
                { } when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(node) => new ImplicitObjectCreation((ImplicitObjectCreationExpressionSyntaxWrapper)node),
                _ => throw new InvalidOperationException("Unexpected type: " + node.GetType().Name)
            };

        public static IObjectCreation TryCreate(SyntaxNode node) =>
             node.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression)
                 ? Create(node)
                 : null;

        public static bool TryCreate(SyntaxNode node, out IObjectCreation objectCreation)
        {
            objectCreation = TryCreate(node);
            return objectCreation is not null;
        }

        private sealed class ObjectCreation : IObjectCreation
        {
            private readonly ObjectCreationExpressionSyntax objectCreation;

            public InitializerExpressionSyntax Initializer => objectCreation.Initializer;
            public ArgumentListSyntax ArgumentList => objectCreation.ArgumentList;
            public ExpressionSyntax Expression => objectCreation;
            public IEnumerable<ExpressionSyntax> InitializerExpressions => objectCreation.Initializer?.Expressions;

            public ObjectCreation(ObjectCreationExpressionSyntax objectCreationExpressionSyntax) =>
                objectCreation = objectCreationExpressionSyntax;

            public bool IsKnownType(KnownType knownType, SemanticModel semanticModel) =>
                objectCreation.Type.GetName().EndsWith(knownType.TypeName) && objectCreation.IsKnownType(knownType, semanticModel);

            public string TypeAsString(SemanticModel semanticModel) =>
                objectCreation.Type.ToString();

            public ITypeSymbol TypeSymbol(SemanticModel semanticModel) =>
                semanticModel.GetTypeInfo(objectCreation).Type;

            public IMethodSymbol MethodSymbol(SemanticModel semanticModel) =>
                semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;
        }

        private sealed class ImplicitObjectCreation : IObjectCreation
        {
            private readonly ImplicitObjectCreationExpressionSyntaxWrapper objectCreation;

            public InitializerExpressionSyntax Initializer => objectCreation.Initializer;
            public ArgumentListSyntax ArgumentList => objectCreation.ArgumentList;
            public ExpressionSyntax Expression => objectCreation.SyntaxNode;
            public IEnumerable<ExpressionSyntax> InitializerExpressions => objectCreation.Initializer?.Expressions;

            public ImplicitObjectCreation(ImplicitObjectCreationExpressionSyntaxWrapper wrapper) =>
                objectCreation = wrapper;

            public bool IsKnownType(KnownType knownType, SemanticModel semanticModel) =>
                semanticModel.GetTypeInfo(objectCreation).Type.Is(knownType);

            // Return null if TypeSymbol returns null to avoid AD0001 due to this issue: https://github.com/dotnet/roslyn/issues/70041
            public string TypeAsString(SemanticModel semanticModel) =>
                TypeSymbol(semanticModel)?.Name;

            public ITypeSymbol TypeSymbol(SemanticModel semanticModel) =>
                semanticModel.GetTypeInfo(objectCreation).ConvertedType;

            public IMethodSymbol MethodSymbol(SemanticModel semanticModel) =>
                semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;
        }
    }
}
