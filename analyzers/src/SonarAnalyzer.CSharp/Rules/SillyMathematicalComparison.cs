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
    public sealed class SillyMathematicalComparison : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2198";
        private const string MathComparisonMessage = "Comparison to this constant is useless; the constant is outside the range of type '{0}'";
        private const string ConstantComparisonMessage = "Don't compare constant values";

        private static readonly DiagnosticDescriptor MathComparisonRule = DescriptorFactory.Create(DiagnosticId, MathComparisonMessage);
        private static readonly DiagnosticDescriptor ConstantComparisonRule = DescriptorFactory.Create(DiagnosticId, ConstantComparisonMessage);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MathComparisonRule, ConstantComparisonRule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                CheckBinary,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

        private static void CheckBinary(SonarSyntaxNodeReportingContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;

            var constantLeft = binary.Left.FindConstantValue(context.SemanticModel);
            var constantRight = binary.Right.FindConstantValue(context.SemanticModel);

            if (constantLeft is not null)
            {
                if (constantRight is not null) // both are constants
                {
                    context.ReportIssue(Diagnostic.Create(ConstantComparisonRule, binary.GetLocation()));
                }
                else // left is constant
                {
                    CheckOneSide(context, constantLeft, binary.Right);
                }
            }
            else if (constantRight is not null) // right is constant
            {
                CheckOneSide(context, constantRight, binary.Left);
            }
        }

        private static void CheckOneSide(SonarSyntaxNodeReportingContext context, object constant, SyntaxNode other)
        {
            if (ConversionHelper.TryConvertWith(constant, Convert.ToDouble, out var doubleConstant)
                && context.SemanticModel.GetTypeInfo(other).Type is { } typeSymbolOfOther
                && TryGetRange(typeSymbolOfOther, out var min, out var max)
                && (doubleConstant < min || doubleConstant > max))
            {
                var typeName = context.SemanticModel.GetTypeInfo(other).Type.ToDisplayString();
                context.ReportIssue(Diagnostic.Create(MathComparisonRule, other.Parent.GetLocation(), typeName));
            }
        }

        private static bool TryGetRange(ITypeSymbol typeSymbol, out double min, out double max)
        {
            if (typeSymbol.Is(KnownType.System_Single))
            {
                min = float.MinValue;
                max = float.MaxValue;
                return true;
            }
            if (typeSymbol.Is(KnownType.System_Int64))
            {
                min = long.MinValue;
                max = long.MaxValue;
                return true;
            }
            if (typeSymbol.Is(KnownType.System_UInt64))
            {
                min = ulong.MinValue;
                max = ulong.MaxValue;
                return true;
            }

            min = default;
            max = default;
            return false;
        }
    }
}
