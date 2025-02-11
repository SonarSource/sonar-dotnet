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
public sealed class RedundantCast : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S1905";
    private const string MessageFormat = "Remove this unnecessary cast to '{0}'.";

    private static readonly DiagnosticDescriptor Rule =
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static readonly ISet<string> CastIEnumerableMethods = new HashSet<string> { "Cast", "OfType" };

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var castExpression = (CastExpressionSyntax)c.Node;
                CheckCastExpression(c, castExpression.Expression, castExpression.Type, castExpression.Type.GetLocation());
            },
            SyntaxKind.CastExpression);

        context.RegisterNodeAction(
            c =>
            {
                var castExpression = (BinaryExpressionSyntax)c.Node;
                CheckCastExpression(c, castExpression.Left, castExpression.Right,
                    castExpression.OperatorToken.CreateLocation(castExpression.Right));
            },
            SyntaxKind.AsExpression);

        context.RegisterNodeAction(
            CheckExtensionMethodInvocation,
            SyntaxKind.InvocationExpression);
    }

    private static void CheckCastExpression(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression, ExpressionSyntax type, Location location)
    {
        if (!expression.IsKind(SyntaxKindEx.DefaultLiteralExpression)
            && context.Model.GetTypeInfo(expression) is { Type: { } expressionType } expressionTypeInfo
            && context.Model.GetTypeInfo(type) is { Type: { } castType }
            && expressionType.Equals(castType)
            && FlowStateEquals(expressionTypeInfo, type))
        {
            ReportIssue(context, expression, castType, location);
        }
    }

    private static bool FlowStateEquals(TypeInfo expressionTypeInfo, ExpressionSyntax type)
    {
        var castingToNullable = type.IsKind(SyntaxKind.NullableType); // The Nullability() annotation of the cast target type can not be found on the symbol
        return expressionTypeInfo.Nullability().FlowState switch
        {
            NullableFlowState.None => true,
            NullableFlowState.MaybeNull => castingToNullable, // false when (string)maybeNull which is a narrowing cast and therefore not redundant
            NullableFlowState.NotNull => !castingToNullable,  // false when (string?)nonNull which is a widening cast but not redundant in cases like tuples (a: (string?)"", b: "")
            _ => true,
        };
    }

    private static void CheckExtensionMethodInvocation(SonarSyntaxNodeReportingContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (GetEnumerableExtensionSymbol(invocation, context.Model) is { } methodSymbol)
        {
            var returnType = methodSymbol.ReturnType;
            if (GetGenericTypeArgument(returnType) is { } castType)
            {
                if (methodSymbol.Name == "OfType" && CanHaveNullValue(castType))
                {
                    // OfType() filters 'null' values from enumerables
                    return;
                }

                var elementType = GetElementType(invocation, methodSymbol, context.Model);
                if (elementType != null && elementType.Equals(castType) && elementType.NullableAnnotation() == castType.NullableAnnotation())
                {
                    var methodCalledAsStatic = methodSymbol.MethodKind == MethodKind.Ordinary;
                    ReportIssue(context, invocation, returnType, GetReportLocation(invocation, methodCalledAsStatic));
                }
            }
        }
    }

    private static void ReportIssue(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression, ITypeSymbol castType, Location location) =>
        context.ReportIssue(Rule, location, castType.ToMinimalDisplayString(context.Model, expression.SpanStart));

    /// If the invocation one of the <see cref="CastIEnumerableMethods"/> extensions, returns the method symbol.
    private static IMethodSymbol GetEnumerableExtensionSymbol(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
        invocation.GetMethodCallIdentifier() is { } methodName
        && CastIEnumerableMethods.Contains(methodName.ValueText)
        && semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
        && methodSymbol.IsExtensionOn(KnownType.System_Collections_IEnumerable)
            ? methodSymbol
            : null;

    private static ITypeSymbol GetGenericTypeArgument(ITypeSymbol type) =>
        type is INamedTypeSymbol returnType && returnType.Is(KnownType.System_Collections_Generic_IEnumerable_T)
            ? returnType.TypeArguments.Single()
            : null;

    private static bool CanHaveNullValue(ITypeSymbol type) =>
        type.IsReferenceType || type.Is(KnownType.System_Nullable_T);

    private static Location GetReportLocation(InvocationExpressionSyntax invocation, bool methodCalledAsStatic) =>
        methodCalledAsStatic is false && invocation.Expression is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.OperatorToken.CreateLocation(invocation)
            : invocation.Expression.GetLocation();

    private static ITypeSymbol GetElementType(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        return semanticModel.GetTypeInfo(CollectionExpression(invocation, methodSymbol)).Type switch
        {
            INamedTypeSymbol { TypeArguments: { Length: 1 } typeArguments } => typeArguments.First(),
            IArrayTypeSymbol { Rank: 1 } arrayType => arrayType.ElementType,    // casting is necessary for multidimensional arrays
            _ => null
        };

        static ExpressionSyntax CollectionExpression(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) =>
            methodSymbol.MethodKind is MethodKind.ReducedExtension
                ? ReducedExtensionExpression(invocation)
                : invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;

        static ExpressionSyntax ReducedExtensionExpression(InvocationExpressionSyntax invocation) =>
            invocation.Expression is MemberAccessExpressionSyntax { Expression: { } memberAccessExpression }
                ? memberAccessExpression
                : invocation.GetParentConditionalAccessExpression()?.Expression;
    }
}
