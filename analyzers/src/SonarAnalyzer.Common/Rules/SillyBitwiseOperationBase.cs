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

namespace SonarAnalyzer.Rules
{
    // FIXME: Rename this to something like SillyNumericalOperatorUsage
    public abstract class SillyBitwiseOperationBase : SonarDiagnosticAnalyzer
    {
        internal const string BitwiseDiagnosticId = "S2437";
        internal const string IsReportingOnLeftKey = "IsReportingOnLeft";
        private const string BitwiseMessageFormat = "Remove this silly bit operation.";

        // No codefix (yet?)
        private const string ComparisonDiagnosticId = "S2198";
        private const string ComparisonMessageFormat = "Remove this silly mathematical comparison.";

        protected abstract ILanguageFacade Language { get; }

        protected DiagnosticDescriptor BitwiseRule { get; }
        protected DiagnosticDescriptor ComparisonRule { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(BitwiseRule, ComparisonRule);

        protected SillyBitwiseOperationBase()
        {
            BitwiseRule = Language.CreateDescriptor(BitwiseDiagnosticId, BitwiseMessageFormat, fadeOutCode: true);
            ComparisonRule = Language.CreateDescriptor(ComparisonDiagnosticId, ComparisonMessageFormat, fadeOutCode: true);
        }

        protected void CheckBinary(SonarSyntaxNodeReportingContext context, SyntaxNode left, SyntaxToken @operator, SyntaxNode right, int constValueToLookFor)
        {
            Location location;
            bool isReportingOnLeftKey;
            if (FindIntConstant(context.SemanticModel, left) is { } valueLeft && valueLeft == constValueToLookFor)
            {
                location = left.CreateLocation(@operator);
                isReportingOnLeftKey = true;
            }
            else if (FindIntConstant(context.SemanticModel, right) is { } valueRight && valueRight == constValueToLookFor)
            {
                location = @operator.CreateLocation(right);
                isReportingOnLeftKey = false;
            }
            else
            {
                return;
            }

            context.ReportIssue(Diagnostic.Create(BitwiseRule, location, ImmutableDictionary<string, string>.Empty.Add(IsReportingOnLeftKey, isReportingOnLeftKey.ToString())));
        }

        protected int? FindIntConstant(SemanticModel semanticModel, SyntaxNode node) =>
            semanticModel.GetSymbolInfo(node).Symbol is var symbol
            && !IsFieldOrPropertyOutsideSystemNamespace(symbol)
            && !IsEnum(symbol)
            && Language.FindConstantValue(semanticModel, node) is { } value
                ? ConversionHelper.TryConvertToInt(value)
                : null;

        protected T? FindConstant<T>(SemanticModel semanticModel, SyntaxNode node, Func<object, T> converter) where T : struct
        {
            return semanticModel.GetSymbolInfo(node).Symbol is var symbol
                && !IsFieldOrPropertyOutsideSystemNamespace(symbol)
                && !IsEnum(symbol)
                && Language.FindConstantValue(semanticModel, node) is { } value
                && ConversionHelper.TryConvertWith(value, converter, out T typedValue)
                ? typedValue
                : null;
        }

        private static bool IsEnum(ISymbol symbol) =>
            symbol.GetSymbolType() is INamedTypeSymbol { EnumUnderlyingType: { } };

        private static bool IsFieldOrPropertyOutsideSystemNamespace(ISymbol symbol) =>
            symbol is { Kind: SymbolKind.Field or SymbolKind.Property }
            && symbol.ContainingNamespace.Name != nameof(System);
    }
}
