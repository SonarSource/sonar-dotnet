/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryMathematicalComparison : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2198";
        private const string MathComparisonMessage = "Comparison to this constant is useless; the constant is outside the range of type '{0}'";

        private static readonly DiagnosticDescriptor MathComparisonRule = DescriptorFactory.Create(DiagnosticId, MathComparisonMessage);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MathComparisonRule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                CheckComparisonOutOfRange,
                CSharpFacade.Instance.SyntaxKind.ComparisonKinds);

        private static void CheckComparisonOutOfRange(SonarSyntaxNodeReportingContext context)
        {
            if (TryGetConstantValue(context.SemanticModel, (BinaryExpressionSyntax)context.Node, out var constant, out var other)
               && context.SemanticModel.GetTypeInfo(other).Type is { } typeSymbolOfOther
               && TryGetRange(typeSymbolOfOther) is { } range
               && range.IsOutOfRange(constant))
            {
                var typeName = typeSymbolOfOther.ToMinimalDisplayString(context.SemanticModel, other.GetLocation().SourceSpan.Start);
                context.ReportIssue(MathComparisonRule, other.Parent, typeName);
            }
        }

        private static bool TryGetConstantValue(SemanticModel model, BinaryExpressionSyntax binary, out double constant, out SyntaxNode other)
        {
            var optionalLeft = model.GetConstantValue(binary.Left);
            var optionalRight = model.GetConstantValue(binary.Right);

            if (optionalLeft.HasValue ^ optionalRight.HasValue)
            {
                if (optionalLeft.HasValue && TryConvertToDouble(optionalLeft.Value, out constant))
                {
                    other = binary.Right;
                    return true;
                }
                else if (optionalRight.HasValue && TryConvertToDouble(optionalRight.Value, out constant))
                {
                    other = binary.Left;
                    return true;
                }
            }
            constant = default;
            other = default;
            return false;
        }

        // 'char' needs to roundtrip {{char -> int -> double}}, can't go {{char -> double}}
        private static bool TryConvertToDouble(object constant, out double typedConstant) =>
            ConversionHelper.TryConvertWith(constant is char ? Convert.ToInt32(constant) : constant, Convert.ToDouble, out typedConstant)
            && !double.IsInfinity(typedConstant);

        private static ValuesRange? TryGetRange(ITypeSymbol typeSymbol) =>
            typeSymbol switch
            {
                _ when typeSymbol.Is(KnownType.System_Char) => new(char.MinValue, char.MaxValue),
                _ when typeSymbol.Is(KnownType.System_Single) => new(float.MinValue, float.MaxValue),
                _ when typeSymbol.Is(KnownType.System_Int64) => new(long.MinValue, long.MaxValue),
                _ when typeSymbol.Is(KnownType.System_UInt64) => new(ulong.MinValue, ulong.MaxValue),
                _ => null,
            };

        private readonly record struct ValuesRange(double Min, double Max)
        {
            public bool IsOutOfRange(double value) =>
               value < Min || value > Max;
        }
    }
}
