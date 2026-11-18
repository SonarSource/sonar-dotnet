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
public sealed class GetTypeWithIsAssignableFrom : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S2219";
    internal const string UseIsOperatorKey = "UseIsOperator";
    internal const string ShouldRemoveGetTypeKey = "ShouldRemoveGetType";
    private const string MessageFormat = "Use {0} instead.";
    private const string MessageIsOperator = "the 'is' operator";
    private const string MessageIsInstanceOfType = "the 'IsInstanceOfType()' method";
    private const string MessageNullCheck = "a 'null' check";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                    && invocation.HasExactlyNArguments(1)
                    && memberAccess.Name.Identifier.ValueText is var methodName
                    && (methodName == "IsInstanceOfType" || methodName == "IsAssignableFrom")
                    && c.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.IsInType(KnownType.System_Type))
                {
                    var argumentExpression = invocation.ArgumentList.Arguments.First().Expression;
                    CheckForIsAssignableFrom(c, memberAccess, methodSymbol, argumentExpression);
                    CheckForIsInstanceOfType(c, memberAccess, methodSymbol, argumentExpression);
                }
            },
            SyntaxKind.InvocationExpression);

        context.RegisterNodeAction(
            c =>
            {
                var binary = (BinaryExpressionSyntax)c.Node;
                CheckGetTypeAndTypeOfEquality(c, binary.Left, binary.Right);
                CheckGetTypeAndTypeOfEquality(c, binary.Right, binary.Left);

                CheckAsOperatorComparedToNull(c, binary.Left, binary.Right);
                CheckAsOperatorComparedToNull(c, binary.Right, binary.Left);
            },
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression);

        context.RegisterNodeAction(c =>
            {
                var isExpression = (BinaryExpressionSyntax)c.Node;
                if (c.Model.GetTypeInfo(isExpression.Left).Type is { IsClass: true } objectToCast
                    && c.Model.GetSymbolInfo(isExpression.Right).Symbol is INamedTypeSymbol namedSymbol
                    && namedSymbol.SymbolType is { IsClass: true } typeCastTo
                    && !typeCastTo.Is(KnownType.System_Object)
                    && objectToCast.DerivesOrImplements(typeCastTo))
                {
                    ReportDiagnostic(c, MessageNullCheck);
                }
            },
            SyntaxKind.IsExpression);

        context.RegisterNodeAction(c =>
            {
                var isPattern = (IsPatternExpressionSyntaxWrapper)c.Node;
                if (ConstantPatternExpression(isPattern.Pattern) is { } constantExpression)
                {
                    CheckAsOperatorComparedToNull(c, isPattern.Expression, constantExpression);
                }
            },
            SyntaxKindEx.IsPatternExpression);
    }

    private static void CheckAsOperatorComparedToNull(SonarSyntaxNodeReportingContext context, ExpressionSyntax sideA, ExpressionSyntax sideB)
    {
        if (sideA.WithoutEnclosingParentheses.IsKind(SyntaxKind.AsExpression) && sideB.WithoutEnclosingParentheses.IsKind(SyntaxKind.NullLiteralExpression))
        {
            ReportDiagnostic(context, MessageIsOperator);
        }
    }

    private static void CheckGetTypeAndTypeOfEquality(SonarSyntaxNodeReportingContext context, ExpressionSyntax sideA, ExpressionSyntax sideB)
    {
        if (sideA.ToStringContains("GetType")
            && sideB is TypeOfExpressionSyntax sideBTypeOf
            && (sideA as InvocationExpressionSyntax).IsGetTypeCall(context.Model)
            && context.Model.GetTypeInfo(sideBTypeOf.Type, context.Cancel).Type is { } typeSymbol // Can be null for empty identifier from 'typeof' unfinished syntax
            && typeSymbol.IsSealed
            && !typeSymbol.OriginalDefinition.Is(KnownType.System_Nullable_T)
            && !IsGetTypeOnUnion((InvocationExpressionSyntax)sideA, context.Model, context.Cancel))
        {
            ReportDiagnostic(context, MessageIsOperator);
        }
    }

    private static void CheckForIsInstanceOfType(SonarSyntaxNodeReportingContext context, MemberAccessExpressionSyntax memberAccess, IMethodSymbol methodSymbol, ExpressionSyntax argument)
    {
        if (methodSymbol.Name == nameof(Type.IsInstanceOfType)
            && memberAccess.Expression is TypeOfExpressionSyntax typeOf
            && !IsUnboundedGenericType(typeOf)
            && !IsUnion(argument, context.Model, context.Cancel))
        {
            ReportDiagnostic(context, MessageIsOperator, useIsOperator: true);
        }
    }

    private static void CheckForIsAssignableFrom(SonarSyntaxNodeReportingContext context, MemberAccessExpressionSyntax memberAccess, IMethodSymbol methodSymbol, ExpressionSyntax argument)
    {
        if (methodSymbol.Name == nameof(Type.IsAssignableFrom) && (argument as InvocationExpressionSyntax).IsGetTypeCall(context.Model))
        {
            if (memberAccess.Expression is TypeOfExpressionSyntax typeOf)
            {
                if (!IsUnboundedGenericType(typeOf) && !IsGetTypeOnUnion((InvocationExpressionSyntax)argument, context.Model, context.Cancel))
                {
                    ReportDiagnostic(context, MessageIsOperator, useIsOperator: true, shouldRemoveGetType: true);
                }
            }
            else
            {
                ReportDiagnostic(context, MessageIsInstanceOfType, shouldRemoveGetType: true);
            }
        }
    }

    private static bool IsUnboundedGenericType(TypeOfExpressionSyntax typeOf) =>
        typeOf.Type switch
        {
            GenericNameSyntax { IsUnboundGenericName: true } => true,
            QualifiedNameSyntax { Right: GenericNameSyntax { IsUnboundGenericName: true } } => true,
            _ => false,
        };

    private static bool IsGetTypeOnUnion(InvocationExpressionSyntax getTypeCall, SemanticModel model, CancellationToken cancel) =>
        getTypeCall.Expression.LeftOfDot is { } receiver
            ? IsUnion(receiver, model, cancel)
            : model.GetEnclosingSymbol(getTypeCall.SpanStart, cancel)?.ContainingType is { IsUnion: true };

    private static bool IsUnion(ExpressionSyntax expression, SemanticModel model, CancellationToken cancel) =>
        model.GetTypeInfo(expression, cancel).Type is { } type && type.NullableUnderlyingTypeOrSelf().IsUnion;

    private static ExpressionSyntax ConstantPatternExpression(SyntaxNode node) =>
        node.Kind() switch
        {
            SyntaxKindEx.ConstantPattern => ((ConstantPatternSyntaxWrapper)node).Expression,
            SyntaxKindEx.NotPattern => ConstantPatternExpression(((UnaryPatternSyntaxWrapper)node).Pattern),
            _ => null
        };

    private static void ReportDiagnostic(SonarSyntaxNodeReportingContext context, string messageArg, bool useIsOperator = false, bool shouldRemoveGetType = false)
    {
        var properties = ImmutableDictionary<string, string>.Empty
            .Add(UseIsOperatorKey, useIsOperator.ToString())
            .Add(ShouldRemoveGetTypeKey, shouldRemoveGetType.ToString());
        context.ReportIssue(Rule, context.Node, properties, messageArg);
    }
}
