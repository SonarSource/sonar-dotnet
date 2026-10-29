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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules;

// S9426: "await foreach" loops should use an available cancellation token.
public sealed partial class CancellationTokenShouldBeUsed
{
    internal const string DiagnosticIdS9426 = "S9426";
    private const string MessageFormatS9426 = "Pass the '{0}' to this 'await foreach' via 'WithCancellation' to allow cancellation of the operation.";
    private const string WithCancellation = "WithCancellation";
    private const string ToBlockingEnumerable = "ToBlockingEnumerable";

    internal static readonly DiagnosticDescriptor RuleS9426 = DescriptorFactory.Create(DiagnosticIdS9426, MessageFormatS9426);
    private static readonly string[] WithCancellationOrConfigureAwait = [WithCancellation, nameof(Task.ConfigureAwait), ToBlockingEnumerable];

    private static void AnalyzeAwaitForEach(SonarSyntaxNodeReportingContext context, CtSource ctSource, IMethodSymbol owningMethod)
    {
        if (CommonForEachStatementSyntaxWrapper.IsInstance(context.Node)
            && (CommonForEachStatementSyntaxWrapper)context.Node is { AwaitKeyword.RawKind: (int)SyntaxKind.AwaitKeyword, Expression: { } foreachExpression }
            && UnwrapConfiguredAsyncEnumerable(context.Model, (ExpressionSyntax)foreachExpression.RemoveParentheses()) is { } sourceExpression
            && !IsAlreadyCancellableViaEnumeratorCancellationAttribute(context.Model, sourceExpression)
            && context.Model.GetTypeInfo(sourceExpression, context.Cancel).Type is { } expressionType
            && (expressionType.Is(KnownType.System_Collections_Generic_IAsyncEnumerable_T) || expressionType.Implements(KnownType.System_Collections_Generic_IAsyncEnumerable_T)))
        {
            context.ReportIssue(
                RuleS9426,
                sourceExpression.GetIdentifier()?.GetLocation() ?? sourceExpression.GetLocation(),
                [ctSource.DeclarationToken?.ToSecondaryLocation()],
                messageArgs: ctSource.Expression.ToString());
        }
    }

    // Peels off chained WithCancellation/ConfigureAwait calls (either order) to reach the real source - see rspec Exceptions.
    // Null once WithCancellation is found - already excluded. Deliberately inline-only: a variable already holding
    // ConfiguredCancelableAsyncEnumerable<T> may carry a token applied upstream that we have no way to verify.
    private static ExpressionSyntax UnwrapConfiguredAsyncEnumerable(SemanticModel model, ExpressionSyntax expression)
    {
        while (expression is InvocationExpressionSyntax { Expression.LeftOfDot: { } receiver } invocation
            && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
            && IsConfiguredAsyncEnumerableCall(method))
        {
            if (TerminatesConfiguredChain(method, invocation))
            {
                return null;
            }
            expression = (ExpressionSyntax)receiver.RemoveParentheses();
        }
        return expression;
    }

    private static bool IsConfiguredAsyncEnumerableCall(IMethodSymbol method) =>
        method.IsAny(KnownType.System_Threading_Tasks_TaskAsyncEnumerableExtensions, WithCancellationOrConfigureAwait)
        || method.IsAny(KnownType.System_Runtime_CompilerServices_ConfiguredCancelableAsyncEnumerable_T, WithCancellationOrConfigureAwait);

    private static bool TerminatesConfiguredChain(IMethodSymbol method, InvocationExpressionSyntax invocation) =>
        (method.Name == WithCancellation && !IsDefaultArgument(invocation))
        || (method.Name == ToBlockingEnumerable && HasExplicitCancellationTokenArgument(invocation, method) && !IsDefaultArgument(invocation));

    private static bool HasExplicitCancellationTokenArgument(InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        var paramLookup = new CSharpMethodParameterLookup(invocation.ArgumentList, method);
        return invocation.ArgumentList.Arguments.Any(x =>
            paramLookup.TryGetSymbol(x, out var param) && param.Type.NullableUnderlyingTypeOrSelf().Is(KnownType.System_Threading_CancellationToken));
    }

    // CancellationToken.None is the established explicit opt-out (see S8949's own message); bare 'default' is not - it never
    // overrides an [EnumeratorCancellation] parameter, so it must not be treated as having wired anything.
    private static bool IsDefaultArgument(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments[0].Expression.RemoveParentheses() is { RawKind: (int)SyntaxKind.DefaultExpression or (int)SyntaxKindEx.DefaultLiteralExpression };

    // The compiler-generated GetAsyncEnumerator(CancellationToken) override keeps an already-supplied [EnumeratorCancellation]
    // argument even without WithCancellation - see rspec Exceptions. The producer call might be stored in a variable
    // before the loop rather than inlined - ResolveAssignedValue looks through that.
    private static bool IsAlreadyCancellableViaEnumeratorCancellationAttribute(SemanticModel model, ExpressionSyntax expression) =>
        ResolveAssignedValue(expression) is InvocationExpressionSyntax invocation
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
        && method.Parameters.FirstOrDefault(x => x.HasAttribute(KnownType.System_Runtime_CompilerServices_EnumeratorCancellationAttribute)) is { } cancellationParameter
        && new CSharpMethodParameterLookup(invocation.ArgumentList, method).TryGetNonParamsSyntax(cancellationParameter, out _);

    // Walks back through simple, unambiguous reassignments/declarations only - bails out (stops) on loops or branches
    // where the value could differ, so this never turns a real issue into a missed one with any confidence loss.
    private static ExpressionSyntax ResolveAssignedValue(ExpressionSyntax expression)
    {
        while (expression is IdentifierNameSyntax identifier
            && CSharpFacade.Instance.AssignmentFinder.FindLinearPrecedingAssignment(identifier.Identifier.ValueText, identifier) is PrecedingAssignment.Found { Assignment: ExpressionSyntax resolved })
        {
            expression = (ExpressionSyntax)resolved.RemoveParentheses();
        }
        return expression;
    }
}
