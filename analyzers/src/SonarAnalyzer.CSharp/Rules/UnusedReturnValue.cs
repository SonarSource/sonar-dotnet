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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;
using NodeSymbolAndSemanticModel = SonarAnalyzer.Helpers.NodeSymbolAndSemanticModel<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax, Microsoft.CodeAnalysis.IMethodSymbol>;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnusedReturnValue : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3241";
        private const string MessageFormat = "Change return type to 'void'; not a single caller uses the returned value.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeNamedTypes, SymbolKind.NamedType);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeLocalFunctionStatements, SyntaxKindEx.LocalFunctionStatement);
        }

        private static void AnalyzeNamedTypes(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            if (!namedType.IsClassOrStruct() || namedType.ContainingType != null)
            {
                return;
            }

            var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, context.Compilation);

            var declaredPrivateMethodsWithReturn = CollectRemovableMethods(removableDeclarationCollector).ToList();
            if (!declaredPrivateMethodsWithReturn.Any())
            {
                return;
            }

            var invocations = removableDeclarationCollector.TypeDeclarations.SelectMany(x => FilterInvocations(x)).ToList();

            foreach (var declaredPrivateMethodWithReturn in declaredPrivateMethodsWithReturn)
            {
                var matchingInvocations = invocations
                    .Where(invocation => invocation.Symbol.OriginalDefinition.Equals(declaredPrivateMethodWithReturn.Symbol))
                    .ToList();

                // Method invocation is noncompliant when there is at least 1 invocation of the method, and no invocation is using the return value. The case of 0 invocation is handled by S1144.
                if (matchingInvocations.Any() && !matchingInvocations.Any(x => IsReturnValueUsed(x)))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, declaredPrivateMethodWithReturn.Node.ReturnType.GetLocation()));
                }
            }
        }

        private static void AnalyzeLocalFunctionStatements(SyntaxNodeAnalysisContext context)
        {
            var localFunctionSyntax = (LocalFunctionStatementSyntaxWrapper)context.Node;
            var topmostContainingMethod = context.Node.GetTopMostContainingMethod();

            if (!(context.SemanticModel.GetDeclaredSymbol(localFunctionSyntax) is IMethodSymbol localFunctionSymbol)
                || localFunctionSymbol.ReturnsVoid
                || topmostContainingMethod == null)
            {
                return;
            }

            var matchingInvocations = GetLocalMatchingInvocations(topmostContainingMethod, localFunctionSymbol, context.SemanticModel).ToList();
            // Method invocation is noncompliant when there is at least 1 invocation of the method, and no invocation is using the return value. The case of 0 invocation is handled by S1144.
            if (matchingInvocations.Any() && !matchingInvocations.Any(x => IsReturnValueUsed(x)))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, localFunctionSyntax.ReturnType.GetLocation()));
            }
        }

        private static bool IsReturnValueUsed(NodeSymbolAndSemanticModel matchingInvocation) =>
            !IsExpressionStatement(matchingInvocation.Node.Parent)
            && !IsActionLambda(matchingInvocation.Node.Parent, matchingInvocation.SemanticModel);

        private static bool IsActionLambda(SyntaxNode node, SemanticModel semanticModel) =>
            node is LambdaExpressionSyntax lambda
            && semanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol { ReturnsVoid: true };

        private static bool IsExpressionStatement(SyntaxNode node) =>
            node is ExpressionStatementSyntax;

        private static IEnumerable<NodeSymbolAndSemanticModel> GetLocalMatchingInvocations(SyntaxNode containingMethod, IMethodSymbol invocationSymbol, SemanticModel semanticModel) =>
            containingMethod.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(x => semanticModel.GetSymbolInfo(x.Expression).Symbol is IMethodSymbol methodSymbol && invocationSymbol.Equals(methodSymbol))
                .Select(x => new NodeSymbolAndSemanticModel(semanticModel, x, invocationSymbol))
                .ToList();

        private static IEnumerable<NodeSymbolAndSemanticModel> FilterInvocations(NodeAndSemanticModel<BaseTypeDeclarationSyntax> container) =>
            container.Node.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(x => new NodeSymbolAndSemanticModel(container.SemanticModel, x, container.SemanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol))
                .Where(x => x.Symbol != null);

        private static IEnumerable<NodeSymbolAndSemanticModel<MethodDeclarationSyntax, IMethodSymbol>> CollectRemovableMethods(CSharpRemovableDeclarationCollector removableDeclarationCollector) =>
                removableDeclarationCollector.TypeDeclarations
                    .SelectMany(container => container.Node.DescendantNodes(CSharpRemovableDeclarationCollector.IsNodeContainerTypeDeclaration)
                        .OfType<MethodDeclarationSyntax>()
                        .Select(x => new NodeSymbolAndSemanticModel<MethodDeclarationSyntax, IMethodSymbol>(container.SemanticModel, x, container.SemanticModel.GetDeclaredSymbol(x))))
                        .Where(x => x.Symbol is { ReturnsVoid: false } && CSharpRemovableDeclarationCollector.IsRemovable(x.Symbol, Accessibility.Private));
    }
}
