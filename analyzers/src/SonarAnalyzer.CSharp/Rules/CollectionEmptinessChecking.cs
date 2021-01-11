/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CollectionEmptinessChecking : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1155";
        private const string MessageFormat = "Use '.Any()' to test whether this 'IEnumerable<{0}>' is empty or not.";

        private static readonly CSharpExpressionNumericConverter ExpressionNumericConverter = new CSharpExpressionNumericConverter();

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckCount((BinaryExpressionSyntax)c.Node, c),
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

        private static void CheckCount(BinaryExpressionSyntax binary, SyntaxNodeAnalysisContext context)
        {
            var countType = CountType.FromExpression(binary);
            if (countType.IsValid)
            {
                var count = countType.Left.HasValue ? binary.Right : binary.Left;
                if (TryGetCountCall(count, context.SemanticModel, out var reportLocation, out var typeArgument))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, reportLocation, typeArgument));
                }
            }
        }
        private static bool TryGetCountCall(ExpressionSyntax expression, SemanticModel semanticModel, out Location countLocation, out string typeArgument)
        {
            countLocation = null;
            typeArgument = null;
            var invocation = expression as InvocationExpressionSyntax;
            if (!(invocation?.Expression is MemberAccessExpressionSyntax memberAccess))
            {
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(memberAccess).Symbol is IMethodSymbol methodSymbol) ||
                !IsMethodCountExtension(methodSymbol) ||
                !methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                return false;
            }

            if (methodSymbol.IsGenericMethod)
            {
                typeArgument = methodSymbol.TypeArguments.First().ToDisplayString();
            }

            countLocation = memberAccess.Name.GetLocation();
            return true;
        }

        private static bool IsMethodCountExtension(IMethodSymbol methodSymbol) =>
            methodSymbol.Name == "Count" 
            && methodSymbol.IsExtensionMethod 
            && methodSymbol.ReceiverType != null;

        internal readonly struct CountType
        {
            public CountType(int? left, SyntaxKind logical, int? right)
            {
                Left = left;
                Right = right;
                LogicalOperator = logical;
            }

            public int? Left { get; }
            public int? Right { get; }
            public SyntaxKind LogicalOperator { get; }
            public bool IsValid => (Left.HasValue ^ Right.HasValue) && (IsEmpty || HasAny);
            public bool IsEmpty => Empties.Contains(this);
            public bool HasAny => Anys.Contains(this);
            public override string ToString() => $"{Left} {LogicalOperator} {Right}";
            public static CountType FromExpression(BinaryExpressionSyntax binary)
            {
                int? left = default;
                int? right = default;
                if (binary.Left is LiteralExpressionSyntax l && ExpressionNumericConverter.TryGetConstantIntValue(l, out var l_out))
                {
                    left = l_out;
                };
                if (binary.Right is LiteralExpressionSyntax r && ExpressionNumericConverter.TryGetConstantIntValue(r, out var r_out))
                {
                    right = r_out;
                };
                return new CountType(left, binary.Kind(), right);
            }

            private static int? Count() => default;
            private static readonly CountType[] Empties = new[]
            {
                new CountType(Count(), SyntaxKind.EqualsExpression, 0),
                new CountType(Count(), SyntaxKind.LessThanExpression, 1),
                new CountType(Count(), SyntaxKind.LessThanOrEqualExpression, 0),
                new CountType(0, SyntaxKind.EqualsExpression, Count()),
                new CountType(1, SyntaxKind.GreaterThanExpression, Count()),
                new CountType(0, SyntaxKind.GreaterThanOrEqualExpression, Count()),
            };
            private static readonly CountType[] Anys = new[]
            {
                new CountType(Count(), SyntaxKind.NotEqualsExpression, 0),
                new CountType(Count(), SyntaxKind.GreaterThanExpression, 0),
                new CountType(Count(), SyntaxKind.GreaterThanOrEqualExpression, 1),
                new CountType(0, SyntaxKind.NotEqualsExpression, Count()),
                new CountType(0, SyntaxKind.LessThanExpression, Count()),
                new CountType(1, SyntaxKind.LessThanOrEqualExpression, Count()),
            };
        }
    }
}
