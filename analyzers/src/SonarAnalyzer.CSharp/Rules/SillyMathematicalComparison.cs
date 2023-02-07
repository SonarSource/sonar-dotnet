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
        private const string MessageFormat = "Comparison to this constant is useless; the constant is outside the range of type '{0}'";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
            CheckOneSide(context, binary.Left, binary.Right);
            CheckOneSide(context, binary.Right, binary.Left);
        }

        private static void CheckOneSide(SonarSyntaxNodeReportingContext context, SyntaxNode first, SyntaxNode second)
        {
            if (ShouldRaise(context.SemanticModel, first, second))
            {
                var typeName = context.SemanticModel.GetTypeInfo(second).Type.ToDisplayString();
                context.ReportIssue(Diagnostic.Create(Rule, first.Parent.GetLocation(), typeName));
            }
        }

        private static bool ShouldRaise(SemanticModel model, SyntaxNode expectedConstant, SyntaxNode other) =>
            TryGetConstant(model, expectedConstant, out var constant)
            && model.GetTypeInfo(other).Type is { } symbol
            && TryGetRange(symbol, out var min, out var max)
            && (constant < min || constant > max);

        private static bool TryGetConstant(SemanticModel model, SyntaxNode expectedConstant, out double constant)
        {
            if (FindConstant(model, expectedConstant) is { } constValue)
            {
                constant = constValue;
                return true;
            }

            constant = default;
            return false;
        }

        private static double? FindConstant(SemanticModel semanticModel, SyntaxNode node) =>
            node.FindConstantValue(semanticModel) is { } value
            && ConversionHelper.TryConvertWith(value, Convert.ToDouble, out var typedValue)
                ? typedValue
                : null;

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
