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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SillyMathematicalComparison : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2198";
        private const string MessageFormat = "Comparison to integral constant is useless; the constant is outside the range of type '{0}'";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                CheckBinary,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression);

        private static void CheckBinary(SonarSyntaxNodeReportingContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            var typeIdentifier = string.Empty;
            if (ShouldRaise(context.SemanticModel, binary.Left, binary.Right, ref typeIdentifier))
            {
                context.ReportIssue(Diagnostic.Create(Rule, binary.GetLocation(), typeIdentifier));
            }

            if (ShouldRaise(context.SemanticModel, binary.Right, binary.Left, ref typeIdentifier))
            {
                context.ReportIssue(Diagnostic.Create(Rule, binary.GetLocation(), typeIdentifier));
            }
        }

        private static bool ShouldRaise(SemanticModel model, SyntaxNode expectedConstant, SyntaxNode other, ref string typeIdentifier) =>
            TryGetConstant(model, expectedConstant, out var constant)
            && model.GetSymbolInfo(other).Symbol.GetSymbolType() is { } symbol
            && TryGetRange(symbol, out typeIdentifier, out var min, out var max)
            && (constant < min || constant > max);

        private static bool TryGetConstant(SemanticModel model, SyntaxNode expectedConstant, out double constant)
        {
            if (FindConstant(model, expectedConstant, Convert.ToDouble) is { } constValue)
            {
                constant = constValue;
                return true;
            }

            constant = default;
            return false;
        }

        private static T? FindConstant<T>(SemanticModel semanticModel, SyntaxNode node, Func<object, T> converter) where T : struct
            => semanticModel.GetSymbolInfo(node).Symbol is var symbol
                && !IsFieldOrPropertyOutsideSystemNamespace(symbol)
                && !IsEnum(symbol)
                && CSharpFacade.Instance.FindConstantValue(semanticModel, node) is { } value
                && ConversionHelper.TryConvertWith(value, converter, out T typedValue)
                ? typedValue
                : null;

        private static bool TryGetRange(ITypeSymbol typeSymbol, out string typeIdentifier, out double min, out double max)
        {
            if (typeSymbol.Is(KnownType.System_Single))
            {
                typeIdentifier = "float";
                min = float.MinValue;
                max = float.MaxValue;
                return true;
            }

            typeIdentifier = default;
            min = default;
            max = default;
            return false;
        }

        private static bool IsFieldOrPropertyOutsideSystemNamespace(ISymbol symbol) =>
            symbol is { Kind: SymbolKind.Field or SymbolKind.Property }
            && symbol.ContainingNamespace.Name != nameof(System);

        private static bool IsEnum(ISymbol symbol) =>
            symbol.GetSymbolType() is INamedTypeSymbol { EnumUnderlyingType: { } };
    }
}
