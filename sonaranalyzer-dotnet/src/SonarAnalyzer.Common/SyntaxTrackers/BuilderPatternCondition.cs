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

using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public abstract class BuilderPatternCondition<TInvocationSyntax>
        where TInvocationSyntax : SyntaxNode
    {
        private readonly bool constructorIsSafe;
        private readonly BuilderPatternDescriptor<TInvocationSyntax>[] descriptors;

        protected abstract SyntaxNode RemoveParentheses(SyntaxNode node);
        protected abstract SyntaxNode GetTopMostContainingMethod(SyntaxNode node);
        protected abstract SyntaxNode GetExpression(TInvocationSyntax node);
        protected abstract string GetIdentifierName(TInvocationSyntax node);
        protected abstract bool IsMemberAccess(SyntaxNode node, out SyntaxNode memberAccessExpression);
        protected abstract bool IsObjectCreation(SyntaxNode node);
        protected abstract bool IsIdentifier(SyntaxNode node, out string identifierName);
        protected abstract bool IsAssignmentToIdentifier(SyntaxNode node, string identifierName, out SyntaxNode rightExpression);
        protected abstract bool IsIdentifierDeclaration(SyntaxNode node, string identifierName, out SyntaxNode initializer);

        protected BuilderPatternCondition(bool constructorIsSafe, params BuilderPatternDescriptor<TInvocationSyntax>[] descriptors)
        {
            this.constructorIsSafe = constructorIsSafe;
            this.descriptors = descriptors;
        }

        public bool InvalidBuilderInitialization(InvocationContext context)
        {
            var current = context.Invocation;
            while (current != null)
            {
                current = RemoveParentheses(current);
                if (current is TInvocationSyntax invocation)
                {
                    var invocationContext = new InvocationContext(invocation, GetIdentifierName(invocation), context.SemanticModel);
                    if (this.descriptors.FirstOrDefault(x => x.IsMatch(invocationContext)) is { } desc)
                    {
                        return !desc.IsValid(context, invocation);
                    }
                    current = GetExpression(invocation);
                }
                else if (IsMemberAccess(current, out var memberAccessExpression))
                {
                    current = memberAccessExpression;
                }
                else if (IsObjectCreation(current))
                {
                    // We're sure that full invocation chain started here => we've seen all configuration invocations.
                    return !constructorIsSafe;
                }
                else if (IsIdentifier(current, out var identifierName))
                {
                    if (!(context.SemanticModel.GetSymbolInfo(current).Symbol is ILocalSymbol))
                    {
                        return false;
                    }
                    current = FindLinearPrecedingAssignmentExpression(identifierName, current);
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private SyntaxNode FindLinearPrecedingAssignmentExpression(string identifierName, SyntaxNode current)
        {
            var method = GetTopMostContainingMethod(current);
            while (current != method && current?.Parent != null)
            {
                var parent = current.Parent;
                foreach (var statement in parent.ChildNodes().TakeWhile(x => x != current).Reverse())
                {
                    if (IsAssignmentToIdentifier(statement, identifierName, out var right))
                    {
                        return right;
                    }
                    else if (IsIdentifierDeclaration(statement, identifierName, out var initializer))
                    {
                        return initializer;
                    }
                    else if (statement.DescendantNodes().Any(x => IsAssignmentToIdentifier(x, identifierName, out _)))
                    {
                        return null; // Assignment inside nested statement (if, try, for, ...)
                    }
                }
                current = parent;
            }
            return null;
        }
    }
}
