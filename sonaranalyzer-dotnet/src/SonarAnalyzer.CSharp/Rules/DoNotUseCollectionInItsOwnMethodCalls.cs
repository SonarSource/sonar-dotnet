/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        private const string AlwaysFalseMessage = "Comparing to itself always returns false.";
        private const string UnexpectedBehaviorMessage = "This operation will probably result in an unexpected behavior.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> trackedMethodNames = new HashSet<string> {"AddRange", "Concat", "Except",
            "ExceptWith", "Intersect", "IntersectWith", "IsSubsetOf", "IsSupersetOf", "IsProperSubsetOf", "IsProperSupersetOf",
            "Overlaps", "SequenceEqual", "SetEquals", "SymmetricExceptWith", "Union", "UnionWith" };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var invocationExpressionString = invocation.Expression.ToString();

                    if (!trackedMethodNames.Any(method =>
                            invocationExpressionString.EndsWith(method, StringComparison.Ordinal)))
                    {
                        return;
                    }

                    var operands = GetOperandsToCheckIfTrackedMethod(invocation, c.SemanticModel);
                    if (operands != null &&
                        CSharpEquivalenceChecker.AreEquivalent(operands.Left, operands.Right))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, operands.Left.GetLocation(),
                            additionalLocations: new[] { operands.Right.GetLocation() },
                            messageArgs: new[] { operands.Right.ToString(), operands.ErrorMessage }));
                    }
                }, SyntaxKind.InvocationExpression);
        }

        private static OperandsToCheck GetOperandsToCheckIfTrackedMethod(InvocationExpressionSyntax invocation,
            SemanticModel model)
        {
            var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            var message = ProcessIssueMessageFromMethod(methodSymbol, out var isEnumerableMethod);
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

            if (IsISetUnionWithImplementation(methodSymbol) ||
                IsISetIntersectWithImplementation(methodSymbol))
            {
                return AlwaysSameCollectionMessage;
            }

            if (IsISetExceptWithImplementation(methodSymbol) ||
                IsISetSymmetricExceptWithImplementation(methodSymbol))
            {
                return AlwaysEmptyCollectionMessage;
            }

            if (IsISetIsSubsetOfImplementation(methodSymbol) ||
                IsISetIsSupersetOfImplementation(methodSymbol) ||
                IsISetOverlapsImplementation(methodSymbol) ||
                IsISetSetEqualsImplementation(methodSymbol))
            {
                return AlwaysTrueMessage;
            }

            if (IsISetIsProperSubsetOfImplementation(methodSymbol) ||
                IsISetIsProperSupersetOfImplementation(methodSymbol))
            {
                return AlwaysFalseMessage;
            }

            return null;
        }

        private static bool IsISetUnionWithImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "UnionWith");
        private static bool IsISetExceptWithImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "ExceptWith");
        private static bool IsISetIntersectWithImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "IntersectWith");
        private static bool IsISetIsProperSubsetOfImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "IsProperSubsetOf");
        private static bool IsISetIsProperSupersetOfImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "IsProperSupersetOf");
        private static bool IsISetIsSubsetOfImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "IsSubsetOf");
        private static bool IsISetIsSupersetOfImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "IsSupersetOf");
        private static bool IsISetOverlapsImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "Overlaps");
        private static bool IsISetSetEqualsImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "SetEquals");
        private static bool IsISetSymmetricExceptWithImplementation(IMethodSymbol methodSymbol) =>
            IsISetMethodImplementation(methodSymbol, "SymmetricExceptWith");

        private static bool IsISetMethodImplementation(IMethodSymbol methodSymbol, string methodName)
        {
            return methodSymbol != null &&
                methodSymbol.Name == methodName &&
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
