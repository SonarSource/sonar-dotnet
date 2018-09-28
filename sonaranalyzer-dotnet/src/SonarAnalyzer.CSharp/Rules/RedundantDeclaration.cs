/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public sealed class RedundantDeclaration : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3257";
        private const string MessageFormat = "Remove the {0}; it is redundant.";


        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

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

        protected override void Initialize(SonarAnalysisContext context)
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
                ReportOnRedundantParameterList,
                SyntaxKind.AnonymousMethodExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                ReportRedundancyInArrayCreation,
                SyntaxKind.ArrayCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                ReportRedundantTypeSpecificationInLambda,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        #region Type specification in lambda

        private static readonly ISet<SyntaxKind> RefOutKeywords = new HashSet<SyntaxKind>
        {
            SyntaxKind.RefKeyword,
            SyntaxKind.OutKeyword
        };

        private static void ReportRedundantTypeSpecificationInLambda(SyntaxNodeAnalysisContext context)
        {
            var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;
            if (!IsParameterListModifiable(lambda))
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol symbol))
            {
                return;
            }

            var newParameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(lambda.ParameterList.Parameters.Select(p => SyntaxFactory.Parameter(p.Identifier))));
            var newLambda = lambda.WithParameterList(newParameterList);

            newLambda = ChangeSyntaxElement(lambda, newLambda, context.SemanticModel, out var newSemanticModel);

            if (!(newSemanticModel.GetSymbolInfo(newLambda).Symbol is IMethodSymbol newSymbol) ||
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
            for (var i = 0; i < method1.Parameters.Length; i++)
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
            var parent = objectCreation.GetFirstNonParenthesizedParent();
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
                rankSpecifier.Sizes.Any(SyntaxKind.OmittedArraySizeExpression))
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
                array.Type == null ||
                array.Type.RankSpecifiers.Count > 1)
            {
                return;
            }

            var rankSpecifier = array.Type.RankSpecifiers.FirstOrDefault();
            if (rankSpecifier == null ||
                rankSpecifier.Sizes.Any(s => !s.IsKind(SyntaxKind.OmittedArraySizeExpression)))
            {
                return;
            }

            if (!(context.SemanticModel.GetTypeInfo(array.Type).Type is IArrayTypeSymbol arrayType))
            {
                return;
            }

            var canBeSimplified = array.Initializer.Expressions
                .Select(exp => context.SemanticModel.GetTypeInfo(exp).Type)
                .All(type => Equals(type, arrayType.ElementType));

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

            return semanticModel.GetSymbolInfo(objectCreation.Type).Symbol is INamedTypeSymbol type &&
                type.TypeKind == TypeKind.Delegate;
        }

        private static bool IsReturnValueNotDelegate(ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel)
        {
            var parent = objectCreation.GetFirstNonParenthesizedParent();

            if (!(parent is ReturnStatementSyntax) &&
                !(parent is LambdaExpressionSyntax))
            {
                return false;
            }

            if (!(semanticModel.GetEnclosingSymbol(objectCreation.SpanStart) is IMethodSymbol enclosing))
            {
                return false;
            }

            return enclosing.ReturnType != null &&
                !enclosing.ReturnType.Is(KnownType.System_Delegate);
        }

        private static bool IsAssignmentNotDelegate(ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel)
        {
            var parent = objectCreation.GetFirstNonParenthesizedParent();
            if (!(parent is AssignmentExpressionSyntax assignment))
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

            if (!(context.SemanticModel.GetSymbolInfo(anonymousMethod).Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            var parameterNames = methodSymbol.Parameters.Select(p => p.Name).ToHashSet();

            var usedParameters = anonymousMethod.Body.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => parameterNames.Contains(id.Identifier.ValueText))
                .Select(id => context.SemanticModel.GetSymbolInfo(id).Symbol as IParameterSymbol)
                .WhereNotNull()
                .ToHashSet();

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
            var parent = objectCreation.GetFirstNonParenthesizedParent();
            var argument = parent as ArgumentSyntax;

            if (!(argument?.Parent?.Parent is InvocationExpressionSyntax invocation))
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
            newInvocation = ChangeSyntaxElement(invocation, newInvocation, semanticModel, out var newSemanticModel);

            return newSemanticModel.GetSymbolInfo(newInvocation).Symbol is IMethodSymbol newMethodSymbol &&
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
