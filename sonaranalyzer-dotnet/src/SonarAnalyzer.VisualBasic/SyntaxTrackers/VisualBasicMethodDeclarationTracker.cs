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
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public class VisualBasicMethodDeclarationTracker : MethodDeclarationTracker<SyntaxKind>
    {
        public VisualBasicMethodDeclarationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
        }

        protected override SyntaxKind[] TrackedSyntaxKinds =>
            throw new NotSupportedException("MethodDeclarationTracker uses symbols, not syntax");

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;

        public override MethodDeclarationCondition ParameterAtIndexIsUsed(int index) =>
            (context) =>
            {
                var parameterSymbol = context.MethodSymbol.Parameters.ElementAtOrDefault(0);
                if (parameterSymbol == null)
                {
                    return false;
                }

                var methodDeclaration = context.MethodSymbol.DeclaringSyntaxReferences
                    .Select(r => (MethodBlockSyntax)r.GetSyntax().Parent)
                    .FirstOrDefault(HasImplementation);

                if (methodDeclaration == null)
                {
                    return false;
                }

                var semanticModel = context.GetSemanticModel(methodDeclaration);

                var descendantNodes = methodDeclaration.Statements
                    .SelectMany(statement => statement.DescendantNodes());

                return descendantNodes.Any(
                    node =>
                    {
                        return node.IsKind(SyntaxKind.IdentifierName) &&
                            ((IdentifierNameSyntax)node).Identifier.ValueText == parameterSymbol.Name &&
                            parameterSymbol.Equals(semanticModel.GetSymbolInfo(node).Symbol);
                    });
            };

        private static bool HasImplementation(MethodBlockSyntax methodBlock) =>
            methodBlock.Statements.Count > 0;

        protected override SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration)
        {
            switch (methodDeclaration?.Kind())
            {
                case SyntaxKind.SubStatement:
                case SyntaxKind.FunctionStatement:
                    return ((MethodStatementSyntax)methodDeclaration).Identifier;
                case SyntaxKind.SubNewStatement:
                    return ((SubNewStatementSyntax)methodDeclaration).NewKeyword;
                case SyntaxKind.AddHandlerAccessorBlock:
                case SyntaxKind.RemoveHandlerAccessorBlock:
                    return ((EventBlockSyntax)methodDeclaration.Parent.Parent).EventStatement?.Identifier;
                case SyntaxKind.GetAccessorBlock:
                case SyntaxKind.SetAccessorBlock:
                    return ((PropertyBlockSyntax)methodDeclaration.Parent.Parent).PropertyStatement?.Identifier;
                case SyntaxKind.OperatorStatement:
                    return ((OperatorStatementSyntax)methodDeclaration).OperatorToken;
                default:
                    return null;
            }
        }
    }
}
