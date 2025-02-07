/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class UnnecessaryBitwiseOperationBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2437";
        internal const string IsReportingOnLeftKey = "IsReportingOnLeft";
        private const string MessageFormat = "Remove this unnecessary bit operation.";

        protected abstract ILanguageFacade Language { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        protected DiagnosticDescriptor Rule { get; }

        protected UnnecessaryBitwiseOperationBase() =>
            Rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, fadeOutCode: true);

        protected void CheckBinary(SonarSyntaxNodeReportingContext context, SyntaxNode left, SyntaxToken @operator, SyntaxNode right, int constValueToLookFor)
        {
            Location location;
            bool isReportingOnLeftKey;
            if (FindIntConstant(context.Model, left) is { } valueLeft && valueLeft == constValueToLookFor)
            {
                location = left.CreateLocation(@operator);
                isReportingOnLeftKey = true;
            }
            else if (FindIntConstant(context.Model, right) is { } valueRight && valueRight == constValueToLookFor)
            {
                location = @operator.CreateLocation(right);
                isReportingOnLeftKey = false;
            }
            else
            {
                return;
            }

            context.ReportIssue(Rule, location, ImmutableDictionary<string, string>.Empty.Add(IsReportingOnLeftKey, isReportingOnLeftKey.ToString()));
        }

        protected int? FindIntConstant(SemanticModel semanticModel, SyntaxNode node) =>
            semanticModel.GetSymbolInfo(node).Symbol is var symbol
            && !IsFieldOrPropertyOutsideSystemNamespace(symbol)
            && !symbol.GetSymbolType().IsEnum()
            && Language.FindConstantValue(semanticModel, node) is { } value
                ? ConversionHelper.TryConvertToInt(value)
                : null;

        private static bool IsFieldOrPropertyOutsideSystemNamespace(ISymbol symbol) =>
            symbol is { Kind: SymbolKind.Field or SymbolKind.Property }
            && symbol.ContainingNamespace.Name != nameof(System);
    }
}
