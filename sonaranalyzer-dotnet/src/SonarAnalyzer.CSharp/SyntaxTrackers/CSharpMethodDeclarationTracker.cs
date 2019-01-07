/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Helpers
{
    public class CSharpMethodDeclarationTracker : MethodDeclarationTracker<SyntaxKind>
    {
        public CSharpMethodDeclarationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
        }

        protected override SyntaxKind[] TrackedSyntaxKinds =>
            throw new NotSupportedException("MethodDeclarationTracker uses symbols, not syntax");

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            CSharp.CSharpGeneratedCodeRecognizer.Instance;

        public override MethodDeclarationCondition ParameterAtIndexIsUsed(int index) =>
            (context) =>
            {
                var parameterSymbol = context.MethodSymbol.Parameters.ElementAtOrDefault(0);
                if (parameterSymbol == null)
                {
                    return false;
                }

                var methodDeclaration = context.MethodSymbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<BaseMethodDeclarationSyntax>()
                    .FirstOrDefault(declaration => declaration.HasBodyOrExpressionBody());

                if (methodDeclaration == null)
                {
                    return false;
                }

                var semanticModel = context.GetSemanticModel(methodDeclaration);

                var descendantNodes = methodDeclaration?.Body?.DescendantNodes()
                    ?? methodDeclaration?.ExpressionBody()?.DescendantNodes()
                    ?? Enumerable.Empty<SyntaxNode>();

                return descendantNodes.Any(
                    node =>
                    {
                        return node.IsKind(SyntaxKind.IdentifierName) &&
                            ((IdentifierNameSyntax)node).Identifier.ValueText == parameterSymbol.Name &&
                            parameterSymbol.Equals(semanticModel.GetSymbolInfo(node).Symbol);
                    });
            };

        protected override SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration)
        {
            switch (methodDeclaration?.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)methodDeclaration).Identifier;
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)methodDeclaration).Identifier;
                case SyntaxKind.DestructorDeclaration:
                    return ((DestructorDeclarationSyntax)methodDeclaration).Identifier;
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                    return ((EventDeclarationSyntax)methodDeclaration.Parent.Parent).Identifier;
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    return ((PropertyDeclarationSyntax)methodDeclaration.Parent.Parent).Identifier;
                case SyntaxKind.OperatorDeclaration:
                    return ((OperatorDeclarationSyntax)methodDeclaration).OperatorToken;
                default:
                    return null;
            }
        }
    }
}
