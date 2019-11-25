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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotCopyArraysInProperties : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2365";
        private const string MessageFormat = "Refactor '{0}' into a method, properties should not copy collections.";

        private static readonly DiagnosticDescriptor s2365 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s2365);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var property = (PropertyDeclarationSyntax)c.Node;

                    var body = GetPropertyBody(property);
                    if (body == null)
                    {
                        return;
                    }

                    var walker = new PropertyWalker(c.SemanticModel);

                    walker.SafeVisit(body);

                    foreach (var location in walker.Locations)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(s2365, location, property.Identifier.Text));
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static SyntaxNode GetPropertyBody(PropertyDeclarationSyntax property)
        {
            return property.ExpressionBody ??
                property.AccessorList
                    .Accessors
                    .Where(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))
                    .Select(a => (SyntaxNode)a.Body ?? a.ExpressionBody())
                    .FirstOrDefault();
        }

        private class PropertyWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;
            private readonly List<Location> locations = new List<Location>();

            private static readonly HashSet<SyntaxKind> returnStatements = new HashSet<SyntaxKind>
            {
                SyntaxKind.ReturnStatement,
                SyntaxKind.ArrowExpressionClause,
            };

            public IEnumerable<Location> Locations => this.locations;

            public PropertyWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (!(this.semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol invokedSymbol))
                {
                    return;
                }

                if (!invokedSymbol.IsArrayClone() &&
                    !invokedSymbol.IsEnumerableToList() &&
                    !invokedSymbol.IsEnumerableToArray())
                {
                    return;
                }

                var returnOrAssignment = node
                    .Ancestors()
                    .FirstOrDefault(IsReturnOrAssignment);

                if (IsReturn(returnOrAssignment))
                {
                    this.locations.Add(node.Expression.GetLocation());
                }
            }

            private static bool IsReturnOrAssignment(SyntaxNode node)
            {
                return IsReturn(node)
                    || node.IsKind(SyntaxKind.SimpleAssignmentExpression);
            }

            private static bool IsReturn(SyntaxNode node)
            {
                return node != null && node.IsAnyKind(returnStatements);
            }
        }
    }
}
