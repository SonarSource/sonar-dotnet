/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class RedundantDeclaration : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3257";
        private const string MessageFormat = "Remove the {0}; it is redundant.";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        internal const string DiagnosticTypeKey = "diagnosticType";

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

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    ReportOnExplicitDelegateCreation(c);
                    ReportRedundantNullableConstructorCall(c);
                    ReportOnRedundantObjectInitializer(c);
                },
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportOnRedundantParameterList(c),
                SyntaxKind.AnonymousMethodExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportRedundancyInArrayCreation(c),
                SyntaxKind.ArrayCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportRedundantTypeSpecificationInLambda(c),
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        #region Type specification in lambda

        private static readonly ISet<SyntaxKind> RefOutKeywords = ImmutableHashSet.Create(
            SyntaxKind.RefKeyword, SyntaxKind.OutKeyword);

        private static void ReportRedundantTypeSpecificationInLambda(SyntaxNodeAnalysisContext context)
        {
            var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;
            if (!IsParameterListModifiable(lambda))
            {
                return;
            }

            var symbol = context.SemanticModel.GetSymbolInfo(lambda).Symbol as IMethodSymbol;
            if (symbol == null)
            {
                return;
            }

            var newParameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(lambda.ParameterList.Parameters.Select(p => SyntaxFactory.Parameter(p.Identifier))));
            var newLambda = lambda.WithParameterList(newParameterList);

            SemanticModel newSemanticModel;
            newLambda = ChangeSyntaxElement(lambda, newLambda, context.SemanticModel, out newSemanticModel);
            var newSymbol = newSemanticModel.GetSymbolInfo(newLambda).Symbol as IMethodSymbol;

            if (newSymbol == null ||
                ParameterTypesDoNotMatch(symbol, newSymbol))
            {
                return;
            }

            foreach (var parameter in lambda.ParameterList.Parameters)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, parameter.Type.GetLocation(),
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, RedundancyType.LambdaParameterType.ToString()),
                    "type specification"));
            }
        }

        private static bool IsParameterListModifiable(ParenthesizedLambdaExpressionSyntax lambda)
        {
            return lambda.ParameterList != null &&
                lambda.ParameterList.Parameters.All(
                    p => p.Type != null &&
                    p.Modifiers.All(m => !RefOutKeywords.Contains(m.Kind())));
        }

        private static bool ParameterTypesDoNotMatch(IMethodSymbol method1, IMethodSymbol method2)
        {
            for (int i = 0; i < method1.Parameters.Length; i++)
            {
                if (method1.Parameters[i].Type.ToDisplayString() != method2.Parameters[i].Type.ToDisplayString())
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Nullable constructor call

        private static void ReportRedundantNullableConstructorCall(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            if (!IsNullableCreation(objectCreation, context.SemanticModel))
            {
                return;
            }

            if (IsInNotVarDeclaration(objectCreation) ||
                IsInAssignmentOrReturnValue(objectCreation) ||
                IsInArgumentAndCanBeChanged(objectCreation, context.SemanticModel))
            {
                ReportIssueOnRedundantObjectCreation(context, objectCreation, "explicit nullable type creation", RedundancyType.ExplicitNullable);
            }
        }

        private static bool IsNullableCreation(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            if (objectCreation.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count != 1)
            {
                return false;
            }

            var type = semanticModel.GetSymbolInfo(objectCreation).Symbol?.ContainingType;
            return type != null &&
                type.OriginalDefinition.Is(KnownType.System_Nullable_T);
        }


        private static bool IsInAssignmentOrReturnValue(ObjectCreationExpressionSyntax objectCreation)
        {
            var parent = objectCreation.GetSelfOrTopParenthesizedExpression().Parent;
            return parent is AssignmentExpressionSyntax ||
                parent is ReturnStatementSyntax ||
                parent is LambdaExpressionSyntax;
        }

        private static bool IsInNotVarDeclaration(ObjectCreationExpressionSyntax objectCreation)
        {
            var variableDeclaration = objectCreation.GetSelfOrTopParenthesizedExpression()
                .Parent?.Parent?.Parent as VariableDeclarationSyntax;

            return variableDeclaration?.Type != null &&
                !variableDeclaration.Type.IsVar;
        }

        #endregion

        #region Array (creation, size, type)

        private static void ReportRedundancyInArrayCreation(SyntaxNodeAnalysisContext context)
        {
            var array = (ArrayCreationExpressionSyntax)context.Node;
            ReportRedundantArraySizeSpecifier(context, array);
            ReportRedundantArrayTypeSpecifier(context, array);
        }

        private static void ReportRedundantArraySizeSpecifier(SyntaxNodeAnalysisContext context, ArrayCreationExpressionSyntax array)
        {
            if (array.Initializer == null ||
                array.Type == null)
            {
                return;
            }
            var rankSpecifier = array.Type.RankSpecifiers.FirstOrDefault();
            if (rankSpecifier == null ||
                rankSpecifier.Sizes.Any(s => s.IsKind(SyntaxKind.OmittedArraySizeExpression)))
            {
                return;
            }

            foreach (var size in rankSpecifier.Sizes)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, size.GetLocation(),
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, RedundancyType.ArraySize.ToString()),
                    "array size specification"));
            }
        }

        private static void ReportRedundantArrayTypeSpecifier(SyntaxNodeAnalysisContext context, ArrayCreationExpressionSyntax array)
        {
            if (array.Initializer == null ||
                !array.Initializer.Expressions.Any() ||
                array.Type == null)
            {
                return;
            }

            var rankSpecifier = array.Type.RankSpecifiers.FirstOrDefault();
            if (rankSpecifier == null ||
                rankSpecifier.Sizes.Any(s => !s.IsKind(SyntaxKind.OmittedArraySizeExpression)))
            {
                return;
            }

            var arrayType = context.SemanticModel.GetTypeInfo(array.Type).Type as IArrayTypeSymbol;
            if (arrayType == null)
            {
                return;
            }

            var canBeSimplified = array.Initializer.Expressions
                .Select(exp => context.SemanticModel.GetTypeInfo(exp).Type)
                .All(type => object.Equals(type, arrayType.ElementType));

            if (canBeSimplified)
            {
                var location = Location.Create(array.SyntaxTree, TextSpan.FromBounds(
                    array.Type.ElementType.SpanStart, array.Type.RankSpecifiers.Last().SpanStart));

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location,
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, RedundancyType.ArrayType.ToString()),
                    "array type"));
            }
        }

        #endregion

        #region Object initializer

        private static void ReportOnRedundantObjectInitializer(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            if (objectCreation.ArgumentList == null)
            {
                return;
            }

            if (objectCreation.Initializer != null &&
                !objectCreation.Initializer.Expressions.Any())
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, objectCreation.Initializer.GetLocation(),
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, RedundancyType.ObjectInitializer.ToString()),
                    "initializer"));
            }
        }

        #endregion

        #region Explicit delegate creation

        private static void ReportOnExplicitDelegateCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            if (!IsDelegateCreation(objectCreation, context.SemanticModel))
            {
                return;
            }

            var argumentExpression = objectCreation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
            if (argumentExpression == null)
            {
                return;
            }

            if (IsInDeclarationNotVarNotDelegate(objectCreation, context.SemanticModel) ||
                IsAssignmentNotDelegate(objectCreation, context.SemanticModel) ||
                IsReturnValueNotDelegate(objectCreation, context.SemanticModel) ||
                IsInArgumentAndCanBeChanged(objectCreation, context.SemanticModel,
                    invocation => invocation.ArgumentList.Arguments.Any(a => IsDynamic(a, context.SemanticModel))))
            {
                ReportIssueOnRedundantObjectCreation(context, objectCreation, "explicit delegate creation", RedundancyType.ExplicitDelegate);
            }
        }

        private static bool IsDynamic(ArgumentSyntax argument, SemanticModel semanticModel)
        {
            return semanticModel.GetTypeInfo(argument.Expression).Type is IDynamicTypeSymbol;
        }

        private static bool IsInDeclarationNotVarNotDelegate(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var variableDeclaration = objectCreation.GetSelfOrTopParenthesizedExpression()
                .Parent?.Parent?.Parent as VariableDeclarationSyntax;

            var type = variableDeclaration?.Type;

            if (type == null ||
                type.IsVar)
            {
                return false;
            }

            var typeInformation = semanticModel.GetTypeInfo(type).Type;

            return typeInformation != null &&
                !typeInformation.Is(KnownType.System_Delegate);
        }

        private static bool IsDelegateCreation(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            var type = semanticModel.GetSymbolInfo(objectCreation.Type).Symbol as INamedTypeSymbol;

            return type != null &&
                type.TypeKind == TypeKind.Delegate;
        }

        private static bool IsReturnValueNotDelegate(ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel)
        {
            var parent = objectCreation.GetSelfOrTopParenthesizedExpression().Parent;

            if (!(parent is ReturnStatementSyntax) &&
                !(parent is LambdaExpressionSyntax))
            {
                return false;
            }

            var enclosing = semanticModel.GetEnclosingSymbol(objectCreation.SpanStart) as IMethodSymbol;
            if (enclosing == null)
            {
                return false;
            }

            return enclosing.ReturnType != null &&
                !enclosing.ReturnType.Is(KnownType.System_Delegate);
        }

        private static bool IsAssignmentNotDelegate(ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel)
        {
            var parent = objectCreation.GetSelfOrTopParenthesizedExpression().Parent;
            var assignment = parent as AssignmentExpressionSyntax;
            if (assignment == null)
            {
                return false;
            }

            var typeInformation = semanticModel.GetTypeInfo(assignment.Left).Type;

            return typeInformation != null &&
                !typeInformation.Is(KnownType.System_Delegate);
        }

        #endregion

        #region Parameter list

        private static void ReportOnRedundantParameterList(SyntaxNodeAnalysisContext context)
        {
            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;
            if (anonymousMethod.ParameterList == null)
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetSymbolInfo(anonymousMethod).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            var parameterNames = methodSymbol.Parameters.Select(p => p.Name).ToImmutableHashSet();

            var usedParameters = anonymousMethod.Body.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => parameterNames.Contains(id.Identifier.ValueText))
                .Select(id => context.SemanticModel.GetSymbolInfo(id).Symbol as IParameterSymbol)
                .Where(p => p != null)
                .ToImmutableHashSet();

            if (!usedParameters.Intersect(methodSymbol.Parameters).Any())
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, anonymousMethod.ParameterList.GetLocation(),
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, RedundancyType.DelegateParameterList.ToString()),
                    "parameter list"));
            }
        }

        #endregion

        private static bool IsInArgumentAndCanBeChanged(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel,
            Func<InvocationExpressionSyntax, bool> additionalFilter = null)
        {
            var parent = objectCreation.GetSelfOrTopParenthesizedExpression().Parent;
            var argument = parent as ArgumentSyntax;

            var invocation = argument?.Parent?.Parent as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return false;
            }

            if (additionalFilter != null && additionalFilter(invocation))
            {
                return false;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            if (methodSymbol == null)
            {
                return false;
            }

            var newArgumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(invocation.ArgumentList.Arguments
                    .Select(a => a == argument
                        ? SyntaxFactory.Argument(objectCreation.ArgumentList.Arguments.First().Expression)
                        : a)));
            var newInvocation = invocation.WithArgumentList(newArgumentList);
            SemanticModel newSemanticModel;
            newInvocation = ChangeSyntaxElement(invocation, newInvocation, semanticModel, out newSemanticModel);
            var newMethodSymbol = newSemanticModel.GetSymbolInfo(newInvocation).Symbol as IMethodSymbol;

            return newMethodSymbol != null &&
                methodSymbol.ToDisplayString() == newMethodSymbol.ToDisplayString();
        }

        private static void ReportIssueOnRedundantObjectCreation(SyntaxNodeAnalysisContext context,
            ObjectCreationExpressionSyntax objectCreation, string message, RedundancyType redundancyType)
        {
            var location = Location.Create(objectCreation.SyntaxTree,
                TextSpan.FromBounds(objectCreation.SpanStart, objectCreation.Type.Span.End));
            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location,
                    ImmutableDictionary<string, string>.Empty.Add(DiagnosticTypeKey, redundancyType.ToString()),
                    message));
        }

        private static T ChangeSyntaxElement<T>(T originalNode, T newNode, SemanticModel originalSemanticModel,
            out SemanticModel newSemanticModel)
            where T : SyntaxNode
        {
            var annotation = new SyntaxAnnotation();
            var annotatedNode = newNode.WithAdditionalAnnotations(annotation);

            var newSyntaxRoot = originalNode.SyntaxTree.GetRoot().ReplaceNode(
                originalNode,
                annotatedNode);

            var newTree = newSyntaxRoot.SyntaxTree.WithRootAndOptions(
                newSyntaxRoot,
                originalNode.SyntaxTree.Options);

            var newCompilation = originalSemanticModel.Compilation.ReplaceSyntaxTree(
                originalNode.SyntaxTree,
                newTree);

            newSemanticModel = newCompilation.GetSemanticModel(newTree);

            return (T)newTree.GetRoot().GetAnnotatedNodes(annotation).First();
        }
    }
}
