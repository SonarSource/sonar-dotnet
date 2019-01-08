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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Syntax and semantic information about a single method declaration
    /// </summary>
    public class MethodDeclarationContext
    {
        private readonly Compilation compilation;

        public IMethodSymbol MethodSymbol { get; }

        public SemanticModel GetSemanticModel(SyntaxNode syntaxNode) =>
            compilation.GetSemanticModel(syntaxNode.SyntaxTree);

        public MethodDeclarationContext(IMethodSymbol methodSymbol, Compilation compilation)
        {
            MethodSymbol = methodSymbol;
            this.compilation = compilation;
        }
    }
}
