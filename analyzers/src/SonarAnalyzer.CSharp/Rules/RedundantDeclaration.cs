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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantDeclaration : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3257";
    internal const string DiagnosticTypeKey = "diagnosticType";
    internal const string ParameterNameKey = "ParameterNameKey";

    private const string MessageFormat = "Remove the {0}; it is redundant.";
    private const string UseDiscardMessageFormat = "'{0}' is not used. Use discard parameter instead.";

    internal enum RedundancyType
    {
        LambdaParameterType,
        ArraySize,
        ArrayType,
        ExplicitDelegate,
        ExplicitNullable,
        ObjectInitializer,
        DelegateParameterList
    }

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly DiagnosticDescriptor DiscardRule = DescriptorFactory.Create(DiagnosticId, UseDiscardMessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, DiscardRule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                ReportOnExplicitDelegateCreation(c);
                ReportRedundantNullableConstructorCall(c);
                ReportOnRedundantObjectInitializer(c);
            },
            SyntaxKind.ObjectCreationExpression,
            SyntaxKindEx.ImplicitObjectCreationExpression);

        context.RegisterNodeAction(ReportOnRedundantParameterList, SyntaxKind.AnonymousMethodExpression);
        context.RegisterNodeAction(ReportRedundancyInArrayCreation, SyntaxKind.ArrayCreationExpression);
        context.RegisterNodeAction(VisitParenthesizedLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
    }

    #region Type specification in lambda

    private static readonly ISet<SyntaxKind> RefOutKeywords = new HashSet<SyntaxKind>
    {
        SyntaxKind.RefKeyword,
        SyntaxKind.OutKeyword
    };

    private static void VisitParenthesizedLambdaExpression(SonarSyntaxNodeReportingContext context)
    {
        var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

        CheckUnusedParameters(context, lambda);
        CheckTypeSpecifications(context, lambda);
    }

    private static void CheckTypeSpecifications(SonarSyntaxNodeReportingContext context, ParenthesizedLambdaExpressionSyntax lambda)
    {
        if (!IsParameterListModifiable(lambda))
        {
            return;
        }

        if (!(context.Model.GetSymbolInfo(lambda).Symbol is IMethodSymbol symbol))
        {
            return;
        }

        var newParameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(lambda.ParameterList.Parameters.Select(x => SyntaxFactory.Parameter(x.Identifier))));
        if (lambda.ChangeSyntaxElement(lambda.WithParameterList(newParameterList), context.Model, out var newSemanticModel) is { } newLambda
            && newSemanticModel.GetSymbolInfo(newLambda) is { Symbol: IMethodSymbol newSymbol }
            && ParameterTypesMatch(symbol, newSymbol))
        {
            foreach (var parameter in lambda.ParameterList.Parameters)
            {
                context.ReportIssue(
                    Rule,
                    parameter.Type,
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, nameof(RedundancyType.LambdaParameterType)),
                    "type specification");
            }
        }
    }

    private static void CheckUnusedParameters(SonarSyntaxNodeReportingContext context, ParenthesizedLambdaExpressionSyntax lambda)
    {
        if (context.Compilation.IsLambdaDiscardParameterSupported())
        {
            var usedIdentifiers = UsedIdentifiers(lambda).ToList();
            foreach (var parameter in lambda.ParameterList.Parameters)
            {
                var parameterName = parameter.Identifier.Text;

                if (parameterName != SyntaxConstants.Discard && !usedIdentifiers.Contains(parameterName))
                {
                    context.ReportIssue(
                        DiscardRule,
                        parameter,
                        ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, nameof(RedundancyType.LambdaParameterType)).Add(ParameterNameKey, parameterName),
                        parameterName);
                }
            }
        }
    }

    private static IEnumerable<string> UsedIdentifiers(ParenthesizedLambdaExpressionSyntax lambda) =>
        lambda.Body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Select(x => x.Identifier.Text);

    private static bool IsParameterListModifiable(ParenthesizedLambdaExpressionSyntax lambda) =>
        lambda is { ParameterList.Parameters: { Count: > 0 } parameters }
        && parameters.All(x => x.Type is not null && x.Modifiers.All(m => !RefOutKeywords.Contains(m.Kind())));

    private static bool ParameterTypesMatch(IMethodSymbol method1, IMethodSymbol method2)
    {
        for (var i = 0; i < method1.Parameters.Length; i++)
        {
            if (!method1.Parameters[i].Type.Equals(method2.Parameters[i].Type))
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region Nullable constructor call

    private static void ReportRedundantNullableConstructorCall(SonarSyntaxNodeReportingContext context)
    {
        var objectCreation = ObjectCreationFactory.Create(context.Node);
        if (!IsNullableCreation(objectCreation, context.Model))
        {
            return;
        }

        if (IsInNotVarDeclaration(objectCreation.Expression)
            || IsInAssignmentOrReturnValue(objectCreation.Expression)
            || IsInArgumentAndCanBeChanged(objectCreation, context.Model))
        {
            ReportIssueOnRedundantObjectCreation(context, objectCreation.Expression, "explicit nullable type creation", RedundancyType.ExplicitNullable);
        }
    }

    private static bool IsNullableCreation(IObjectCreation objectCreation, SemanticModel model) =>
        objectCreation.ArgumentList is { Arguments.Count: 1 } && objectCreation.TypeSymbol(model).OriginalDefinition.Is(KnownType.System_Nullable_T);

    private static bool IsInAssignmentOrReturnValue(SyntaxNode objectCreation) =>
        objectCreation.GetFirstNonParenthesizedParent() switch
        {
            AssignmentExpressionSyntax _ => true,
            ReturnStatementSyntax _ => true,
            LambdaExpressionSyntax _ => true,
            _ => false
        };

    private static bool IsInNotVarDeclaration(SyntaxNode objectCreation)
    {
        var variableDeclaration = objectCreation.GetSelfOrTopParenthesizedExpression()
            .Parent?.Parent?.Parent as VariableDeclarationSyntax;

        return variableDeclaration is { Type.IsVar: false };
    }

    #endregion

    #region Array (creation, size, type)

    private static void ReportRedundancyInArrayCreation(SonarSyntaxNodeReportingContext context)
    {
        var array = (ArrayCreationExpressionSyntax)context.Node;
        ReportRedundantArraySizeSpecifier(context, array);
        ReportRedundantArrayTypeSpecifier(context, array);
    }

    private static void ReportRedundantArraySizeSpecifier(SonarSyntaxNodeReportingContext context, ArrayCreationExpressionSyntax array)
    {
        if (array.Initializer is null || array.Type is null)
        {
            return;
        }
        var rankSpecifier = array.Type.RankSpecifiers.FirstOrDefault();
        if (rankSpecifier is null || rankSpecifier.Sizes.Any(SyntaxKind.OmittedArraySizeExpression))
        {
            return;
        }

        foreach (var size in rankSpecifier.Sizes)
        {
            context.ReportIssue(
                Rule,
                size,
                ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, nameof(RedundancyType.ArraySize)),
                "array size specification");
        }
    }

    private static void ReportRedundantArrayTypeSpecifier(SonarSyntaxNodeReportingContext context, ArrayCreationExpressionSyntax array)
    {
        if (array.Initializer is null
            || !array.Initializer.Expressions.Any()
            || array.Initializer.Expressions.All(ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance)
            || array.Type is null
            || array.Type.RankSpecifiers.Count > 1)
        {
            return;
        }

        var rankSpecifier = array.Type.RankSpecifiers.FirstOrDefault();
        if (rankSpecifier is null
            || rankSpecifier.Sizes.Any(x => !x.IsKind(SyntaxKind.OmittedArraySizeExpression)))
        {
            return;
        }

        if (context.Model.GetTypeInfo(array.Type).Type is not IArrayTypeSymbol { ElementType: { TypeKind: not TypeKind.Error } arrayElementType })
        {
            return;
        }

        var canBeSimplified = array.Initializer.Expressions
            .Select(x => context.Model.GetTypeInfo(x).Type)
            .All(arrayElementType.Equals);

        if (canBeSimplified)
        {
            var location = Location.Create(array.SyntaxTree, TextSpan.FromBounds(array.Type.ElementType.SpanStart, array.Type.RankSpecifiers.Last().SpanStart));
            context.ReportIssue(Rule, location, ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, nameof(RedundancyType.ArrayType)), "array type");
        }
    }

    #endregion

    #region Object initializer

    private static void ReportOnRedundantObjectInitializer(SonarSyntaxNodeReportingContext context)
    {
        var objectCreation = ObjectCreationFactory.Create(context.Node);

        if (objectCreation.ArgumentList is not null && objectCreation.Initializer is not null && !objectCreation.Initializer.Expressions.Any())
        {
            context.ReportIssue(Rule, objectCreation.Initializer, ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, nameof(RedundancyType.ObjectInitializer)), "initializer");
        }
    }

    #endregion

    #region Explicit delegate creation

    private static void ReportOnExplicitDelegateCreation(SonarSyntaxNodeReportingContext context)
    {
        var objectCreation = ObjectCreationFactory.Create(context.Node);
        var argumentExpression = objectCreation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
        if (argumentExpression is null)
        {
            return;
        }

        if (!IsDelegateCreation(objectCreation, context.Model))
        {
            return;
        }

        if (IsInDeclarationNotVarNotDelegate(objectCreation.Expression, context.Model)
            || IsAssignmentNotDelegate(objectCreation.Expression, context.Model)
            || IsReturnValueNotDelegate(objectCreation.Expression, context.Model)
            || IsInArgumentAndCanBeChanged(
                objectCreation,
                context.Model,
                x => x.ArgumentList.Arguments.Any(a => a.Expression.IsDynamic(context.Model))))
        {
            ReportIssueOnRedundantObjectCreation(context, objectCreation.Expression, "explicit delegate creation", RedundancyType.ExplicitDelegate);
        }
    }

    private static bool IsInDeclarationNotVarNotDelegate(SyntaxNode objectCreation, SemanticModel model)
    {
        var variableDeclaration = objectCreation.GetSelfOrTopParenthesizedExpression()
            .Parent?.Parent?.Parent as VariableDeclarationSyntax;

        var type = variableDeclaration?.Type;

        if (type is null || type.IsVar)
        {
            return false;
        }

        var typeInformation = model.GetTypeInfo(type).Type;

        return !typeInformation.Is(KnownType.System_Delegate);
    }

    private static bool IsDelegateCreation(IObjectCreation objectCreation, SemanticModel model) =>
        objectCreation.TypeSymbol(model) is INamedTypeSymbol { TypeKind: TypeKind.Delegate };

    private static bool IsReturnValueNotDelegate(SyntaxNode objectCreation, SemanticModel model)
    {
        var parent = objectCreation.GetFirstNonParenthesizedParent();

        if (parent is not ReturnStatementSyntax and not LambdaExpressionSyntax)
        {
            return false;
        }

        if (model.GetEnclosingSymbol(objectCreation.SpanStart) is not IMethodSymbol enclosing)
        {
            return false;
        }

        return enclosing.ReturnType is not null && !enclosing.ReturnType.Is(KnownType.System_Delegate);
    }

    private static bool IsAssignmentNotDelegate(SyntaxNode objectCreation, SemanticModel model)
    {
        var parent = objectCreation.GetFirstNonParenthesizedParent();
        if (!(parent is AssignmentExpressionSyntax assignment))
        {
            return false;
        }

        var typeInformation = model.GetTypeInfo(assignment.Left).Type;

        return !typeInformation.Is(KnownType.System_Delegate);
    }

    #endregion

    #region Parameter list

    private static void ReportOnRedundantParameterList(SonarSyntaxNodeReportingContext context)
    {
        var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;
        if (anonymousMethod.ParameterList is null)
        {
            return;
        }

        if (context.Model.GetSymbolInfo(anonymousMethod).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var parameterNames = methodSymbol.Parameters.Select(x => x.Name).ToHashSet();

        var usedParameters = anonymousMethod.Body.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(x => parameterNames.Contains(x.Identifier.ValueText))
            .Select(x => context.Model.GetSymbolInfo(x).Symbol as IParameterSymbol)
            .WhereNotNull()
            .ToHashSet();

        if (!usedParameters.Intersect(methodSymbol.Parameters).Any())
        {
            context.ReportIssue(Rule, anonymousMethod.ParameterList, ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, nameof(RedundancyType.DelegateParameterList)), "parameter list");
        }
    }

    #endregion

    private static bool IsInArgumentAndCanBeChanged(IObjectCreation objectCreation, SemanticModel model, Func<InvocationExpressionSyntax, bool> additionalFilter = null)
    {
        if (!(objectCreation.Expression.GetFirstNonParenthesizedParent() is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } } argument))
        {
            return false;
        }

        if (additionalFilter is not null && additionalFilter(invocation))
        {
            return false;
        }

        // In C# 10 and later, the natural type of a lambda expression is Func<T> or Action<T>,
        // which are implicit convertable to Delegate. In earlier versions
        // CS1660: Cannot convert lambda expression to type 'Delegate' because it is not a delegate type
        // is raised.
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/lambda-improvements#natural-function-type
        // If the user specified another delegate type than Action<T> or Func<T>, like EventHandler, we
        // assume that the called method explicitly requires the more specific concrete delegate type.
        if (model.GetTypeInfo(objectCreation.Expression) is { Type: { } from, ConvertedType: { } convertedTo }
            && convertedTo.Is(KnownType.System_Delegate)
            && (model.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: < LanguageVersionEx.CSharp10 }
                || !(from.IsAny(KnownType.SystemFuncVariants) || from.IsAny(KnownType.SystemActionVariants))))
        {
            return false;
        }

        var methodSymbol = model.GetSymbolInfo(invocation).Symbol;
        if (methodSymbol is null)
        {
            return false;
        }
        var newArgument = argument.WithExpression(objectCreation.ArgumentList.Arguments.First().Expression);
        var newInvocation = invocation.WithArgumentList(invocation.ArgumentList.ReplaceNode(argument, newArgument));
        var overloadResolution = model.GetSpeculativeSymbolInfo(invocation.SpanStart, newInvocation, SpeculativeBindingOption.BindAsExpression);
        // The speculative binding is sometimes unable to do proper overload resolution and returns candidate overloads.
        // This is good enough for us as long as the original method is part of that list. Note: Any attempts with ChangeSyntaxElement and
        // TryGetSpeculativeSemanticModel failed to produce better results.
        return overloadResolution.AllSymbols().Any(x => x.Equals(methodSymbol));
    }

    private static void ReportIssueOnRedundantObjectCreation(SonarSyntaxNodeReportingContext context, SyntaxNode node, string message, RedundancyType redundancyType)
    {
        var location = node is ObjectCreationExpressionSyntax objectCreation
            ? objectCreation.CreateLocation(objectCreation.Type)
            : node.GetLocation();
        context.ReportIssue(Rule, location, ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, redundancyType.ToString()), message);
    }
}
