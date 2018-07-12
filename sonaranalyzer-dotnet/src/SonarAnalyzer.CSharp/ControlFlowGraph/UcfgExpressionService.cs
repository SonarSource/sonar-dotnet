/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal class UcfgExpressionService
    {
        private static readonly ISet<KnownType> UnsupportedVariableTypes = new[] { KnownType.System_Boolean }
            .Union(KnownType.IntegralNumbers)
            .Union(KnownType.NonIntegralNumbers)
            .Union(KnownType.PointerTypes)
            .ToHashSet();

        private readonly Dictionary<SyntaxNode, UcfgExpression> cache
            = new Dictionary<SyntaxNode, UcfgExpression>();

        private int numberedVariableCounter;

        public void Associate(SyntaxNode syntaxNode, UcfgExpression expression) =>
            cache[syntaxNode.RemoveParentheses()] = expression;

        public UcfgExpression GetExpression(SyntaxNode syntaxNode) =>
            cache.GetValueOrDefault(syntaxNode.RemoveParentheses(), UcfgExpression.Unknown);

        public UcfgExpression CreateVariable(ITypeSymbol returnType) =>
            new UcfgExpression.VariableExpression($"%{numberedVariableCounter++}", returnType);

        public UcfgExpression CreateClassName(INamedTypeSymbol namedTypeSymbol) =>
            new UcfgExpression.ClassNameExpression(namedTypeSymbol);

        public UcfgExpression CreateConstant(IMethodSymbol methodSymbol) =>
            new UcfgExpression.ConstantExpression(methodSymbol);

        public UcfgExpression CreateArrayAccess(ISymbol symbol, UcfgExpression targetExpression) =>
            new UcfgExpression.ElementAccessExpression(symbol, targetExpression);

        public UcfgExpression Create(ISymbol symbol, SyntaxNode node, UcfgExpression targetExpression)
        {
            switch (symbol)
            {
                case null:
                    return UcfgExpression.Unknown;

                case IParameterSymbol parameterSymbol:
                    return parameterSymbol.Type.IsAny(UnsupportedVariableTypes)
                        ? (UcfgExpression)new UcfgExpression.ConstantExpression(parameterSymbol)
                        : new UcfgExpression.VariableExpression(parameterSymbol, node);

                case ILocalSymbol localSymbol:
                    return new UcfgExpression.VariableExpression(symbol, node);

                case IFieldSymbol fieldSymbol:
                    return new UcfgExpression.FieldAccessExpression(fieldSymbol, node, targetExpression);

                case IPropertySymbol propertySymbol:
                    return new UcfgExpression.PropertyAccessExpression(propertySymbol, node, targetExpression);

                case INamedTypeSymbol namedTypeSymbol:
                    return CreateClassName(namedTypeSymbol);

                case IMethodSymbol methodSymbol:
                    return new UcfgExpression.MethodAccessExpression(methodSymbol, node, targetExpression);

                default:
                    throw new UcfgException($"Create UcfgExpression does not expect symbol of type '{symbol.GetType().Name}'.");
            }
        }
    }
}
