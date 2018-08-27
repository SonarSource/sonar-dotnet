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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ParameterAssignedTo : ParameterAssignedToBase<SyntaxKind, AssignmentExpressionSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<SyntaxKind> kindsOfInterest = ImmutableArray.Create(
            SyntaxKind.SimpleAssignmentExpression
            );

        public override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => kindsOfInterest;

        protected override bool IsAssignmentToCatchVariable(ISymbol symbol, SyntaxNode node)
        {
            if (!(symbol is ILocalSymbol localSymbol))
            {
                return false;
            }

            var result = localSymbol.DeclaringSyntaxReferences
                .Select(declaringSyntaxReference => declaringSyntaxReference.GetSyntax())
                .Any(syntaxNode =>
                    syntaxNode.Parent is CatchClauseSyntax &&
                    ((CatchClauseSyntax)syntaxNode.Parent).Declaration == syntaxNode);

            return result;
        }

        protected override bool IsAssignmentToParameter(ISymbol symbol)
        {
            var parameterSymbol = symbol as IParameterSymbol;
            var result = parameterSymbol?.RefKind == RefKind.None;
            return result;
        }

        protected override bool IsReadBefore(SemanticModel semanticModel, ISymbol parameterSymbol, AssignmentExpressionSyntax assignment)
        {
            return GetPreviousStatements(assignment)
                .Union(new[] { assignment.Right })
                .SelectMany(s => s.DescendantNodes(n => true))
                .OfType<IdentifierNameSyntax>()
                .Any(node =>
                {
                    var nodeSymbol = semanticModel.GetSymbolInfo(node).Symbol;
                    return parameterSymbol.Equals(nodeSymbol) && IsReadAccess(node);
                });
        }

        protected override SyntaxNode GetAssignedNode(AssignmentExpressionSyntax assignment) => assignment.Left;

        protected sealed override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.CSharp.GeneratedCodeRecognizer.Instance;

        /// <summary>
        /// Returns all statements before the specified statement within the containing method.
        /// This method recursively traverses all parent blocks of the provided statement.
        /// </summary>
        private static IEnumerable<SyntaxNode> GetPreviousStatements(SyntaxNode statement)
        {
            var previousStatements = statement.Parent.ChildNodes()
                .OfType<StatementSyntax>()
                .TakeWhile(x => x != statement)
                .Reverse();

            return statement.Parent is StatementSyntax parentStatement
                ? previousStatements.Union(GetPreviousStatements(parentStatement))
                : previousStatements;
        }

        private bool IsReadAccess(IdentifierNameSyntax node)
        {
            bool isLeftSideOfAssignment =
                node.Parent is AssignmentExpressionSyntax assignmentExpression &&
                assignmentExpression.Left == node;

            return !isLeftSideOfAssignment;
        }
    }
}

