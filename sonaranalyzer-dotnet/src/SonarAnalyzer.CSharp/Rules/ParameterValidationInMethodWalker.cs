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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    internal class ParameterValidationInMethodWalker : CSharpSyntaxWalker
    {
        private static readonly ISet<SyntaxKind> SubMethodEquivalents =
            new HashSet<SyntaxKind>
            {
                SyntaxKindEx.LocalFunctionStatement,        // Local function
                SyntaxKind.SimpleLambdaExpression,          // Action
                SyntaxKind.ParenthesizedLambdaExpression    // Func
            };

        private readonly SemanticModel semanticModel;
        private readonly List<Location> argumentExceptionLocations = new List<Location>();

        private bool keepWalking = true;

        public IEnumerable<Location> ArgumentExceptionLocations => this.argumentExceptionLocations;

        public ParameterValidationInMethodWalker(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        public override void Visit(SyntaxNode node)
        {
            if (this.keepWalking &&
                !node.IsAnyKind(SubMethodEquivalents))  // Don't explore deeper if this node is equivalent to a method declaration
            {
                base.Visit(node);
            }
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            this.keepWalking = false;
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            // When throw is like `throw new XXX` where XXX derives from ArgumentException, save location
            if (node.Expression is ObjectCreationExpressionSyntax oces &&
                this.semanticModel.GetSymbolInfo(oces.Type).Symbol?.OriginalDefinition is ITypeSymbol typeSymbol &&
                typeSymbol.DerivesFrom(KnownType.System_ArgumentException))
            {
                this.argumentExceptionLocations.Add(oces.GetLocation());
            }

            base.VisitThrowStatement(node);
        }
    }
}
