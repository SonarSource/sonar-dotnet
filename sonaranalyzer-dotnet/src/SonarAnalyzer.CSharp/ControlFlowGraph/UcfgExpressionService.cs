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
        internal const string DefaultConstantValue = "\"\"";
        internal static readonly Expression UnknownExpression = new Expression { Const = new Constant { Value = "{unknown}" } };

        private readonly Dictionary<SyntaxNode, Expression> cache = new Dictionary<SyntaxNode, Expression>();

        private int numberedVariableCounter;

        public static Expression This { get; } = new Expression { This = new This() };

        public void Associate(SyntaxNode syntaxNode, Expression expression) =>
            this.cache[syntaxNode.RemoveParentheses()] = expression;

        public Expression GetOrDefault(SyntaxNode syntaxNode) =>
            this.cache.GetValueOrDefault(syntaxNode.RemoveParentheses(), UnknownExpression);

        public Expression CreateVariable(string variableName = null) =>
            new Expression { Var = new Variable { Name = variableName ?? $"%{this.numberedVariableCounter++}" } };

        public Expression CreateConstant(string value = DefaultConstantValue) =>
            new Expression { Const = new Constant { Value = value } };

        public Expression CreateClassName(INamedTypeSymbol namedTypeSymbol) =>
            new Expression
            {
                Classname = new ClassName
                {
                    Classname = namedTypeSymbol?.ConstructedFrom.ToDisplayString() ?? UcfgBuiltInMethodId.Unknown
                }
            };

        public Expression CreateFieldAccess(string fieldName, Expression target)
        {
            var expression = new Expression { FieldAccess = new FieldAccess { Field = new Variable { Name = fieldName } } };

            switch (target.ExprCase)
            {
                case Expression.ExprOneofCase.Var:
                    expression.FieldAccess.Object = target.Var;
                    break;

                case Expression.ExprOneofCase.This:
                    expression.FieldAccess.This = target.This;
                    break;

                case Expression.ExprOneofCase.Classname:
                    expression.FieldAccess.Classname = target.Classname;
                    break;

                default:
                    //throw new UcfgException("Invalid target"); // TODO: These cases should be fixed
                    expression.FieldAccess.This = This.This;
                    break;
            }

            return expression;
        }

        /// <summary>
        /// If a node returns something other than "UnknownExpression" then it
        /// has already been processed succesfully
        /// </summary>
        public bool IsAlreadyProcessed(SyntaxNode syntaxNode) =>
            GetOrDefault(syntaxNode) != UnknownExpression;
    }
}
