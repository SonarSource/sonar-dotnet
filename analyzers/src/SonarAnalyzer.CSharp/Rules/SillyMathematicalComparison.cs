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

        private static readonly Dictionary<KnownType, ValuesRange> ValuesRanges = new()
        {
            {KnownType.System_Char, new ValuesRange(char.MinValue, char.MaxValue) },
            {KnownType.System_Single, new ValuesRange(float.MinValue, float.MaxValue) },
            {KnownType.System_Int64, new ValuesRange(long.MinValue, long.MaxValue) },
            {KnownType.System_UInt64, new ValuesRange(ulong.MinValue, ulong.MaxValue) },
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MathComparisonRule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                CheckBinary,
                CSharpFacade.Instance.SyntaxKind.ComparisonKinds);

        private static void CheckBinary(SonarSyntaxNodeReportingContext context)
        {
            if (TryGetConstantValue(context.SemanticModel, (BinaryExpressionSyntax)context.Node, out var constant, out var other)
               && context.SemanticModel.GetTypeInfo(other).Type is { } typeSymbolOfOther
               && TryGetRange(typeSymbolOfOther) is { } range
               && (constant < range.Min || constant > range.Max))
            {
                var typeName = typeSymbolOfOther.ToMinimalDisplayString(context.SemanticModel, other.GetLocation().SourceSpan.Start);
                context.ReportIssue(Diagnostic.Create(MathComparisonRule, other.Parent.GetLocation(), typeName));
            }
        }

        private static bool TryGetConstantValue(SemanticModel model, BinaryExpressionSyntax binary, out double constant, out SyntaxNode other)
        {
            constant = default;
            other = default;
            var optionalLeft = model.GetConstantValue(binary.Left);
            var optionalRight = model.GetConstantValue(binary.Right);

            if (optionalLeft.HasValue ^ optionalRight.HasValue)
            {
                if (optionalLeft.HasValue && TryConvertToDouble(optionalLeft.Value, out constant))
                {
                    other = binary.Right;
                    return true;
                }
                else if (TryConvertToDouble(optionalRight.Value, out constant))
                {
                    other = binary.Left;
                    return true;
                }
            }
            return false;
        }

        // 'char' needs to roundtrip {{char -> int -> double}}, can't go {{char -> double}}
        private static bool TryConvertToDouble(object constant, out double typedConstant) =>
            ConversionHelper.TryConvertWith(constant is char ? Convert.ToInt32(constant) : constant, Convert.ToDouble, out typedConstant);

        private static ValuesRange TryGetRange(ITypeSymbol typeSymbol) =>
            ValuesRanges.FirstOrDefault(x => typeSymbol.Is(x.Key)).Value;

        private sealed record ValuesRange(double Min, double Max);
    }
}
