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

        private static readonly DiagnosticDescriptor MathComparisonRule = DescriptorFactory.Create(DiagnosticId, MathComparisonMessage);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MathComparisonRule);

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
            if (TryGetConstantValue(context, (BinaryExpressionSyntax)context.Node, out var constant, out var other)
               && context.SemanticModel.GetTypeInfo(other).Type is { } typeSymbolOfOther
               && TryGetRange(typeSymbolOfOther, out var min, out var max)
               && (constant < min || constant > max))
            {
                var typeName = context.SemanticModel.GetTypeInfo(other).Type.ToMinimalDisplayString(context.SemanticModel, other.GetLocation().SourceSpan.Start);
                context.ReportIssue(Diagnostic.Create(MathComparisonRule, other.Parent.GetLocation(), typeName));
            }
        }

        private static bool TryGetConstantValue(SonarSyntaxNodeReportingContext context, BinaryExpressionSyntax binary, out double constant, out SyntaxNode other)
        {
            constant = default;
            other = default;
            var maybeLeft = context.SemanticModel.GetConstantValue(binary.Left);
            var maybeRight = context.SemanticModel.GetConstantValue(binary.Right);

            if (maybeLeft.HasValue ^ maybeRight.HasValue)
            {
                if (maybeLeft.HasValue)
                {
                    ConversionHelper.TryConvertWith(maybeLeft.Value, Convert.ToDouble, out constant);
                    other = binary.Right;
                    return true;
                }
                else
                {
                    ConversionHelper.TryConvertWith(maybeRight.Value, Convert.ToDouble, out constant);
                    other = binary.Left;
                    return true;
                }
            }
            return false;
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
