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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal class UcfgExpressionService
    {
        private readonly Dictionary<SyntaxNode, Expression> syntaxNodeToUcfgExpressionCache
            = new Dictionary<SyntaxNode, Expression>();

        public void RegisterAsThis(SyntaxNode syntaxNode) =>
            syntaxNodeToUcfgExpressionCache[syntaxNode] = UcfgExpression.This;

        public void RegisterAsVariable(SyntaxNode syntaxNode, string variableName) =>
            syntaxNodeToUcfgExpressionCache[syntaxNode] = UcfgExpression.FromVariable(variableName);

        public void RegisterAsConstant(SyntaxNode syntaxNode) =>
            syntaxNodeToUcfgExpressionCache[syntaxNode] = UcfgExpression.Constant;

        public Expression Get(SyntaxNode syntaxNode) =>
            syntaxNodeToUcfgExpressionCache.GetValueOrDefault(syntaxNode.RemoveParentheses())
            // In some cases the CFG does not contain all syntax nodes that were used in an expression, for example when
            // ternary operator is passed as an argument. This could potentially be improved, but for the time being the constant
            // expression fallback will do what we used to do before.
            ?? UcfgExpression.Constant;
    }
}
