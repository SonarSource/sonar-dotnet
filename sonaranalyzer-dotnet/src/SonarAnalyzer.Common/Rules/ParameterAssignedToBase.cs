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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{

    public abstract class ParameterAssignedToBase<TLanguageKindEnum, TAssignmentStatementSyntax, TIdentifierNameSyntax> : SonarDiagnosticAnalyzer
        where TLanguageKindEnum : struct
        where TAssignmentStatementSyntax : SyntaxNode
        where TIdentifierNameSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S1226";
        protected const string MessageFormat = "Introduce a new variable instead of reusing the parameter '{0}'.";

        private readonly DiagnosticDescriptor rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract bool IsAssignmentToCatchVariable(ISymbol symbol, SyntaxNode node);

        protected abstract bool IsAssignmentToParameter(ISymbol symbol);

        protected abstract SyntaxNode AssignmentLeft(TAssignmentStatementSyntax assignment);

        protected abstract SyntaxNode AssignmentRight(TAssignmentStatementSyntax assignment);

        protected abstract TLanguageKindEnum SyntaxKindOfInterest { get; }

        protected ParameterAssignedToBase(System.Resources.ResourceManager rspecResources)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);
        }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var assignment = (TAssignmentStatementSyntax)c.Node;
                    var left = AssignmentLeft(assignment);
                    var symbol = c.SemanticModel.GetSymbolInfo(left).Symbol;

                    if (symbol != null
                        && (IsAssignmentToParameter(symbol) || IsAssignmentToCatchVariable(symbol, left))
                        && (!IsReadBefore(c.SemanticModel, symbol, assignment)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], left.GetLocation(), left.ToString()));
                    }
                },
                SyntaxKindOfInterest);
        }

        private bool IsReadBefore(SemanticModel semanticModel, ISymbol parameterSymbol, TAssignmentStatementSyntax assignment)
        {
            // Same problem as in VB.NET / IsAssignmentToCatchVariable:
            // parameterSymbol.DeclaringSyntaxReferences is empty for Catch syntax in VB.NET as well as for indexer syntax for C#
            // https://github.com/dotnet/roslyn/issues/6209
            var stopLocation = parameterSymbol.Locations.FirstOrDefault();
            if (stopLocation == null)
            {
                return true; //If we can't find the location, it's going to be FN
            }
            return GetPreviousNodes(stopLocation, assignment)
                .Union(AssignmentRight(assignment).DescendantNodes())
                .OfType<TIdentifierNameSyntax>()
                .Any(node =>
                {
                    var nodeSymbol = semanticModel.GetSymbolInfo(node).Symbol;
                    return parameterSymbol.Equals(nodeSymbol);
                });
        }

        /// <summary>
        /// Returns all nodes before the specified statement to the declaration of variable/parameter given by stopLocation.
        /// This method recursively traverses all parent blocks of the provided statement.
        /// </summary>
        private static IEnumerable<SyntaxNode> GetPreviousNodes(Location stopLocation, SyntaxNode statement) 
        {
            if (statement == null || statement.GetLocation().SourceSpan.IntersectsWith(stopLocation.SourceSpan))   //Method declaration or Catch variable declaration, stop here and do not include this statement
            {
                return new SyntaxNode[] { };
            }
            var previousNodes = statement.Parent.ChildNodes()
                .TakeWhile(x => x != statement)     //Take all from beginning, including "catch ex" on the way, down to current statement
                .Reverse()                          //Reverse in order to keep the tail
                .TakeWhile(x => !x.GetLocation().SourceSpan.IntersectsWith(stopLocation.SourceSpan))    //Keep the tail until "catch ex" or "int i" is found
                .SelectMany(x => x.DescendantNodes());

            return previousNodes.Union(GetPreviousNodes(stopLocation, statement.Parent));
        }

    }
}
