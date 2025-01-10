/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Shared.Extensions;
using WellKnownExtensionMethodContainer = SonarAnalyzer.Common.MultiValueDictionary<Microsoft.CodeAnalysis.ITypeSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol>;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAwaitableMethod : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6966";
    private const string MessageFormat = "Await {0} instead.";
    private static readonly string[] ExcludedMethodNames = ["Add", "AddRange"];
    private static readonly ImmutableArray<KnownType> ExcludedTypes = ImmutableArray.Create(KnownType.System_Xml_XmlWriter, KnownType.System_Xml_XmlReader);

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
            var exclusions = BuildExclusions(compilationStart.Compilation);
            compilationStart.RegisterCodeBlockStartAction<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, codeBlockStart =>
            {
                if (IsAsyncCodeBlock(codeBlockStart.CodeBlock))
                {
                    codeBlockStart.RegisterNodeAction(nodeContext =>
                    {
                        var invocationExpression = (InvocationExpressionSyntax)nodeContext.Node;

                        var awaitableAlternatives = FindAwaitableAlternatives(wellKnownExtensionMethodContainer, exclusions, invocationExpression,
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

    private static ImmutableArray<Func<IMethodSymbol, bool>> BuildExclusions(Compilation compilation)
    {
        var exclusions = ImmutableArray.CreateBuilder<Func<IMethodSymbol, bool>>();
        if (compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFrameworkCore_DbSet_TEntity) is not null)
        {
            exclusions.Add(x => x.IsAny(KnownType.Microsoft_EntityFrameworkCore_DbSet_TEntity, ExcludedMethodNames)); // https://github.com/SonarSource/sonar-dotnet/issues/9269
            exclusions.Add(x => x.IsAny(KnownType.Microsoft_EntityFrameworkCore_DbContext, ExcludedMethodNames));     // https://github.com/SonarSource/sonar-dotnet/issues/9269
            // https://github.com/SonarSource/sonar-dotnet/issues/9590
            exclusions.Add(x => x.IsImplementingInterfaceMember(KnownType.Microsoft_EntityFrameworkCore_IDbContextFactory_TContext, "CreateDbContext"));
        }
        if (compilation.GetTypeByMetadataName(KnownType.FluentValidation_IValidator) is not null)
        {
            exclusions.Add(x => x.IsImplementingInterfaceMember(KnownType.FluentValidation_IValidator, "Validate"));   // https://github.com/SonarSource/sonar-dotnet/issues/9339
            exclusions.Add(x => x.IsImplementingInterfaceMember(KnownType.FluentValidation_IValidator_T, "Validate")); // https://github.com/SonarSource/sonar-dotnet/issues/9339
        }
        if (compilation.GetTypeByMetadataName(KnownType.MongoDB_Driver_IMongoCollectionExtensions) is not null)
        {
            exclusions.Add(x => x.Is(KnownType.MongoDB_Driver_IMongoCollectionExtensions, "Find")); // https://github.com/SonarSource/sonar-dotnet/issues/9265
        }
        return exclusions.ToImmutableArray();
    }

    private static ImmutableArray<ISymbol> FindAwaitableAlternatives(WellKnownExtensionMethodContainer wellKnownExtensionMethodContainer, ImmutableArray<Func<IMethodSymbol, bool>> exclusions,
        InvocationExpressionSyntax invocationExpression, SemanticModel model, ISymbol containingSymbol, CancellationToken cancel)
    {
        var awaitableRoot = GetAwaitableRootOfInvocation(invocationExpression);
        if (awaitableRoot is not { Parent: AwaitExpressionSyntax } // Invocation result is already awaited.
            && invocationExpression.EnclosingScope() is { } scope
            && IsAsyncCodeBlock(scope)
            && model.GetSymbolInfo(invocationExpression, cancel).Symbol is IMethodSymbol { MethodKind: not MethodKind.DelegateInvoke } methodSymbol
            && !(methodSymbol.IsAwaitableNonDynamic()  // The invoked method returns something awaitable (but it isn't awaited).
                || methodSymbol.ContainingType.DerivesFromAny(ExcludedTypes))
            && !exclusions.Any(x => x(methodSymbol)))
        {
            // Perf: Before doing (expensive) speculative re-binding in SpeculativeBindCandidates, we check if there is an "..Async()" alternative in scope.
            var invokedType = invocationExpression.Expression.GetLeftOfDot() is { } expression && model.GetTypeInfo(expression) is { Type: { } type }
                ? type // A dotted expression: Lookup the type, left of the dot (this may be different from methodSymbol.ContainingType)
                : containingSymbol.ContainingType; // If not dotted, than the scope is the current type. Local function support is missing here.
            var members = GetMethodSymbolsInScope($"{methodSymbol.Name}Async", wellKnownExtensionMethodContainer, invokedType, methodSymbol.ContainingType);
            var awaitableCandidates = members.Where(x => x.IsAwaitableNonDynamic());
            // Get the method alternatives and exclude candidates that would resolve to the containing method (endless loop)
            var awaitableAlternatives = SpeculativeBindCandidates(model, awaitableRoot, invocationExpression, awaitableCandidates)
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

    private static IEnumerable<ISymbol> SpeculativeBindCandidates(SemanticModel model, SyntaxNode awaitableRoot,
        InvocationExpressionSyntax invocationExpression, IEnumerable<IMethodSymbol> awaitableCandidates) =>
        awaitableCandidates
            .Select(x => x.Name)
            .Distinct()
            .Select(x => SpeculativeBindCandidate(model, x, awaitableRoot, invocationExpression))
            .WhereNotNull();

    private static IMethodSymbol SpeculativeBindCandidate(SemanticModel model, string candidateName, SyntaxNode awaitableRoot,
        InvocationExpressionSyntax invocationExpression)
    {
        var invocationIdentifierName = invocationExpression.GetMethodCallIdentifier()?.Parent;
        if (invocationIdentifierName is null)
        {
            return null;
        }
        var invocationReplaced = ReplaceInvocation(awaitableRoot, invocationExpression, invocationIdentifierName, candidateName);
        var speculativeSymbolInfo = model.GetSpeculativeSymbolInfo(invocationReplaced.SpanStart, invocationReplaced, SpeculativeBindingOption.BindAsExpression);
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
