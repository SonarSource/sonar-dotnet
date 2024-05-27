/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Common.Walkers
{
    internal class ParameterValidationInMethodWalker : SafeCSharpSyntaxWalker
    {
        private static readonly ISet<SyntaxKind> SubMethodEquivalents =
            new HashSet<SyntaxKind>
            {
                SyntaxKindEx.LocalFunctionStatement,        // Local function
                SyntaxKind.SimpleLambdaExpression,          // Action
                SyntaxKind.ParenthesizedLambdaExpression    // Func
            };

        private readonly SemanticModel semanticModel;
        private readonly List<SecondaryLocation> argumentExceptionLocations = new();

        protected bool keepWalking = true;

        public IEnumerable<SecondaryLocation> ArgumentExceptionLocations => argumentExceptionLocations;

        public ParameterValidationInMethodWalker(SemanticModel semanticModel) =>
            this.semanticModel = semanticModel;

        public override void Visit(SyntaxNode node)
        {
            if (keepWalking
                && !node.IsAnyKind(SubMethodEquivalents))  // Don't explore deeper if this node is equivalent to a method declaration
            {
                base.Visit(node);
            }
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            // When throw is like `throw new XXX` where XXX derives from ArgumentException, save location
            if (node.Expression != null
                && semanticModel.GetTypeInfo(node.Expression) is var typeInfo
                && typeInfo.Type.DerivesFrom(KnownType.System_ArgumentException))
            {
                argumentExceptionLocations.Add(node.Expression.ToSecondaryLocation());
            }

            // there is no need to visit children
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.IsMemberAccessOnKnownType("ThrowIfNull", KnownType.System_ArgumentNullException, semanticModel))
            {
                // "ThrowIfNull" returns void so it cannot be an argument. We can stop.
                argumentExceptionLocations.Add(node.ToSecondaryLocation());
            }
            else
            {
                // Need to check the children of this node because of the pattern (await SomeTask()).Invocation()
                base.VisitInvocationExpression(node);
            }
        }
    }
}
