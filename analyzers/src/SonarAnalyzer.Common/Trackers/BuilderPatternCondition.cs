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

using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public abstract class BuilderPatternCondition<TInvocationSyntax>
        where TInvocationSyntax : SyntaxNode
    {
        private readonly bool constructorIsSafe;
        private readonly BuilderPatternDescriptor<TInvocationSyntax>[] descriptors;
        private readonly AssignmentFinder assignmentFinder;

        protected abstract SyntaxNode RemoveParentheses(SyntaxNode node);
        protected abstract SyntaxNode GetExpression(TInvocationSyntax node);
        protected abstract string GetIdentifierName(TInvocationSyntax node);
        protected abstract bool IsMemberAccess(SyntaxNode node, out SyntaxNode memberAccessExpression);
        protected abstract bool IsObjectCreation(SyntaxNode node);
        protected abstract bool IsIdentifier(SyntaxNode node, out string identifierName);

        protected BuilderPatternCondition(bool constructorIsSafe, BuilderPatternDescriptor<TInvocationSyntax>[] descriptors, AssignmentFinder assignmentFinder)
        {
            this.constructorIsSafe = constructorIsSafe;
            this.descriptors = descriptors;
            this.assignmentFinder = assignmentFinder;
        }

        public bool IsInvalidBuilderInitialization(InvocationContext context)
        {
            var current = context.Node;
            while (current != null)
            {
                current = RemoveParentheses(current);
                if (current is TInvocationSyntax invocation)
                {
                    var invocationContext = new InvocationContext(invocation, GetIdentifierName(invocation), context.SemanticModel);
                    if (descriptors.FirstOrDefault(x => x.IsMatch(invocationContext)) is { } desc)
                    {
                        return !desc.IsValid(invocation);
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
                    // When tracking reaches the local variable in invocation chain 'variable.MethodA().MethodB()'
                    // we'll try to find preceding assignment to that variable to continue inspection of initialization chain.
                    current = assignmentFinder.FindLinearPrecedingAssignmentExpression(identifierName, current);
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
