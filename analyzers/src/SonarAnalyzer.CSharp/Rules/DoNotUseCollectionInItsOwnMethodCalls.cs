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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseCollectionInItsOwnMethodCalls : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S2114";
    private const string MessageFormat = "Change one instance of '{0}' to a different value; {1}";
    private const string AlwaysEmptyCollectionMessage = "This operation always produces an empty collection.";
    private const string AlwaysSameCollectionMessage = "This operation always produces the same collection.";
    private const string AlwaysTrueMessage = "Comparing to itself always returns true.";
    private const string AlwaysFalseMessage = "Comparing to itself always returns false.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<string> TrackedMethodNames = new HashSet<string>
    {
        nameof(Enumerable.Except),
        nameof(ISet<object>.ExceptWith),
        nameof(Enumerable.Intersect),
        nameof(ISet<object>.IntersectWith),
        nameof(ISet<object>.IsSubsetOf),
        nameof(ISet<object>.IsSupersetOf),
        nameof(ISet<object>.IsProperSubsetOf),
        nameof(ISet<object>.IsProperSupersetOf),
        nameof(ISet<object>.Overlaps),
        nameof(Enumerable.SequenceEqual),
        nameof(ISet<object>.SetEquals),
        nameof(ISet<object>.SymmetricExceptWith),
        nameof(Enumerable.Union),
        nameof(ISet<object>.UnionWith)
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (invocation.GetMethodCallIdentifier() is { } identifier
                    && TrackedMethodNames.Any(x => identifier.ValueText.EndsWith(x, StringComparison.Ordinal))
                    && OperandsToCheckIfTrackedMethod(invocation, c.Model) is { } operands
                    && CSharpEquivalenceChecker.AreEquivalent(operands.Left, operands.Right))
                {
                    c.ReportIssue(Rule, operands.Left, [operands.Right.ToSecondaryLocation()], operands.Right.ToString(), operands.ErrorMessage);
                }
            },
            SyntaxKind.InvocationExpression);

    private static OperandsToCheck? OperandsToCheckIfTrackedMethod(InvocationExpressionSyntax invocation, SemanticModel model)
    {
        var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (ProcessIssueMessage(methodSymbol) is { } message)
        {
            if (methodSymbol is { IsExtensionMethod: true, MethodKind: MethodKind.Ordinary } && invocation is { ArgumentList.Arguments: { Count: >= 2 } extensionArgs })
            {
                return new(extensionArgs[0].Expression, extensionArgs[1].Expression, message);
            }
            else if (invocation is { Expression: MemberAccessExpressionSyntax { Expression: { } invokingExpression }, ArgumentList.Arguments: { Count: >= 1 } memberArgs })
            {
                return new(invokingExpression, memberArgs[0].Expression, message);
            }
        }
        return null;
    }

    private static string ProcessIssueMessage(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsEnumerableIntersect()
            || methodSymbol.IsEnumerableUnion()
            || IsAnySetMethod(methodSymbol, nameof(ISet<object>.UnionWith), nameof(ISet<object>.IntersectWith)))
        {
            return AlwaysSameCollectionMessage;
        }
        else if (methodSymbol.IsEnumerableSequenceEqual()
            || IsAnySetMethod(methodSymbol, nameof(ISet<object>.IsSubsetOf), nameof(ISet<object>.IsSupersetOf), nameof(ISet<object>.Overlaps), nameof(ISet<object>.SetEquals)))
        {
            return AlwaysTrueMessage;
        }
        else if (methodSymbol.IsEnumerableExcept()
            || IsAnySetMethod(methodSymbol, nameof(ISet<object>.ExceptWith), nameof(ISet<object>.SymmetricExceptWith)))
        {
            return AlwaysEmptyCollectionMessage;
        }
        else if (IsAnySetMethod(methodSymbol, nameof(ISet<object>.IsProperSubsetOf), nameof(ISet<object>.IsProperSupersetOf), nameof(ISet<object>.IsProperSupersetOf)))
        {
            return AlwaysFalseMessage;
        }
        return null;
    }

    private static bool IsAnySetMethod(IMethodSymbol methodSymbol, params string[] methodNames) =>
        methodSymbol is { MethodKind: MethodKind.Ordinary, Parameters.Length: 1 }
        && methodNames.Contains(methodSymbol.Name, StringComparer.Ordinal)
        && methodSymbol.ContainingType.Implements(KnownType.System_Collections_Generic_ISet_T);

    private readonly record struct OperandsToCheck
    {
        public ExpressionSyntax Left { get; }

        public ExpressionSyntax Right { get; }

        public string ErrorMessage { get; }

        public OperandsToCheck(ExpressionSyntax left, ExpressionSyntax right, string errorMessage)
        {
            Left = left.RemoveParentheses();
            Right = right.RemoveParentheses();
            ErrorMessage = errorMessage;
        }
    }
}
