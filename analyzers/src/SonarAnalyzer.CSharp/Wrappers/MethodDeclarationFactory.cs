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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Wrappers
{
    internal class MethodDeclarationFactory
    {
        public static IMethodDeclaration Create(SyntaxNode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            else if (LocalFunctionStatementSyntaxWrapper.IsInstance(node))
            {
                return new LocalFunctionStatementAdapter((LocalFunctionStatementSyntaxWrapper)node);
            }
            else if (node is MethodDeclarationSyntax method)
            {
                return new MethodDeclarationSyntaxAdapter(method);
            }
            else
            {
                throw new InvalidOperationException("Unexpected type: " + node.GetType().Name);
            }
        }

        private class LocalFunctionStatementAdapter : IMethodDeclaration
        {
            private readonly LocalFunctionStatementSyntaxWrapper syntaxWrapper;

            public LocalFunctionStatementAdapter(LocalFunctionStatementSyntaxWrapper syntaxWrapper)
                => this.syntaxWrapper = syntaxWrapper;

            public BlockSyntax Body => syntaxWrapper.Body;

            public ArrowExpressionClauseSyntax ExpressionBody => syntaxWrapper.ExpressionBody;

            public SyntaxToken Identifier => syntaxWrapper.Identifier;

            public ParameterListSyntax ParameterList => syntaxWrapper.ParameterList;

            public bool HasImplementation => Body != null || ExpressionBody != null;

            public bool IsLocal => true;
        }

        private class MethodDeclarationSyntaxAdapter : IMethodDeclaration
        {
            private readonly MethodDeclarationSyntax declarationSyntax;

            public MethodDeclarationSyntaxAdapter(MethodDeclarationSyntax declarationSyntax)
                => this.declarationSyntax = declarationSyntax;

            public BlockSyntax Body => declarationSyntax.Body;

            public ArrowExpressionClauseSyntax ExpressionBody => declarationSyntax.ExpressionBody;

            public SyntaxToken Identifier => declarationSyntax.Identifier;

            public ParameterListSyntax ParameterList => declarationSyntax.ParameterList;

            public bool HasImplementation => Body != null || ExpressionBody != null;

            public bool IsLocal => false;
        }
    }
}
