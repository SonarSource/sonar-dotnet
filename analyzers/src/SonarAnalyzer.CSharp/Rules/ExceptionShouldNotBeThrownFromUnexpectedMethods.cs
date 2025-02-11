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
public sealed class ExceptionShouldNotBeThrownFromUnexpectedMethods : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3877";
    private const string MessageFormat = "Remove this 'throw' {0}.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> DefaultAllowedExceptions = ImmutableArray.Create(KnownType.System_NotImplementedException);

    private static readonly ImmutableArray<KnownType> EventAccessorAllowedExceptions = ImmutableArray.Create(
        KnownType.System_NotImplementedException,
        KnownType.System_InvalidOperationException,
        KnownType.System_NotSupportedException,
        KnownType.System_ArgumentException);

    private static readonly HashSet<SyntaxKind> TrackedOperators =
    [
        SyntaxKind.EqualsEqualsToken,
        SyntaxKind.ExclamationEqualsToken,
        SyntaxKind.LessThanToken,
        SyntaxKind.GreaterThanToken,
        SyntaxKind.LessThanEqualsToken,
        SyntaxKind.GreaterThanEqualsToken
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c => CheckForIssue<MethodDeclarationSyntax>(c, mds => IsTrackedMethod(mds, c.Model), DefaultAllowedExceptions),
            SyntaxKind.MethodDeclaration);

        context.RegisterNodeAction(
            c => CheckForIssue<ConstructorDeclarationSyntax>(c, cds => cds.Modifiers.Any(SyntaxKind.StaticKeyword), DefaultAllowedExceptions),
            SyntaxKind.ConstructorDeclaration);

        context.RegisterNodeAction(
            c => CheckForIssue<OperatorDeclarationSyntax>(c, IsTrackedOperator, DefaultAllowedExceptions),
            SyntaxKind.OperatorDeclaration);

        context.RegisterNodeAction(
            c => CheckForIssue<AccessorDeclarationSyntax>(c, x => true, EventAccessorAllowedExceptions),
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.RemoveAccessorDeclaration);

        context.RegisterNodeAction(
            c => CheckForIssue<ConversionOperatorDeclarationSyntax>(c, cods => cods.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword), DefaultAllowedExceptions),
            SyntaxKind.ConversionOperatorDeclaration);
    }

    private static void CheckForIssue<TSyntax>(SonarSyntaxNodeReportingContext analysisContext, Func<TSyntax, bool> isTrackedSyntax, ImmutableArray<KnownType> allowedThrowTypes)
        where TSyntax : SyntaxNode
    {
        var syntax = (TSyntax)analysisContext.Node;
        if (isTrackedSyntax(syntax))
        {
            ReportOnInvalidThrow(analysisContext, syntax, allowedThrowTypes);
        }
    }

    private static bool IsTrackedOperator(OperatorDeclarationSyntax declaration) =>
        TrackedOperators.Contains(declaration.OperatorToken.Kind());

    private static bool IsTrackedMethod(MethodDeclarationSyntax declaration, SemanticModel model) =>
        HasTrackedMethodOrAttributeName(declaration)
        && model.GetDeclaredSymbol(declaration) is { } methodSymbol
        && HasTrackedMethodOrAttributeType(methodSymbol);

    private static bool HasTrackedMethodOrAttributeName(MethodDeclarationSyntax declaration)
    {
        var name = declaration.Identifier.ValueText;
        return name == "Equals"
            || name == "GetHashCode"
            || name == "ToString"
            || name == "Dispose"
            || name == "Equals"
            || CanBeModuleInitializer();

        bool CanBeModuleInitializer() =>
            declaration.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.ArgumentList is null && x.Name.ToStringContains("ModuleInitializer"));
    }

    private static bool HasTrackedMethodOrAttributeType(IMethodSymbol method) =>
        method.IsObjectEquals()
        || method.IsObjectGetHashCode()
        || method.IsObjectToString()
        || method.IsIDisposableDispose()
        || method.IsIEquatableEquals()
        || IsModuleInitializer(method);

    private static bool IsModuleInitializer(IMethodSymbol method) =>
        method.AnyAttributeDerivesFrom(KnownType.System_Runtime_CompilerServices_ModuleInitializerAttribute);

    private static void ReportOnInvalidThrow(SonarSyntaxNodeReportingContext context, SyntaxNode node, ImmutableArray<KnownType> allowedTypes)
    {
        if (node.ArrowExpressionBody() is { } expressionBody
            && GetLocationToReport(
                expressionBody.Expression.DescendantNodesAndSelf().Where(ThrowExpressionSyntaxWrapper.IsInstance).Select(x => (ThrowExpressionSyntaxWrapper)x),
                x => x.SyntaxNode,
                x => x.Expression) is { } throwExpressionLocation)
        {
            context.ReportIssue(Rule, throwExpressionLocation, "expression");
        }
        else if (GetLocationToReport(
            node.DescendantNodes().OfType<ThrowStatementSyntax>().Where(x => x.Expression is not null),
            x => x,
            x => x.Expression) is { } throwStatementLocation)
        {
            context.ReportIssue(Rule, throwStatementLocation, "statement");
        }

        // `throwNodes` is an enumeration of either throw expressions or throw statements
        // Because of the ShimLayer ThrowExpression implementation, we need to provide extra boilerplate as the wrappers to extract the node and the expression.
        // The location is returned only if an issue should be reported. Otherwise, null is returned.
        Location GetLocationToReport<TThrow>(IEnumerable<TThrow> throwNodes, Func<TThrow, SyntaxNode> getNode, Func<TThrow, ExpressionSyntax> getExpression) =>
            throwNodes.Select(x => new NodeAndSymbol(getNode(x), context.Model.GetSymbolInfo(getExpression(x)).Symbol))
                .FirstOrDefault(x => x.Symbol is not null && ShouldReport(x.Symbol.ContainingType, allowedTypes))?
                .Node
                .GetLocation();
    }

    private static bool ShouldReport(INamedTypeSymbol exceptionType, ImmutableArray<KnownType> allowedTypes) =>
        !exceptionType.IsAny(allowedTypes) && !exceptionType.DerivesFromAny(allowedTypes);
}
