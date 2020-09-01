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
    public sealed class DoNotNestTypesInArguments : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4017";
        private const string MessageFormat = "Refactor this method to remove the nested type argument.";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var argumentTypeSymbols = GetParametersSyntaxNodes(c.Node).Where(p => MaxDepthReached(p, c.SemanticModel));

                    foreach (var argument in argumentTypeSymbols)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argument.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKindEx.LocalFunctionStatement);

        private static bool MaxDepthReached(SyntaxNode parameterSyntax, SemanticModel model)
        {
            var walker = new GenericWalker(2, model);
            walker.SafeVisit(parameterSyntax);
            return walker.HasReachedMaxDepth;
        }

        private static IEnumerable<ParameterSyntax> GetParametersSyntaxNodes(SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax methodDeclarationSyntax => methodDeclarationSyntax.ParameterList.Parameters,
                var wrapper when LocalFunctionStatementSyntaxWrapper.IsInstance(wrapper) => ((LocalFunctionStatementSyntaxWrapper)wrapper).ParameterList.Parameters,
                _ => Enumerable.Empty<ParameterSyntax>()
            };

        private sealed class GenericWalker : CSharpSyntaxWalker
        {
            private static readonly ImmutableArray<KnownType> ignoredTypes =
                KnownType.SystemFuncVariants
                         .Union(KnownType.SystemActionVariants)
                         .Union(new[] { KnownType.System_Linq_Expressions_Expression_T })
                         .ToImmutableArray();

            private readonly int maxDepth;
            private readonly SemanticModel model;

            private int depth;

            public bool HasReachedMaxDepth { get; private set; }

            public GenericWalker(int maxDepth, SemanticModel model)
            {
                this.maxDepth = maxDepth;
                this.model = model;
            }

            public override void VisitGenericName(GenericNameSyntax node)
            {
                if (!(model.GetSymbolInfo(node).Symbol is INamedTypeSymbol namedTypeSymbol))
                {
                    return;
                }

                if (!namedTypeSymbol.ConstructedFrom.IsAny(ignoredTypes))
                {
                    if (depth == maxDepth - 1)
                    {
                        HasReachedMaxDepth = true;
                    }
                    else
                    {
                        depth++;
                        base.VisitGenericName(node);
                        depth--;
                    }
                }
                else
                {
                    base.VisitGenericName(node);
                }
            }
        }
    }
}
