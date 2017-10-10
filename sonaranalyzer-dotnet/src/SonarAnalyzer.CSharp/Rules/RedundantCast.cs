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
    public class RedundantCast : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1905";
        private const string MessageFormat = "Remove this unnecessary cast to '{0}'.";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> CastIEnumerableMethods = ImmutableHashSet.Create(
            "Cast",
            "OfType");

    protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var castExpression = (CastExpressionSyntax)c.Node;
                    CheckCastExpression(c, castExpression.Expression, castExpression.Type, castExpression.Type.GetLocation());
                },
                SyntaxKind.CastExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var castExpression = (BinaryExpressionSyntax)c.Node;
                    CheckCastExpression(c, castExpression.Left, castExpression.Right,
                        Location.Create(c.Node.SyntaxTree,
                            TextSpan.FromBounds(castExpression.OperatorToken.SpanStart, castExpression.Right.Span.End)));
                },
                SyntaxKind.AsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExtensionMethodInvocation(c),
                SyntaxKind.InvocationExpression);
        }

        private static void CheckCastExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression,
            ExpressionSyntax type, Location location)
        {
            var expressionType = context.SemanticModel.GetTypeInfo(expression).Type;
            if (expressionType == null)
            {
                return;
            }

            var castType = context.SemanticModel.GetTypeInfo(type).Type;
            if (castType == null)
            {
                return;
            }

            if (expressionType.Equals(castType))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location,
                    castType.ToMinimalDisplayString(context.SemanticModel, expression.SpanStart)));
            }
        }

        private static void CheckExtensionMethodInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null ||
                !methodSymbol.IsExtensionOn(KnownType.System_Collections_IEnumerable) ||
                !CastIEnumerableMethods.Contains(methodSymbol.Name))
            {
                return;
            }

            var elementType = GetElementType(invocation, methodSymbol, context.SemanticModel);
            if (elementType == null)
            {
                return;
            }

            var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
            if (returnType == null ||
                !returnType.TypeArguments.Any())
            {
                return;
            }

            var castType = returnType.TypeArguments.First();

            if (elementType.Equals(castType))
            {
                var methodCalledAsStatic = methodSymbol.MethodKind == MethodKind.Ordinary;
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetReportLocation(invocation, methodCalledAsStatic),
                    returnType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)));
            }
        }

        private static Location GetReportLocation(InvocationExpressionSyntax invocation, bool methodCalledAsStatic)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return invocation.Expression.GetLocation();
            }

            return methodCalledAsStatic
                ? memberAccess.GetLocation()
                : Location.Create(invocation.SyntaxTree,
                    new TextSpan(memberAccess.OperatorToken.SpanStart, invocation.Span.End - memberAccess.OperatorToken.SpanStart));
        }

        private static ITypeSymbol GetElementType(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol,
            SemanticModel semanticModel)
        {
            ExpressionSyntax collection;
            if (methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                if (!invocation.ArgumentList.Arguments.Any())
                {
                    return null;
                }
                collection = invocation.ArgumentList.Arguments.First().Expression;
            }
            else
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null)
                {
                    return null;
                }
                collection = memberAccess.Expression;
            }

            var collectionType = semanticModel.GetTypeInfo(collection).Type as INamedTypeSymbol;
            if (collectionType != null &&
                collectionType.TypeArguments.Length == 1)
            {
                return collectionType.TypeArguments.First();
            }

            var arrayType = semanticModel.GetTypeInfo(collection).Type as IArrayTypeSymbol;
            return arrayType?.ElementType;
        }
    }
}
