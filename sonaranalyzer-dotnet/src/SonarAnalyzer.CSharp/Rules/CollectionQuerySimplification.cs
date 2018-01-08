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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CollectionQuerySimplification : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2971";
        private const string MessageFormat = "{0}";
        internal const string MessageUseInstead = "Use {0} here instead.";
        internal const string MessageDropAndChange = "Drop '{0}' and move the condition into the '{1}'.";
        internal const string MessageDropFromMiddle = "Drop '{0}' from the middle of the call chain.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> MethodNamesWithPredicate = ImmutableHashSet.Create(
            "Any", "LongCount", "Count",
            "First", "FirstOrDefault", "Last", "LastOrDefault",
            "Single", "SingleOrDefault");

        private static readonly ISet<string> MethodNamesForTypeCheckingWithSelect = ImmutableHashSet.Create(
            "Any", "LongCount", "Count",
            "First", "FirstOrDefault", "Last", "LastOrDefault",
            "Single", "SingleOrDefault", "SkipWhile", "TakeWhile");

        private static readonly ISet<string> MethodNamesToCollection = ImmutableHashSet.Create(
            "ToList",
            "ToArray");

        private static readonly ISet<SyntaxKind> AsIsSyntaxKinds = ImmutableHashSet.Create(
            SyntaxKind.AsExpression,
            SyntaxKind.IsExpression);

        private const string WhereMethodName = "Where";
        private const string SelectMethodName = "Select";

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckExtensionMethodsOnIEnumerable, SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(CheckToCollectionCalls, SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(CheckCountCall, SyntaxKind.InvocationExpression);
        }

        private static void CheckCountCall(SyntaxNodeAnalysisContext context)
        {
            const string CountName = "Count";

            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null ||
                methodSymbol.Name != CountName ||
                invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Any() ||
                !methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return;
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return;
            }

            var symbol = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (symbol.GetMembers(CountName).OfType<IPropertySymbol>().Any())
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetReportLocation(invocation),
                    string.Format(MessageUseInstead, $"'{CountName}' property")));
            }
        }

        private static void CheckToCollectionCalls(SyntaxNodeAnalysisContext context)
        {
            var outerInvocation = (InvocationExpressionSyntax)context.Node;
            var outerMethodSymbol = context.SemanticModel.GetSymbolInfo(outerInvocation).Symbol as IMethodSymbol;
            if (outerMethodSymbol == null ||
                !MethodExistsOnIEnumerable(outerMethodSymbol, context.SemanticModel))
            {
                return;
            }

            var innerInvocation = GetInnerInvocation(outerInvocation, outerMethodSymbol);
            if (innerInvocation == null)
            {
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(innerInvocation).Symbol is IMethodSymbol innerMethodSymbol &&
                IsToCollectionCall(innerMethodSymbol))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageDropFromMiddle, innerMethodSymbol.Name)));
            }
        }

        private static bool MethodExistsOnIEnumerable(IMethodSymbol methodSymbol, SemanticModel semanticModel)
        {
            if (methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return true;
            }

            var enumerableType = semanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if (enumerableType == null)
            {
                return false;
            }

            var members = enumerableType.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>();
            return members.Any(member => ParametersMatch(methodSymbol.OriginalDefinition, member));
        }

        private static bool ParametersMatch(IMethodSymbol originalDefinition, IMethodSymbol member)
        {
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

        private static bool IsToCollectionCall(IMethodSymbol methodSymbol)
        {
            return MethodNamesToCollection.Contains(methodSymbol.Name) &&
                (methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T) ||
                methodSymbol.ContainingType.ConstructedFrom.Is(KnownType.System_Collections_Generic_List_T));
        }

        private static void CheckExtensionMethodsOnIEnumerable(SyntaxNodeAnalysisContext context)
        {
            var outerInvocation = (InvocationExpressionSyntax)context.Node;
            var outerMethodSymbol = context.SemanticModel.GetSymbolInfo(outerInvocation).Symbol as IMethodSymbol;
            if (outerMethodSymbol == null ||
                !outerMethodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return;
            }

            var innerInvocation = GetInnerInvocation(outerInvocation, outerMethodSymbol);
            if (innerInvocation == null)
            {
                return;
            }

            var innerMethodSymbol = context.SemanticModel.GetSymbolInfo(innerInvocation).Symbol as IMethodSymbol;
            if (innerMethodSymbol == null ||
                !innerMethodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return;
            }

            if (CheckForSimplifiable(outerMethodSymbol, outerInvocation, innerMethodSymbol, innerInvocation, context))
            {
                return;
            }

            CheckForCastSimplification(outerMethodSymbol, outerInvocation, innerMethodSymbol, innerInvocation, context);
        }

        private static InvocationExpressionSyntax GetInnerInvocation(InvocationExpressionSyntax outerInvocation,
            IMethodSymbol outerMethodSymbol)
        {
            if (outerMethodSymbol.MethodKind == MethodKind.ReducedExtension)
            {
                if (outerInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Expression as InvocationExpressionSyntax;
                }
            }
            else
            {
                var argument = outerInvocation.ArgumentList.Arguments.FirstOrDefault();
                if (argument != null)
                {
                    return argument.Expression as InvocationExpressionSyntax;
                }

                if (outerInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Expression as InvocationExpressionSyntax;
                }
            }

            return null;
        }

        private static List<ArgumentSyntax> GetReducedArguments(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation)
        {
            return methodSymbol.MethodKind == MethodKind.ReducedExtension
                ? invocation.ArgumentList.Arguments.ToList()
                : invocation.ArgumentList.Arguments.Skip(1).ToList();
        }

        private static void CheckForCastSimplification(IMethodSymbol outerMethodSymbol, InvocationExpressionSyntax outerInvocation,
            IMethodSymbol innerMethodSymbol, InvocationExpressionSyntax innerInvocation, SyntaxNodeAnalysisContext context)
        {
            if (MethodNamesForTypeCheckingWithSelect.Contains(outerMethodSymbol.Name) &&
                innerMethodSymbol.Name == SelectMethodName &&
                IsFirstExpressionInLambdaIsNullChecking(outerMethodSymbol, outerInvocation) &&
                TryGetCastInLambda(SyntaxKind.AsExpression, innerMethodSymbol, innerInvocation, out var typeNameInInner))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageUseInstead, $"'OfType<{typeNameInInner}>()'")));
            }

            if (outerMethodSymbol.Name == SelectMethodName &&
                innerMethodSymbol.Name == WhereMethodName &&
                IsExpressionInLambdaIsCast(outerMethodSymbol, outerInvocation, out var typeNameInOuter) &&
                TryGetCastInLambda(SyntaxKind.IsExpression, innerMethodSymbol, innerInvocation, out typeNameInInner) &&
                typeNameInOuter == typeNameInInner)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageUseInstead, $"'OfType<{typeNameInInner}>()'")));
            }
        }

        private static Location GetReportLocation(InvocationExpressionSyntax invocation)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            return memberAccess == null
                ? invocation.Expression.GetLocation()
                : memberAccess.Name.GetLocation();
        }

        private static bool IsExpressionInLambdaIsCast(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation, out string typeName)
        {
            return TryGetCastInLambda(SyntaxKind.AsExpression, methodSymbol, invocation, out typeName) ||
                TryGetCastInLambda(methodSymbol, invocation, out typeName);
        }

        private static bool IsFirstExpressionInLambdaIsNullChecking(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation)
        {
            var arguments = GetReducedArguments(methodSymbol, invocation);
            if (arguments.Count != 1)
            {
                return false;
            }
            var expression = arguments.First().Expression;

            var binaryExpression = GetExpressionFromLambda(expression).RemoveParentheses() as BinaryExpressionSyntax;
            var lambdaParameter = GetLambdaParameter(expression);

            while (binaryExpression != null)
            {
                if (!binaryExpression.IsKind(SyntaxKind.LogicalAndExpression))
                {
                    return binaryExpression.IsKind(SyntaxKind.NotEqualsExpression) &&
                           IsNullChecking(binaryExpression, lambdaParameter);
                }
                binaryExpression = binaryExpression.Left.RemoveParentheses() as BinaryExpressionSyntax;
            }
            return false;
        }

        private static bool IsNullChecking(BinaryExpressionSyntax binaryExpression, string lambdaParameter)
        {
            if (EquivalenceChecker.AreEquivalent(CSharpSyntaxHelper.NullLiteralExpression, binaryExpression.Left.RemoveParentheses()) &&
                binaryExpression.Right.RemoveParentheses().ToString() == lambdaParameter)
            {
                return true;
            }

            if (EquivalenceChecker.AreEquivalent(CSharpSyntaxHelper.NullLiteralExpression, binaryExpression.Right.RemoveParentheses()) &&
                binaryExpression.Left.RemoveParentheses().ToString() == lambdaParameter)
            {
                return true;
            }

            return false;
        }

        private static ExpressionSyntax GetExpressionFromLambda(ExpressionSyntax expression)
        {
            var lambda = expression as SimpleLambdaExpressionSyntax;
            if (lambda == null)
            {
                var parenthesizedLambda = expression as ParenthesizedLambdaExpressionSyntax;
                return parenthesizedLambda?.Body as ExpressionSyntax;
            }

            return lambda.Body as ExpressionSyntax;
        }
        private static string GetLambdaParameter(ExpressionSyntax expression)
        {
            if (expression is SimpleLambdaExpressionSyntax lambda)
            {
                return lambda.Parameter.Identifier.ValueText;
            }

            var parenthesizedLambda = expression as ParenthesizedLambdaExpressionSyntax;
            if (parenthesizedLambda == null ||
                parenthesizedLambda.ParameterList.Parameters.Count == 0)
            {
                return null;
            }
            return parenthesizedLambda.ParameterList.Parameters.First().Identifier.ValueText;
        }

        private static bool TryGetCastInLambda(SyntaxKind asOrIs, IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation, out string type)
        {
            type = null;
            if (!AsIsSyntaxKinds.Contains(asOrIs))
            {
                return false;
            }

            var arguments = GetReducedArguments(methodSymbol, invocation);
            if (arguments.Count != 1)
            {
                return false;
            }

            var expression = arguments.First().Expression;
            var lambdaBody = GetExpressionFromLambda(expression).RemoveParentheses() as BinaryExpressionSyntax;
            var lambdaParameter = GetLambdaParameter(expression);
            if (lambdaBody == null ||
                lambdaParameter == null ||
                !lambdaBody.IsKind(asOrIs))
            {
                return false;
            }

            var castedExpression = lambdaBody.Left.RemoveParentheses();
            if (lambdaParameter != castedExpression.ToString())
            {
                return false;
            }

            type = lambdaBody.Right.ToString();
            return true;
        }
        private static bool TryGetCastInLambda(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation, out string type)
        {
            type = null;
            var arguments = GetReducedArguments(methodSymbol, invocation);
            if (arguments.Count != 1)
            {
                return false;
            }

            var expression = arguments.First().Expression;
            var castExpression = GetExpressionFromLambda(expression).RemoveParentheses() as CastExpressionSyntax;
            var lambdaParameter = GetLambdaParameter(expression);
            if (castExpression == null ||
                lambdaParameter == null)
            {
                return false;
            }

            var castedExpression = castExpression.Expression.RemoveParentheses();
            if (lambdaParameter != castedExpression.ToString())
            {
                return false;
            }

            type = castExpression.Type.ToString();
            return true;
        }

        private static bool CheckForSimplifiable(IMethodSymbol outerMethodSymbol, InvocationExpressionSyntax outerInvocation,
            IMethodSymbol innerMethodSymbol, InvocationExpressionSyntax innerInvocation, SyntaxNodeAnalysisContext context)
        {
            if (MethodIsNotUsingPredicate(outerMethodSymbol, outerInvocation) &&
                innerMethodSymbol.Name == WhereMethodName &&
                innerMethodSymbol.Parameters.Any(symbol => (symbol.Type as INamedTypeSymbol)?.TypeArguments.Length == 2))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageDropAndChange, WhereMethodName, outerMethodSymbol.Name)));
                return true;
            }

            return false;
        }

        private static bool MethodIsNotUsingPredicate(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation)
        {
            var arguments = GetReducedArguments(methodSymbol, invocation);

            return arguments.Count == 0 && MethodNamesWithPredicate.Contains(methodSymbol.Name);
        }
    }
}
