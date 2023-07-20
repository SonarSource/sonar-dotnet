/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using NodeSymbolAndModel = SonarAnalyzer.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax, Microsoft.CodeAnalysis.IMethodSymbol>;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnusedReturnValue : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3241";
        private const string MessageFormat = "Change return type to 'void'; not a single caller uses the returned value.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeNamedTypes, SymbolKind.NamedType);
            context.RegisterNodeAction(AnalyzeLocalFunctionStatements, SyntaxKindEx.LocalFunctionStatement);
        }

        private static void AnalyzeNamedTypes(SonarSymbolReportingContext context)
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

            var invocations = removableDeclarationCollector.TypeDeclarations.SelectMany(FilterInvocations).ToList();

            foreach (var declaredPrivateMethodWithReturn in declaredPrivateMethodsWithReturn)
            {
                var matchingInvocations = invocations
                    .Where(invocation => invocation.Symbol.OriginalDefinition.Equals(declaredPrivateMethodWithReturn.Symbol))
                    .ToList();

                // Method invocation is noncompliant when there is at least 1 invocation of the method, and no invocation is using the return value. The case of 0 invocation is handled by S1144.
                if (matchingInvocations.Any() && !matchingInvocations.Any(IsReturnValueUsed))
                {
                    context.ReportIssue(CreateDiagnostic(Rule, declaredPrivateMethodWithReturn.Node.ReturnType.GetLocation()));
                }
            }
        }

        private static void AnalyzeLocalFunctionStatements(SonarSyntaxNodeReportingContext context)
        {
            var localFunctionSyntax = (LocalFunctionStatementSyntaxWrapper)context.Node;
            var topMostContainingMethod = localFunctionSyntax.IsTopLevel()
                                              ? context.Node.Parent.Parent // .Parent.Parent is the CompilationUnit
                                              : context.Node.GetTopMostContainingMethod();

            if (topMostContainingMethod == null)
            {
                return;
            }

            var localFunctionSymbol = (IMethodSymbol)context.SemanticModel.GetDeclaredSymbol(localFunctionSyntax);
            if (localFunctionSymbol.ReturnsVoid || localFunctionSymbol.IsAsync)
            {
                return;
            }

            var matchingInvocations = GetLocalMatchingInvocations(topMostContainingMethod, localFunctionSymbol, context.SemanticModel).ToList();
            // Method invocation is noncompliant when there is at least 1 invocation of the method, and no invocation is using the return value. The case of 0 invocation is handled by S1144.
            if (matchingInvocations.Any() && !matchingInvocations.Any(IsReturnValueUsed))
            {
                context.ReportIssue(CreateDiagnostic(Rule, localFunctionSyntax.ReturnType.GetLocation()));
            }
        }

        private static bool IsReturnValueUsed(NodeSymbolAndModel matchingInvocation) =>
            !IsExpressionStatement(matchingInvocation.Node.Parent)
            && !IsActionLambda(matchingInvocation.Node.Parent, matchingInvocation.Model);

        private static bool IsActionLambda(SyntaxNode node, SemanticModel semanticModel) =>
            node is LambdaExpressionSyntax lambda
            && semanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol { ReturnsVoid: true };

        private static bool IsExpressionStatement(SyntaxNode node) =>
            node is ExpressionStatementSyntax;

        private static IEnumerable<NodeSymbolAndModel> GetLocalMatchingInvocations(SyntaxNode containingMethod, IMethodSymbol invocationSymbol, SemanticModel semanticModel) =>
            containingMethod.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(x => semanticModel.GetSymbolInfo(x.Expression).Symbol is IMethodSymbol methodSymbol && invocationSymbol.Equals(methodSymbol))
                .Select(x => new NodeSymbolAndModel(semanticModel, x, invocationSymbol))
                .ToList();

        private static IEnumerable<NodeSymbolAndModel> FilterInvocations(NodeAndModel<BaseTypeDeclarationSyntax> container) =>
            container.Node.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(x => new NodeSymbolAndModel(container.Model, x, container.Model.GetSymbolInfo(x).Symbol as IMethodSymbol))
                .Where(x => x.Symbol != null);

        private static IEnumerable<NodeSymbolAndModel<MethodDeclarationSyntax, IMethodSymbol>> CollectRemovableMethods(CSharpRemovableDeclarationCollector removableDeclarationCollector) =>
                removableDeclarationCollector.TypeDeclarations
                    .SelectMany(container => container.Node.DescendantNodes(CSharpRemovableDeclarationCollector.IsNodeContainerTypeDeclaration)
                        .OfType<MethodDeclarationSyntax>()
                        .Select(x => new NodeSymbolAndModel<MethodDeclarationSyntax, IMethodSymbol>(container.Model, x, container.Model.GetDeclaredSymbol(x))))
                        .Where(x => x.Symbol is { ReturnsVoid: false, IsAsync: false } && CSharpRemovableDeclarationCollector.IsRemovable(x.Symbol, Accessibility.Private));
    }
}
