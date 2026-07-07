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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CollectionQuerySimplification : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S2971";
    internal const string MessageUseInstead = "Use {0} here instead.";
    internal const string MessageDropAndChange = "Drop '{0}' and move the condition into the '{1}'.";
    internal const string MessageDropFromMiddle = "Drop this useless call to '{0}'.";
    private const string MessageFormat = "{0}";
    private const string WhereMethodName = "Where";
    private const int WherePredicateTypeArgumentNumber = 2;
    private const string SelectMethodName = "Select";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly HashSet<string> MethodNamesWithPredicate =
    [
        "Any", "LongCount", "Count",
        "First", "FirstOrDefault", "Last", "LastOrDefault",
        "Single", "SingleOrDefault",
    ];

    private static readonly HashSet<string> MethodNamesForTypeCheckingWithSelect =
    [
        "Any", "LongCount", "Count",
        "First", "FirstOrDefault", "Last", "LastOrDefault",
        "Single", "SingleOrDefault", "SkipWhile", "TakeWhile",
    ];

    private static readonly HashSet<string> MethodNamesToCollection =
    [
        "ToList",
        "ToArray",
    ];

    private static readonly HashSet<string> IgnoredMethodNames =
    [
        "AsEnumerable", // ignored as it is somewhat cleaner way to cast to IEnumerable<T> and has no side effects
    ];

    private static readonly HashSet<SyntaxKind> AsIsSyntaxKinds =
    [
        SyntaxKind.AsExpression,
        SyntaxKind.IsExpression
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(CheckExtensionMethodsOnIEnumerable, SyntaxKind.InvocationExpression);
        context.RegisterNodeAction(CheckToCollectionCalls, SyntaxKind.InvocationExpression);
        context.RegisterNodeAction(CheckCountCall, SyntaxKind.InvocationExpression);
    }

    private static void CheckCountCall(SonarSyntaxNodeReportingContext context)
    {
        const string CountName = "Count";

        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation is
            {
                ArgumentList.Arguments.Count: 0,
                Expression: MemberAccessExpressionSyntax
                {
                    Name.Identifier.ValueText: CountName,
                    Expression: { } memberAccessExpression
                }
            }
            && context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: CountName } methodSymbol
            // Only IEnumerable<T>: a custom IQueryable provider may expose a '.Count' property so 'Count()' on an IQueryable must not be flagged.
            && methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            && HasCountProperty(memberAccessExpression, context.Model))
        {
            context.ReportIssue(Rule, ReportLocation(invocation), string.Format(MessageUseInstead, $"'{CountName}' property"));
        }

        static bool HasCountProperty(ExpressionSyntax expression, SemanticModel model) =>
            model.GetTypeInfo(expression).Type.GetMembers(CountName).OfType<IPropertySymbol>().Any();
    }

    private static void CheckToCollectionCalls(SonarSyntaxNodeReportingContext context)
    {
        var outerInvocation = (InvocationExpressionSyntax)context.Node;
        if (context.Model.GetSymbolInfo(outerInvocation).Symbol is IMethodSymbol outerMethodSymbol
            && MethodExistsOnIEnumerable(outerMethodSymbol, context.Model)
            && InnerInvocation(outerInvocation, outerMethodSymbol) is { } innerInvocation
            && context.Model.GetSymbolInfo(innerInvocation).Symbol is IMethodSymbol innerMethodSymbol
            && IsToCollectionCall(innerMethodSymbol)
            && !IsQueryableSource(innerInvocation, context.Model))
        {
            context.ReportIssue(Rule, ReportLocation(innerInvocation), string.Format(MessageDropFromMiddle, innerMethodSymbol.Name));
        }
    }

    private static bool MethodExistsOnIEnumerable(IMethodSymbol methodSymbol, SemanticModel model) =>
        !IgnoredMethodNames.Contains(methodSymbol.Name)
        && (methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            || (model.Compilation.GetTypeByMetadataName(KnownType.System_Linq_Enumerable) is { } enumerableType
                && enumerableType.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>().Any(x => ParametersMatch(methodSymbol.OriginalDefinition, x))));

    private static bool ParametersMatch(IMethodSymbol originalDefinition, IMethodSymbol member)
    {
        // Traditional Extension methods have an extra parameter for the instance, so we need to account for that when comparing parameters.
        var parameterIndexOffset = originalDefinition.IsExtensionMethod ? 0 : 1;

        if (originalDefinition.Parameters.Length + parameterIndexOffset != member.Parameters.Length)
        {
            return false;
        }

        for (var i = 1; i < member.Parameters.Length; i++)
        {
            if (!originalDefinition.Parameters[i - parameterIndexOffset].Type.Equals(member.Parameters[i].Type))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsToCollectionCall(IMethodSymbol methodSymbol) =>
        MethodNamesToCollection.Contains(methodSymbol.Name)
        && (methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
            || methodSymbol.ContainingType.ConstructedFrom.Is(KnownType.System_Collections_Generic_List_T));

    private static bool IsQueryableSource(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.Operands.Left is { } source
        && model.GetTypeInfo(source).Type is { } sourceType
        && sourceType.DerivesOrImplements(KnownType.System_Linq_IQueryable);

    private static void CheckExtensionMethodsOnIEnumerable(SonarSyntaxNodeReportingContext context)
    {
        var outerInvocation = (InvocationExpressionSyntax)context.Node;
        if (context.Model.GetSymbolInfo(outerInvocation).Symbol is IMethodSymbol outerMethodSymbol
            && (outerMethodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T) || outerMethodSymbol.IsExtensionOn(KnownType.System_Linq_IQueryable))
            && InnerInvocation(outerInvocation, outerMethodSymbol) is { } innerInvocation
            && context.Model.GetSymbolInfo(innerInvocation).Symbol is IMethodSymbol innerMethodSymbol
            && (innerMethodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T) || innerMethodSymbol.IsExtensionOn(KnownType.System_Linq_IQueryable)))
        {
            if (IsSimplifiable(outerMethodSymbol, outerInvocation, innerMethodSymbol))
            {
                context.ReportIssue(Rule, ReportLocation(innerInvocation), string.Format(MessageDropAndChange, WhereMethodName, outerMethodSymbol.Name));
            }
            else if (IsSelectSimplifiable(outerMethodSymbol, outerInvocation, innerInvocation, innerMethodSymbol) is { } typeNameInInnerSelect)
            {
                context.ReportIssue(Rule, ReportLocation(innerInvocation), string.Format(MessageUseInstead, $"'OfType<{typeNameInInnerSelect}>()'"));
            }
            else if (IsWhereSimplifiable(outerMethodSymbol, outerInvocation, innerInvocation, innerMethodSymbol) is { } typeNameInInnerWhere)
            {
                context.ReportIssue(Rule, ReportLocation(innerInvocation), string.Format(MessageUseInstead, $"'OfType<{typeNameInInnerWhere}>()'"));
            }
        }
    }

    private static InvocationExpressionSyntax InnerInvocation(InvocationExpressionSyntax outerInvocation, IMethodSymbol outerMethodSymbol) =>
        outerMethodSymbol.MethodKind == MethodKind.ReducedExtension
            ? outerInvocation.Operands.Left as InvocationExpressionSyntax
            : outerInvocation.ArgumentList.Arguments.FirstOrDefault()?.Expression as InvocationExpressionSyntax
                ?? outerInvocation.Operands.Left as InvocationExpressionSyntax;

    private static List<ArgumentSyntax> ReducedArguments(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation) =>
        methodSymbol.MethodKind == MethodKind.ReducedExtension
            ? invocation.ArgumentList.Arguments.ToList()
            : invocation.ArgumentList.Arguments.Skip(1).ToList();

    private static Location ReportLocation(InvocationExpressionSyntax invocation) =>
        invocation.Expression is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.Name.GetLocation()
            : invocation.Expression.GetLocation();

    private static string IsSelectSimplifiable(IMethodSymbol outerMethod, InvocationExpressionSyntax outerInvocation, InvocationExpressionSyntax innerInvocation, IMethodSymbol innerMethod) =>
        MethodNamesForTypeCheckingWithSelect.Contains(outerMethod.Name)
        && innerMethod.Name == SelectMethodName
        && IsFirstExpressionInLambdaIsNullChecking(outerMethod, outerInvocation)
        && CastTypeNameInLambda(SyntaxKind.AsExpression, innerMethod, innerInvocation) is { } typeNameInInnerSelect
            ? typeNameInInnerSelect
            : null;

    private static string IsWhereSimplifiable(IMethodSymbol outerMethod, InvocationExpressionSyntax outerInvocation, InvocationExpressionSyntax innerInvocation, IMethodSymbol innerMethod) =>
        outerMethod.Name == SelectMethodName
        && innerMethod.Name == WhereMethodName
        && IsExpressionInLambdaIsCast(outerMethod, outerInvocation) is { } typeNameInOuter
        && CastTypeNameInLambda(SyntaxKind.IsExpression, innerMethod, innerInvocation) is { } typeNameInInnerWhere
        && typeNameInOuter == typeNameInInnerWhere
            ? typeNameInInnerWhere
            : null;

    private static string IsExpressionInLambdaIsCast(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation) =>
        CastTypeNameInLambda(SyntaxKind.AsExpression, methodSymbol, invocation) ?? TypeNameFromLambdaExpression(methodSymbol, invocation);

    private static bool IsFirstExpressionInLambdaIsNullChecking(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation)
    {
        if (ReducedArguments(methodSymbol, invocation) is { Count: 1 } arguments)
        {
            var binaryExpression = ExpressionFromLambda(arguments[0].Expression).WithoutEnclosingParentheses as BinaryExpressionSyntax;
            while (binaryExpression is not null)
            {
                if (binaryExpression.IsKind(SyntaxKind.LogicalAndExpression))
                {
                    binaryExpression = binaryExpression.Left.WithoutEnclosingParentheses as BinaryExpressionSyntax;
                }
                else
                {
                    return binaryExpression.IsKind(SyntaxKind.NotEqualsExpression) && IsNullChecking(binaryExpression, LambdaParameter(arguments[0].Expression));
                }
            }
        }
        return false;
    }

    private static bool IsNullChecking(BinaryExpressionSyntax binaryExpression, string lambdaParameter) =>
        (CSharpEquivalenceChecker.AreEquivalent(SyntaxConstants.NullLiteralExpression, binaryExpression.Left.WithoutEnclosingParentheses)
            && binaryExpression.Right.WithoutEnclosingParentheses is IdentifierNameSyntax right
            && right.Identifier.ValueText == lambdaParameter)
        || (CSharpEquivalenceChecker.AreEquivalent(SyntaxConstants.NullLiteralExpression, binaryExpression.Right.WithoutEnclosingParentheses)
            && binaryExpression.Left.WithoutEnclosingParentheses is IdentifierNameSyntax left
            && left.Identifier.ValueText == lambdaParameter);

    private static ExpressionSyntax ExpressionFromLambda(ExpressionSyntax expression) =>
        expression is SimpleLambdaExpressionSyntax lambda
            ? lambda.Body as ExpressionSyntax
            : (expression as ParenthesizedLambdaExpressionSyntax)?.Body as ExpressionSyntax;

    private static string LambdaParameter(ExpressionSyntax expression) =>
        expression switch
        {
            SimpleLambdaExpressionSyntax lambda => lambda.Parameter.Identifier.ValueText,
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda
                when parenthesizedLambda.ParameterList.Parameters.Count != 0 => parenthesizedLambda.ParameterList.Parameters[0].Identifier.ValueText,
            _ => null
        };

    private static string CastTypeNameInLambda(SyntaxKind asOrIs, IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation) =>
        AsIsSyntaxKinds.Contains(asOrIs)
        && ReducedArguments(methodSymbol, invocation) is { Count: 1 } arguments
        && TypeNameFromLambdaExpression(asOrIs, arguments[0].Expression) is { } type
            ? type
            : null;

    private static string TypeNameFromLambdaExpression(SyntaxKind asOrIs, ExpressionSyntax expression) =>
        ExpressionFromLambda(expression).WithoutEnclosingParentheses is BinaryExpressionSyntax lambdaBody
        && LambdaParameter(expression) is { } lambdaParameter
        && lambdaBody.IsKind(asOrIs)
        && lambdaBody.Left.WithoutEnclosingParentheses is IdentifierNameSyntax castedParameter
        && castedParameter.Identifier.ValueText == lambdaParameter
            ? lambdaBody.Right.ToString()
            : null;

    private static string TypeNameFromLambdaExpression(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation) =>
        ReducedArguments(methodSymbol, invocation) is { Count: 1 } arguments
        && ExpressionFromLambda(arguments[0].Expression).WithoutEnclosingParentheses is CastExpressionSyntax lambdaBody
        && LambdaParameter(arguments[0].Expression) is { } lambdaParameter
        && lambdaBody.Expression.WithoutEnclosingParentheses is IdentifierNameSyntax castedParameter
        && castedParameter.Identifier.ValueText == lambdaParameter
            ? lambdaBody.Type.ToString()
            : null;

    private static bool IsSimplifiable(IMethodSymbol outerMethodSymbol,
                                       InvocationExpressionSyntax outerInvocation,
                                       IMethodSymbol innerMethodSymbol) =>
        MethodIsNotUsingPredicate(outerMethodSymbol, outerInvocation)
        && innerMethodSymbol.Name == WhereMethodName
        && (innerMethodSymbol.Parameters.Any(x =>
            x.Type is INamedTypeSymbol { TypeArguments.Length: WherePredicateTypeArgumentNumber })
            || innerMethodSymbol.Parameters.Any(x =>
            x.Type is INamedTypeSymbol predicate // For IQueryables 'Where' is an Expression<Func<,>> with the inner predicate having two type arguments
            && predicate.TypeArguments.OfType<INamedTypeSymbol>().Any(t => t.TypeArguments.Length == WherePredicateTypeArgumentNumber)));

    private static bool MethodIsNotUsingPredicate(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation) =>
        ReducedArguments(methodSymbol, invocation) is { Count: 0 } && MethodNamesWithPredicate.Contains(methodSymbol.Name);
}
