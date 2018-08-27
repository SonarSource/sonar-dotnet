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
    public sealed class UnusedReturnValue : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3241";
        private const string MessageFormat = "Change return type to 'void'; not a single caller uses the returned value.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (!namedType.IsClassOrStruct() ||
                        namedType.ContainingType != null)
                    {
                        return;
                    }

                    var removableDeclarationCollector = new RemovableDeclarationCollector(namedType, c.Compilation);

                    var declaredPrivateMethodsWithReturn = CollectRemovableMethods(removableDeclarationCollector).ToList();
                    if (!declaredPrivateMethodsWithReturn.Any())
                    {
                        return;
                    }

                    var invocations = CollectInvocations(removableDeclarationCollector.TypeDeclarations).ToList();

                    foreach (var declaredPrivateMethodWithReturn in declaredPrivateMethodsWithReturn)
                    {
                        var matchingInvocations = invocations
                            .Where(inv => object.Equals(inv.Symbol.OriginalDefinition, declaredPrivateMethodWithReturn.Symbol))
                            .ToList();

                        if (!matchingInvocations.Any())
                        {
                            /// this is handled by S1144 <see cref="UnusedPrivateMember"/>
                            continue;
                        }

                        if (!IsReturnValueUsed(matchingInvocations))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, declaredPrivateMethodWithReturn.SyntaxNode.ReturnType.GetLocation()));
                        }
                    }
                },
                SymbolKind.NamedType);
        }

        private static bool IsReturnValueUsed(IEnumerable<SyntaxNodeSymbolSemanticModelTuple<InvocationExpressionSyntax, IMethodSymbol>> matchingInvocations)
        {
            return matchingInvocations.Any(invocation =>
                !IsExpressionStatement(invocation.SyntaxNode.Parent) &&
                !IsActionLambda(invocation.SyntaxNode.Parent, invocation.SemanticModel));
        }

        private static bool IsActionLambda(SyntaxNode node, SemanticModel semanticModel)
        {
            if (!(node is LambdaExpressionSyntax lambda))
            {
                return false;
            }

            return semanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol symbol && symbol.ReturnsVoid;
        }

        private static bool IsExpressionStatement(SyntaxNode node)
        {
            return node is ExpressionStatementSyntax;
        }

        private static IEnumerable<SyntaxNodeSymbolSemanticModelTuple<InvocationExpressionSyntax, IMethodSymbol>> CollectInvocations(
            IEnumerable<SyntaxNodeSemanticModelTuple<BaseTypeDeclarationSyntax>> containers)
        {
            return containers
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(node =>
                        new SyntaxNodeSymbolSemanticModelTuple<InvocationExpressionSyntax, IMethodSymbol>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol
                        }))
                    .Where(invocation => invocation.Symbol != null);
        }

        private static IEnumerable<SyntaxNodeSymbolSemanticModelTuple<MethodDeclarationSyntax, IMethodSymbol>> CollectRemovableMethods(
            RemovableDeclarationCollector removableDeclarationCollector)
        {
            return removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes(RemovableDeclarationCollector.IsNodeContainerTypeDeclaration)
                    .OfType<MethodDeclarationSyntax>()
                    .Select(node =>
                        new SyntaxNodeSymbolSemanticModelTuple<MethodDeclarationSyntax, IMethodSymbol>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetDeclaredSymbol(node)
                        }))
                    .Where(node =>
                        node.Symbol != null &&
                        !node.Symbol.ReturnsVoid &&
                        RemovableDeclarationCollector.IsRemovable(node.Symbol, Accessibility.Private));
        }
    }
}
