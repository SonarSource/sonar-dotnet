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

using Microsoft.CodeAnalysis.Shared.Extensions;
using WellKnownExtensionMethodContainer = SonarAnalyzer.Common.MultiValueDictionary<Microsoft.CodeAnalysis.ITypeSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol>;
namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAwaitableMethod : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6966";
    private const string MessageFormat = "Await {0} instead.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            // Not every async method is defined in the same class/interface as its non-async counterpart.
            // For example the EntityFrameworkQueryableExtensions.AnyAsync() method provides an async version of the Enumerable.Any() method for IQueryable types.
            // WellKnownExtensionMethodContainer stores where to look for the async versions of certain methods from a type, e.g. async versions of methods from
            // System.Linq.Enumerable can be found in Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.
            var wellKnownExtensionMethodContainer = BuildWellKnownExtensionMethodContainers(compilationStart.Compilation);
            context.RegisterCodeBlockStartAction<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, codeBlockStart =>
            {
                if (IsAsyncCodeBlock(codeBlockStart.CodeBlock))
                {
                    codeBlockStart.RegisterNodeAction(nodeContext =>
                    {
                        var invocationExpression = (InvocationExpressionSyntax)nodeContext.Node;

                        var awaitableAlternatives = FindAwaitableAlternatives(wellKnownExtensionMethodContainer, invocationExpression,
                            nodeContext.SemanticModel, nodeContext.ContainingSymbol, nodeContext.Cancel);
                        if (awaitableAlternatives.FirstOrDefault() is { Name: { } alternative })
                        {
                            nodeContext.ReportIssue(Rule, invocationExpression, alternative);
                        }
                    }, SyntaxKind.InvocationExpression);
                }
            });
        });

    private static WellKnownExtensionMethodContainer BuildWellKnownExtensionMethodContainers(Compilation compilation)
    {
        var wellKnownExtensionMethodContainer = new WellKnownExtensionMethodContainer();
        var queryable = compilation.GetTypeByMetadataName(KnownType.System_Linq_Queryable);
        var enumerable = compilation.GetTypeByMetadataName(KnownType.System_Linq_Enumerable);
        if (queryable is not null && enumerable is not null)
        {
            if (compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFrameworkCore_EntityFrameworkQueryableExtensions) is { } entityFrameworkQueryableExtensions)
            {
                wellKnownExtensionMethodContainer.Add(queryable, entityFrameworkQueryableExtensions);
                wellKnownExtensionMethodContainer.Add(enumerable, entityFrameworkQueryableExtensions);
            }
            if (compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFrameworkCore_RelationalQueryableExtensions) is { } relationalQueryableExtensions)
            {
                wellKnownExtensionMethodContainer.Add(queryable, relationalQueryableExtensions);
                wellKnownExtensionMethodContainer.Add(enumerable, relationalQueryableExtensions);
            }
        }
        if (compilation.GetTypeByMetadataName(KnownType.System_Net_Sockets_Socket) is { } socket
            && compilation.GetTypeByMetadataName(KnownType.System_Net_Sockets_SocketTaskExtensions) is { } socketTaskExtensions)
        {
            wellKnownExtensionMethodContainer.Add(socket, socketTaskExtensions);
        }
        return wellKnownExtensionMethodContainer;
    }

    private static ImmutableArray<ISymbol> FindAwaitableAlternatives(WellKnownExtensionMethodContainer wellKnownExtensionMethodContainer,
        InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, ISymbol containingSymbol, CancellationToken cancel)
    {
        var awaitableRoot = GetAwaitableRootOfInvocation(invocationExpression);
        if (awaitableRoot is not { Parent: AwaitExpressionSyntax } // Invocation result is already awaited.
            && invocationExpression.EnclosingScope() is { } scope
            && IsAsyncCodeBlock(scope)
            && semanticModel.GetSymbolInfo(invocationExpression, cancel).Symbol is IMethodSymbol { MethodKind: not MethodKind.DelegateInvoke } methodSymbol
            && !methodSymbol.IsAwaitableNonDynamic()) // The invoked method returns something awaitable (but it isn't awaited).
        {
            // Perf: Before doing (expensive) speculative re-binding in SpeculativeBindCandidates, we check if there is an "..Async()" alternative in scope.
            var invokedType = invocationExpression.Expression.GetLeftOfDot() is { } expression && semanticModel.GetTypeInfo(expression) is { Type: { } type }
                ? type // A dotted expression: Lookup the type, left of the dot (this may be different from methodSymbol.ContainingType)
                : containingSymbol.ContainingType; // If not dotted, than the scope is the current type. Local function support is missing here.
            var members = GetMethodSymbolsInScope($"{methodSymbol.Name}Async", wellKnownExtensionMethodContainer, invokedType, methodSymbol.ContainingType);
            var awaitableCandidates = members.Where(x => x.IsAwaitableNonDynamic());
            // Get the method alternatives and exclude candidates that would resolve to the containing method (endless loop)
            var awaitableAlternatives = SpeculativeBindCandidates(semanticModel, awaitableRoot, invocationExpression, awaitableCandidates)
                .Where(x => !containingSymbol.Equals(x))
                .ToImmutableArray();
            return awaitableAlternatives;
        }
        return ImmutableArray<ISymbol>.Empty;
    }

    private static IEnumerable<IMethodSymbol> GetMethodSymbolsInScope(string methodName, WellKnownExtensionMethodContainer wellKnownExtensionMethodContainer,
        ITypeSymbol invokedType, ITypeSymbol methodContainer) =>
        ((ITypeSymbol[])[.. invokedType.GetSelfAndBaseTypes(), .. WellKnownExtensionMethodContainer(wellKnownExtensionMethodContainer, methodContainer), methodContainer])
            .Distinct()
            .SelectMany(x => x.GetMembers(methodName))
            .OfType<IMethodSymbol>()
            .Where(x => !x.HasAttribute(KnownType.System_ObsoleteAttribute));

    private static IEnumerable<INamedTypeSymbol> WellKnownExtensionMethodContainer(WellKnownExtensionMethodContainer lookup, ITypeSymbol invokedType) =>
        lookup.TryGetValue(invokedType, out var extensionMethodContainer)
            ? extensionMethodContainer
            : [];

    private static IEnumerable<ISymbol> SpeculativeBindCandidates(SemanticModel semanticModel, SyntaxNode awaitableRoot,
        InvocationExpressionSyntax invocationExpression, IEnumerable<IMethodSymbol> awaitableCandidates) =>
        awaitableCandidates
            .Select(x => x.Name)
            .Distinct()
            .Select(x => SpeculativeBindCandidate(semanticModel, x, awaitableRoot, invocationExpression))
            .WhereNotNull();

    private static IMethodSymbol SpeculativeBindCandidate(SemanticModel semanticModel, string candidateName, SyntaxNode awaitableRoot,
        InvocationExpressionSyntax invocationExpression)
    {
        var invocationIdentifierName = invocationExpression.GetMethodCallIdentifier()?.Parent;
        if (invocationIdentifierName is null)
        {
            return null;
        }
        var invocationReplaced = ReplaceInvocation(awaitableRoot, invocationExpression, invocationIdentifierName, candidateName);
        var speculativeSymbolInfo = semanticModel.GetSpeculativeSymbolInfo(invocationReplaced.SpanStart, invocationReplaced, SpeculativeBindingOption.BindAsExpression);
        var speculativeSymbol = speculativeSymbolInfo.Symbol as IMethodSymbol;
        return speculativeSymbol;
    }

    private static SyntaxNode ReplaceInvocation(SyntaxNode awaitableRoot, InvocationExpressionSyntax invocationExpression, SyntaxNode invocationIdentifierName, string candidateName)
    {
        var root = invocationExpression.SyntaxTree.GetRoot();
        var invocationAnnotation = new SyntaxAnnotation();
        var replace = root.ReplaceNodes([awaitableRoot, invocationIdentifierName, invocationExpression], (original, newNode) =>
        {
            var result = newNode;
            if (original == invocationIdentifierName)
            {
                var newIdentifierToken = SyntaxFactory.Identifier(candidateName);
                var simpleName = invocationIdentifierName switch
                {
                    IdentifierNameSyntax => (SimpleNameSyntax)SyntaxFactory.IdentifierName(newIdentifierToken),
                    GenericNameSyntax { TypeArgumentList: { } typeArguments } => SyntaxFactory.GenericName(newIdentifierToken, typeArguments),
                    _ => null,
                };
                result = simpleName is null ? newNode : simpleName.WithTriviaFrom(invocationIdentifierName);
            }
            if (original == invocationExpression)
            {
                result = result.WithAdditionalAnnotations(invocationAnnotation);
            }
            if (original == awaitableRoot && result is ExpressionSyntax resultExpression)
            {
                result = SyntaxFactory.ParenthesizedExpression(
                    SyntaxFactory.AwaitExpression(resultExpression.WithoutTrivia().WithLeadingTrivia(SyntaxFactory.ElasticSpace))).WithTriviaFrom(resultExpression);
            }
            return result;
        });
        return replace.GetAnnotatedNodes(invocationAnnotation).First();
    }

    private static ExpressionSyntax GetAwaitableRootOfInvocation(ExpressionSyntax expression) =>
        expression switch
        {
            { Parent: ConditionalAccessExpressionSyntax conditional } => conditional.GetRootConditionalAccessExpression(),
            { Parent: MemberAccessExpressionSyntax memberAccess } => memberAccess.GetRootConditionalAccessExpression() ?? GetAwaitableRootOfInvocation(memberAccess),
            { Parent: PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression } parent } => GetAwaitableRootOfInvocation(parent),
            { Parent: ParenthesizedExpressionSyntax parent } => GetAwaitableRootOfInvocation(parent),
            { } self => self,
        };

    private static bool IsAsyncCodeBlock(SyntaxNode codeBlock) =>
        codeBlock switch
        {
            CompilationUnitSyntax => true,
            BaseMethodDeclarationSyntax { Modifiers: { } modifiers } => modifiers.AnyOfKind(SyntaxKind.AsyncKeyword),
            AnonymousFunctionExpressionSyntax { AsyncKeyword: { } asyncKeyword } => asyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            var localFunction when LocalFunctionStatementSyntaxWrapper.IsInstance(localFunction) => ((LocalFunctionStatementSyntaxWrapper)localFunction).Modifiers.AnyOfKind(SyntaxKind.AsyncKeyword),
            _ => false,
        };
}
