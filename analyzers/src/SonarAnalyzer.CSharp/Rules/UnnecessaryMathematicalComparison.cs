/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
            if (TryGetConstantValue(context.Model, (BinaryExpressionSyntax)context.Node, out var constant, out var other)
               && context.Model.GetTypeInfo(other).Type is { } typeSymbolOfOther
               && TryGetRange(typeSymbolOfOther) is { } range
               && range.IsOutOfRange(constant))
            {
                var typeName = typeSymbolOfOther.ToMinimalDisplayString(context.Model, other.GetLocation().SourceSpan.Start);
                context.ReportIssue(MathComparisonRule, other.Parent, typeName);
            }
        }

        private static bool TryGetConstantValue(SemanticModel model, BinaryExpressionSyntax binary, out double constant, out SyntaxNode other)
        {
            var optionalLeft = model.GetConstantValue(binary.Left);
            var optionalRight = model.GetConstantValue(binary.Right);

            if (optionalLeft.HasValue ^ optionalRight.HasValue)
            {
                if (optionalLeft.HasValue && Conversions.ToDouble(optionalLeft.Value) is { } left)
                {
                    constant = left;
                    other = binary.Right;
                    return true;
                }
                else if (optionalRight.HasValue && Conversions.ToDouble(optionalRight.Value) is { } right)
                {
                    constant = right;
                    other = binary.Left;
                    return true;
                }
            }
            constant = default;
            other = null;
            return false;
        }

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
