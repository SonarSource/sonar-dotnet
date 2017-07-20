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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotUseCollectionInItsOwnMethodCalls : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2114";
        private const string MessageFormat = "Change one instance of '{0}' to a different value; {1}";
        private const string AlwaysEmptyCollectionMessage = "This operation always produces an empty collection.";
        private const string AlwaysSameCollectionMessage = "This operation always produces the same collection.";
        private const string AlwaysTrueMessage = "Comparing to itself always returns true.";
        private const string UnexpectedBehaviorMessage = "This operation will probably result in an unexpected behavior.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> trackedMethodNames = ImmutableHashSet.Create("AddRange", "Concat",
            "Union", "Except", "Intersect", "SequenceEqual", "UnionWith", "ExceptWith");

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var invocationExpressionString = invocation.Expression.ToString();

                    if (!trackedMethodNames.Any(method => invocationExpressionString.EndsWith(method,
                        StringComparison.Ordinal)))
                    {
                        return;
                    }

                    var operands = GetOperandsToCheckIfTrackedMethod(invocation, c.SemanticModel);
                    if (operands != null &&
                        EquivalenceChecker.AreEquivalent(operands.Left, operands.Right))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, operands.Left.GetLocation(),
                            additionalLocations: new[] { operands.Right.GetLocation() },
                            messageArgs: new[] { operands.Right.ToString(), operands.ErrorMessage }));
                    }
                }, SyntaxKind.InvocationExpression);
        }

        private static OperandsToCheck GetOperandsToCheckIfTrackedMethod(InvocationExpressionSyntax invocation,
            SemanticModel model)
        {
            var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            bool isEnumerableMethod;
            var message = ProcessIssueMessageFromMethod(methodSymbol, out isEnumerableMethod);
            if (message == null)
            {
                return null;
            }

            if (isEnumerableMethod && methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                return new OperandsToCheck(invocation.ArgumentList.Arguments[0].Expression,
                    invocation.ArgumentList.Arguments[1].Expression, message);
            }

            var invokingExpression = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
            if (invokingExpression != null)
            {
                return new OperandsToCheck(invokingExpression, invocation.ArgumentList.Arguments[0].Expression,
                    message);
            }

            return null;
        }

        private static string ProcessIssueMessageFromMethod(IMethodSymbol methodSymbol, out bool isEnumerableMethod)
        {
            isEnumerableMethod = true;

            if (methodSymbol.IsEnumerableConcat())
            {
                return UnexpectedBehaviorMessage;
            }

            if (methodSymbol.IsEnumerableIntersect() ||
                methodSymbol.IsEnumerableUnion())
            {
                return AlwaysSameCollectionMessage;
            }

            if (methodSymbol.IsEnumerableSequenceEqual())
            {
                return AlwaysTrueMessage;
            }

            if (methodSymbol.IsEnumerableExcept())
            {
                return AlwaysEmptyCollectionMessage;
            }

            isEnumerableMethod = false;
            if (methodSymbol.IsListAddRange())
            {
                return UnexpectedBehaviorMessage;
            }

            if (IsISetUnionWith(methodSymbol))
            {
                return AlwaysSameCollectionMessage;
            }

            if (IsISetExceptWith(methodSymbol))
            {
                return AlwaysEmptyCollectionMessage;
            }

            return null;
        }

        private static bool IsISetUnionWith(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.Name == "UnionWith" &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Parameters.Length == 1 &&
                methodSymbol.ContainingType.Implements(KnownType.System_Collections_Generic_ISet_T);
        }

        private static bool IsISetExceptWith(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.Name == "ExceptWith" &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Parameters.Length == 1 &&
                methodSymbol.ContainingType.Implements(KnownType.System_Collections_Generic_ISet_T);
        }

        private class OperandsToCheck
        {
            public OperandsToCheck(ExpressionSyntax left, ExpressionSyntax right, string errorMessage)
            {
                Left = left.RemoveParentheses();
                Right = right.RemoveParentheses();
                ErrorMessage = errorMessage;
            }

            public ExpressionSyntax Left { get; }
            public ExpressionSyntax Right { get; }
            public string ErrorMessage { get; }
        }
    }
}