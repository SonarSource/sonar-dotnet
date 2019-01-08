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

using System.Collections.Immutable;
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
    public sealed class IndexOfCheckAgainstZero : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2692";
        private const string MessageFormat = "0 is a valid index, but this check ignores it.";

        private static readonly ImmutableArray<KnownType> CheckedTypes =
            ImmutableArray.Create(
                KnownType.System_Array,
                KnownType.System_Collections_Generic_IList_T,
                KnownType.System_String,
                KnownType.System_Collections_IList
            );

    private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var lessThan = (BinaryExpressionSyntax) c.Node;
                    if (ExpressionNumericConverter.TryGetConstantIntValue(lessThan.Left, out var constValue) &&
                        constValue == 0 &&
                        IsIndexOfCall(lessThan.Right, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, Location.Create(lessThan.SyntaxTree,
                            TextSpan.FromBounds(lessThan.Left.SpanStart, lessThan.OperatorToken.Span.End))));
                    }
                },
                SyntaxKind.LessThanExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var greaterThan = (BinaryExpressionSyntax)c.Node;
                    if (ExpressionNumericConverter.TryGetConstantIntValue(greaterThan.Right, out var constValue) &&
                        constValue == 0 &&
                        IsIndexOfCall(greaterThan.Left, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, Location.Create(greaterThan.SyntaxTree,
                            TextSpan.FromBounds(greaterThan.OperatorToken.SpanStart, greaterThan.Right.Span.End))));
                    }
                },
                SyntaxKind.GreaterThanExpression);
        }

        private static bool IsIndexOfCall(ExpressionSyntax call, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetSymbolInfo(call).Symbol is IMethodSymbol indexOfSymbol) ||
                indexOfSymbol.Name != "IndexOf")
            {
                return false;
            }

            return indexOfSymbol.ContainingType.DerivesOrImplementsAny(CheckedTypes);
        }
    }
}
