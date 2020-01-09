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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Helpers.Wrappers
{
    public class LocalFunctionStatementAdapter : IMethodDeclaration
    {
        private readonly LocalFunctionStatementSyntaxWrapper _syntaxWrapper;

        public LocalFunctionStatementAdapter(LocalFunctionStatementSyntaxWrapper syntaxWrapper)
        {
            if (syntaxWrapper.SyntaxNode == null)
            {
                throw new ArgumentException(nameof(syntaxWrapper));
            }

            this._syntaxWrapper = syntaxWrapper;
        }

        public BlockSyntax Body { get => this._syntaxWrapper.Body; }

        public SyntaxToken Identifier { get => this._syntaxWrapper.Identifier; }

        public ParameterListSyntax ParameterList { get => this._syntaxWrapper.ParameterList; }
    }
}
