/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
public sealed class ReturnValueIgnored : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2201";
    private const string MessageFormat = "Use the return value of method '{0}'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> ImmutableKnownTypes =
        ImmutableArray.Create(
            KnownType.System_Object,
            KnownType.System_Int16,
            KnownType.System_Int32,
            KnownType.System_Int64,
            KnownType.System_UInt16,
            KnownType.System_UInt32,
            KnownType.System_UInt64,
            KnownType.System_IntPtr,
            KnownType.System_UIntPtr,
            KnownType.System_Char,
            KnownType.System_Byte,
            KnownType.System_SByte,
            KnownType.System_Single,
            KnownType.System_Double,
            KnownType.System_Decimal,
            KnownType.System_Boolean,
            KnownType.System_String,
            KnownType.System_Collections_Frozen_FrozenSet,
            KnownType.System_Collections_Immutable_ImmutableArray,
            KnownType.System_Collections_Immutable_ImmutableArray_T,
            KnownType.System_Collections_Immutable_ImmutableDictionary,
            KnownType.System_Collections_Immutable_ImmutableDictionary_TKey_TValue,
            KnownType.System_Collections_Immutable_ImmutableHashSet,
            KnownType.System_Collections_Immutable_ImmutableHashSet_T,
            KnownType.System_Collections_Immutable_ImmutableList,
            KnownType.System_Collections_Immutable_ImmutableList_T,
            KnownType.System_Collections_Immutable_ImmutableQueue,
            KnownType.System_Collections_Immutable_ImmutableQueue_T,
            KnownType.System_Collections_Immutable_ImmutableSortedDictionary,
            KnownType.System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue,
            KnownType.System_Collections_Immutable_ImmutableSortedSet,
            KnownType.System_Collections_Immutable_ImmutableSortedSet_T,
            KnownType.System_Collections_Immutable_ImmutableStack,
            KnownType.System_Collections_Immutable_ImmutableStack_T);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var expressionStatement = (ExpressionStatementSyntax)c.Node;
                CheckExpressionForPureMethod(c, expressionStatement.Expression);
            },
            SyntaxKind.ExpressionStatement);

        context.RegisterNodeAction(
            c =>
            {
                var lambda = (LambdaExpressionSyntax)c.Node;

                if (c.Model.GetSymbolInfo(lambda).Symbol is not IMethodSymbol { ReturnsVoid: true } symbol)
                {
                    return;
                }

                var expression = lambda.Body as ExpressionSyntax;
                CheckExpressionForPureMethod(c, expression);
            },
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.SimpleLambdaExpression);
    }

    private static void CheckExpressionForPureMethod(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression)
    {
        if (expression is InvocationExpressionSyntax invocation
            && context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { ReturnsVoid: false } invokedMethodSymbol
            && invokedMethodSymbol.Parameters.All(p => p.RefKind == RefKind.None)
            && IsSideEffectFreeOrPure(invokedMethodSymbol))
        {
            context.ReportIssue(Rule, expression, invokedMethodSymbol.Name);
        }
    }

    private static bool IsSideEffectFreeOrPure(IMethodSymbol invokedMethodSymbol)
    {
        var constructedFrom = invokedMethodSymbol.ContainingType.ConstructedFrom;

        return IsLinqMethod(invokedMethodSymbol)
            || HasOnlySideEffectFreeMethods(constructedFrom)
            || IsPureMethod(invokedMethodSymbol, constructedFrom);
    }

    private static bool IsPureMethod(IMethodSymbol invokedMethodSymbol, INamedTypeSymbol containingType) =>
        invokedMethodSymbol.HasAttribute(KnownType.System_Diagnostics_Contracts_PureAttribute)
        || containingType.HasAttribute(KnownType.System_Diagnostics_Contracts_PureAttribute);

    private static bool HasOnlySideEffectFreeMethods(INamedTypeSymbol containingType) =>
        containingType.IsAny(ImmutableKnownTypes);

    private static bool IsLinqMethod(IMethodSymbol methodSymbol) =>
        methodSymbol.ContainingType.Is(KnownType.System_Linq_Enumerable)
        || methodSymbol.ContainingType.Is(KnownType.System_Linq_ImmutableArrayExtensions);
}
