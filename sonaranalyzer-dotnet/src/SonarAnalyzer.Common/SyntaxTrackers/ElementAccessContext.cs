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

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Syntax and semantic information about a single element access
    /// </summary>
    public class ElementAccessContext
    {
        public SyntaxNode Expression { get; }
        public SemanticModel SemanticModel { get; }
        public Lazy<IPropertySymbol> InvokedPropertySymbol { get; }

        public ElementAccessContext(SyntaxNode invocation, SemanticModel semanticModel)
        {
            Expression = invocation;
            SemanticModel = semanticModel;
            InvokedPropertySymbol = new Lazy<IPropertySymbol>(() => semanticModel.GetSymbolInfo(Expression).Symbol as IPropertySymbol);
        }
    }
}
