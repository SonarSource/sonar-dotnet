/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Collections.Concurrent;

namespace SonarAnalyzer.CSharp.Rules;

// S8949: the overload accepting a CancellationToken should be used instead of an overload that doesn't.
public sealed partial class CancellationTokenShouldBeUsed
{
    private readonly record struct MethodGroupKey(ITypeSymbol Type, string Name);

    internal const string DiagnosticIdS8949 = "S8949";
    private const string MessageFormatS8949 = "Pass the '{0}' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.";
    private const string MessageFormatS8949Default = "Pass the '{0}' instead of 'default' to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.";

    internal static readonly DiagnosticDescriptor RuleS8949 = DescriptorFactory.Create(DiagnosticIdS8949, "{0}");
    // Reused across calls: each call's ReplaceNode produces its own private tree, so there's no cross-call collision to guard against.
    private static readonly SyntaxAnnotation SpeculativeInvocationAnnotation = new();

    private static void AnalyzeInvocation(
        SonarSyntaxNodeReportingContext nodeContext,
        CtSource ctSource,
        IMethodSymbol owningMethod,
        ConcurrentDictionary<MethodGroupKey, ImmutableArray<string>> candidateCache)
    {
        var invocation = (InvocationExpressionSyntax)nodeContext.Node;
        if (nodeContext.Model.GetSymbolInfo(invocation, nodeContext.Cancel).Symbol is IMethodSymbol method
            && !IsAlreadyConfiguredWithCancellation(nodeContext.Model, invocation, method, nodeContext.Cancel))
        {
            if (HasCancellationTokenArgument(nodeContext, invocation, method, ctSource))
            {
                return;
            }

            var searchType = (method.ReducedFrom ?? method).ContainingType;
            var ctParamNames = candidateCache.GetOrAdd(new MethodGroupKey(searchType, method.Name), x =>
                x.Type
                    .GetMembers(x.Name)
                    .OfType<IMethodSymbol>()
                    .Where(x => !x.IsHiddenFromIntelliSense)
                    .Select(x => x.Parameters.FirstOrDefault(p => p.Type.NullableUnderlyingTypeOrSelf().Is(KnownType.System_Threading_CancellationToken))?.Name)
                    .WhereNotNull()
                    .Distinct()
                    .ToImmutableArray());

            if (!ctParamNames.IsDefaultOrEmpty && ctParamNames.Any(x => CanSpeculativelyPassCt(nodeContext.Model, invocation, method, x, ctSource.Expression, owningMethod)))
            {
                ReportIssue(nodeContext, invocation, ctSource, MessageFormatS8949);
            }
        }
    }

    // An arg bound to a CT parameter means we are already on the CT overload - reports only if it's the 'default' sentinel.
    // Speculative binding would produce a duplicate CT arg and fail; handle directly. ParameterLookup resolves named
    // and reordered args correctly; params CT[] is skipped because its element type (CancellationToken[]) does not match CT.
    private static bool HasCancellationTokenArgument(SonarSyntaxNodeReportingContext nodeContext, InvocationExpressionSyntax invocation, IMethodSymbol method, CtSource ctSource)
    {
        var paramLookup = new CSharpMethodParameterLookup(invocation.ArgumentList, method);
        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            if (paramLookup.TryGetSymbol(arg, out var param) && param.Type.NullableUnderlyingTypeOrSelf().Is(KnownType.System_Threading_CancellationToken))
            {
                if (arg.Expression.RawKind is (int)SyntaxKind.DefaultExpression or (int)SyntaxKindEx.DefaultLiteralExpression)
                {
                    ReportIssue(nodeContext, invocation, ctSource, MessageFormatS8949Default);
                }
                return true;
            }
        }
        return false;
    }

    private static void ReportIssue(SonarSyntaxNodeReportingContext nodeContext, InvocationExpressionSyntax invocation, CtSource ctSource, string messageFormat) =>
        nodeContext.ReportIssue(
            RuleS8949,
            invocation.MethodCallIdentifier?.GetLocation() ?? invocation.GetLocation(),
            [ctSource.DeclarationToken?.ToSecondaryLocation()],
            string.Format(messageFormat, ctSource.Expression));

    // Already configured via .WithCancellation(...)/.ConfigureAwait(...)/.ToBlockingEnumerable(...) further down the chain, only if every CT parameter is attributed - see S9426's AnalyzeAwaitForEach.
    private static bool IsAlreadyConfiguredWithCancellation(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol method, CancellationToken cancel)
    {
        var ctParameters = method.Parameters.Where(x => x.Type.NullableUnderlyingTypeOrSelf().Is(KnownType.System_Threading_CancellationToken)).ToList();
        if (ctParameters.Count == 0 || ctParameters.Any(x => !x.HasAttribute(KnownType.System_Runtime_CompilerServices_EnumeratorCancellationAttribute)))
        {
            return false;
        }

        var current = invocation;
        while (OuterInvocation(current) is { } outerInvocation
            && model.GetSymbolInfo(outerInvocation, cancel).Symbol is IMethodSymbol outerMethod
            && IsConfiguredAsyncEnumerableCall(outerMethod))
        {
            if (TerminatesConfiguredChain(outerMethod, outerInvocation))
            {
                return true;
            }
            current = outerInvocation;
        }
        return false;

        // Misses longer '?.' chains, '!', and parenthesized receivers - should be replaced by a proper RightOfDot once NET-4740 is resolved.
        static InvocationExpressionSyntax OuterInvocation(ExpressionSyntax expression) =>
            expression.Parent switch
            {
                MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax outer } when outer.Expression.LeftOfDot == expression => outer,
                ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax outer } when outer.Expression.LeftOfDot == expression => outer,
                _ => null,
            };
    }

    private static bool CanSpeculativelyPassCt(
        SemanticModel model,
        InvocationExpressionSyntax invocation,
        IMethodSymbol calledMethod,
        string ctParamName,
        ExpressionSyntax ctSource,
        IMethodSymbol owningMethod)
    {
        var ctArg = SyntaxFactory.Argument(
            SyntaxFactory.NameColon(ctParamName.EscapedIdentifierName),
            default,
            ctSource);
        var modifiedInvocation = invocation.WithArgumentList(invocation.ArgumentList.AddArguments(ctArg)).WithAdditionalAnnotations(SpeculativeInvocationAnnotation);
        // A detached WithArgumentList() copy has Parent == null, which NREs Roslyn's binder on conditional-access
        // chains like x?.y.M() (https://github.com/dotnet/roslyn/issues/25262). ReplaceNode on the tree root keeps the parent chain intact.
        var speculatedNode = (ExpressionSyntax)invocation.SyntaxTree.GetRoot().ReplaceNode(invocation, modifiedInvocation).GetAnnotatedNodes(SpeculativeInvocationAnnotation).First();
        // Speculative binding also succeeds inside a static local function. Check the return type compatability too to avoid FPs (e.g. Task<int> vs Task<string>).
        // If the token-forwarded call now binds back to the owning method, we risk introducing endless recursion - unless it already recursed there before.
        return model.GetSpeculativeSymbolInfo(invocation.SpanStart, speculatedNode, SpeculativeBindingOption.BindAsExpression).Symbol
            is IMethodSymbol resolved
            && !(resolved.ReducedFrom ?? resolved).IsHiddenFromIntelliSense
            && model.Compilation.ClassifyConversion(resolved.ReturnType, calledMethod.ReturnType).IsImplicit
            && (!IsOwningMethod(resolved) || IsOwningMethod(calledMethod));

        bool IsOwningMethod(IMethodSymbol symbol) => (symbol.ReducedFrom ?? symbol).OriginalDefinition.Equals(owningMethod.OriginalDefinition);
    }
}
