/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers.Wrappers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Helpers
{
    public static class TypeDeclarationSyntaxExtensions
    {
        /// <summary>
        /// Returns a union of all the methods and local functions from a given type declaration.
        /// </summary>
        public static IEnumerable<IMethodDeclaration> GetMethodDeclarations(this TypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration
                .Members
                .OfType<MethodDeclarationSyntax>()
                .SelectMany(method => GetLocalFunctions(method).Union(new List<IMethodDeclaration> { new MethodDeclarationSyntaxAdapter(method)}));

        private static IEnumerable<LocalFunctionStatementAdapter> GetLocalFunctions(MethodDeclarationSyntax methodDeclaration)
            => methodDeclaration.DescendantNodes()
                .Where(member => member.IsKind(SyntaxKindEx.LocalFunctionStatement))
                .Select(member => new LocalFunctionStatementAdapter((LocalFunctionStatementSyntaxWrapper)member));

        private class LocalFunctionStatementAdapter : IMethodDeclaration
        {
            private readonly LocalFunctionStatementSyntaxWrapper _syntaxWrapper;

            public LocalFunctionStatementAdapter(LocalFunctionStatementSyntaxWrapper syntaxWrapper)
                => this._syntaxWrapper = syntaxWrapper;

            public BlockSyntax Body { get => this._syntaxWrapper.Body; }

            public SyntaxToken Identifier { get => this._syntaxWrapper.Identifier; }

            public ParameterListSyntax ParameterList { get => this._syntaxWrapper.ParameterList; }
        }

        private class MethodDeclarationSyntaxAdapter : IMethodDeclaration
        {
            private readonly MethodDeclarationSyntax _declarationSyntax;

            public MethodDeclarationSyntaxAdapter(MethodDeclarationSyntax declarationSyntax)
                => this._declarationSyntax = declarationSyntax;

            public BlockSyntax Body { get => this._declarationSyntax.Body; }

            public SyntaxToken Identifier { get => this._declarationSyntax.Identifier; }

            public ParameterListSyntax ParameterList { get => this._declarationSyntax.ParameterList; }
        }
    }
}
