/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CollectionQuerySimplification : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2971";
        private const string MessageFormat = "{0}";
        internal const string MessageUseInstead = "Use {0} here instead.";
        internal const string MessageDropAndChange = "Drop '{0}' and move the condition into the '{1}'.";
        internal const string MessageDropFromMiddle = "Drop this useless call to '{0}' or replace it by 'AsEnumerable' if " +
            "you are using LINQ to Entities.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> MethodNamesWithPredicate = new HashSet<string>
        {
            "Any", "LongCount", "Count",
            "First", "FirstOrDefault", "Last", "LastOrDefault",
            "Single", "SingleOrDefault",
        };

        private static readonly ISet<string> MethodNamesForTypeCheckingWithSelect = new HashSet<string>
        {
            "Any", "LongCount", "Count",
            "First", "FirstOrDefault", "Last", "LastOrDefault",
            "Single", "SingleOrDefault", "SkipWhile", "TakeWhile",
        };

        private static readonly ISet<string> MethodNamesToCollection = new HashSet<string>
        {
            "ToList",
            "ToArray",
        };

        private static readonly ISet<string> IgnoredMethodNames = new HashSet<string>
        {
            "AsEnumerable", // ignored as it is somewhat cleaner way to cast to IEnumerable<T> and has no side effects
        };

        private static readonly ISet<SyntaxKind> AsIsSyntaxKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.AsExpression,
            SyntaxKind.IsExpression
        };

        private const string WhereMethodName = "Where";
        private const string SelectMethodName = "Select";

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
            if (invocation.ArgumentList?.Arguments.Count == 0 &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.ValueText == CountName &&
                context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
                methodSymbol.Name == CountName &&
                methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T) &&
                HasCountProperty(memberAccess.Expression, context.SemanticModel))
            {
                context.ReportIssue(CreateDiagnostic(rule, GetReportLocation(invocation),
                    string.Format(MessageUseInstead, $"'{CountName}' property")));
            }

            static bool HasCountProperty(ExpressionSyntax expression, SemanticModel semanticModel) =>
                semanticModel.GetTypeInfo(expression).Type.GetMembers(CountName).OfType<IPropertySymbol>().Any();
        }

        private static void CheckToCollectionCalls(SonarSyntaxNodeReportingContext context)
        {
            var outerInvocation = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(outerInvocation).Symbol is IMethodSymbol outerMethodSymbol) ||
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
                context.ReportIssue(CreateDiagnostic(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageDropFromMiddle, innerMethodSymbol.Name)));
            }
        }

        private static bool MethodExistsOnIEnumerable(IMethodSymbol methodSymbol, SemanticModel semanticModel)
        {
            if (IgnoredMethodNames.Contains(methodSymbol.Name))
            {
                return false;
            }

            if (methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return true;
            }

            var enumerableType = semanticModel.Compilation.GetTypeByMetadataName(KnownType.System_Linq_Enumerable);
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

        private static void CheckExtensionMethodsOnIEnumerable(SonarSyntaxNodeReportingContext context)
        {
            var outerInvocation = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(outerInvocation).Symbol is IMethodSymbol outerMethodSymbol) ||
                !outerMethodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return;
            }

            var innerInvocation = GetInnerInvocation(outerInvocation, outerMethodSymbol);
            if (innerInvocation == null)
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbolInfo(innerInvocation).Symbol is IMethodSymbol innerMethodSymbol) ||
                !innerMethodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return;
            }

            if (CheckForSimplifiable(context, outerMethodSymbol, outerInvocation, innerMethodSymbol, innerInvocation))
            {
                return;
            }

            CheckForCastSimplification(context, outerMethodSymbol, outerInvocation, innerMethodSymbol, innerInvocation);
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

        private static void CheckForCastSimplification(SonarSyntaxNodeReportingContext context,
                                                       IMethodSymbol outerMethodSymbol,
                                                       InvocationExpressionSyntax outerInvocation,
                                                       IMethodSymbol innerMethodSymbol,
                                                       InvocationExpressionSyntax innerInvocation)
        {
            if (MethodNamesForTypeCheckingWithSelect.Contains(outerMethodSymbol.Name) &&
                innerMethodSymbol.Name == SelectMethodName &&
                IsFirstExpressionInLambdaIsNullChecking(outerMethodSymbol, outerInvocation) &&
                TryGetCastInLambda(SyntaxKind.AsExpression, innerMethodSymbol, innerInvocation, out var typeNameInInner))
            {
                context.ReportIssue(CreateDiagnostic(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageUseInstead, $"'OfType<{typeNameInInner}>()'")));
            }

            if (outerMethodSymbol.Name == SelectMethodName &&
                innerMethodSymbol.Name == WhereMethodName &&
                IsExpressionInLambdaIsCast(outerMethodSymbol, outerInvocation, out var typeNameInOuter) &&
                TryGetCastInLambda(SyntaxKind.IsExpression, innerMethodSymbol, innerInvocation, out typeNameInInner) &&
                typeNameInOuter == typeNameInInner)
            {
                context.ReportIssue(CreateDiagnostic(rule, GetReportLocation(innerInvocation),
                    string.Format(MessageUseInstead, $"'OfType<{typeNameInInner}>()'")));
            }
        }

        private static Location GetReportLocation(InvocationExpressionSyntax invocation)
        {
            return !(invocation.Expression is MemberAccessExpressionSyntax memberAccess)
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
            if (CSharpEquivalenceChecker.AreEquivalent(CSharpSyntaxHelper.NullLiteralExpression, binaryExpression.Left.RemoveParentheses()) &&
                binaryExpression.Right.RemoveParentheses().ToString() == lambdaParameter)
            {
                return true;
            }

            if (CSharpEquivalenceChecker.AreEquivalent(CSharpSyntaxHelper.NullLiteralExpression, binaryExpression.Right.RemoveParentheses()) &&
                binaryExpression.Left.RemoveParentheses().ToString() == lambdaParameter)
            {
                return true;
            }

            return false;
        }

        private static ExpressionSyntax GetExpressionFromLambda(ExpressionSyntax expression)
        {
            if (!(expression is SimpleLambdaExpressionSyntax lambda))
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

            if (!(expression is ParenthesizedLambdaExpressionSyntax parenthesizedLambda) ||
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
            var lambdaParameter = GetLambdaParameter(expression);
            if (!(GetExpressionFromLambda(expression).RemoveParentheses() is BinaryExpressionSyntax lambdaBody) ||
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
            var lambdaParameter = GetLambdaParameter(expression);
            if (!(GetExpressionFromLambda(expression).RemoveParentheses() is CastExpressionSyntax castExpression) ||
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

        private static bool CheckForSimplifiable(SonarSyntaxNodeReportingContext context,
                                                 IMethodSymbol outerMethodSymbol,
                                                 InvocationExpressionSyntax outerInvocation,
                                                 IMethodSymbol innerMethodSymbol,
                                                 InvocationExpressionSyntax innerInvocation)
        {
            if (MethodIsNotUsingPredicate(outerMethodSymbol, outerInvocation) &&
                innerMethodSymbol.Name == WhereMethodName &&
                innerMethodSymbol.Parameters.Any(symbol => (symbol.Type as INamedTypeSymbol)?.TypeArguments.Length == 2))
            {
                context.ReportIssue(CreateDiagnostic(rule, GetReportLocation(innerInvocation),
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
