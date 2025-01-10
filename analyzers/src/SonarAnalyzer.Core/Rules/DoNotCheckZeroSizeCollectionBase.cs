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

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotCheckZeroSizeCollectionBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S3981";
        private const string CountName = nameof(Enumerable.Count);
        private const string LengthName = nameof(Array.Length);
        private const string LongLengthName = nameof(Array.LongLength);

        protected abstract string IEnumerableTString { get; }

        protected override string MessageFormat => "The '{0}' of '{1}' always evaluates as '{2}' regardless the size.";

        protected DoNotCheckZeroSizeCollectionBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var binary = c.Node;
                    var binaryLeft = Language.Syntax.BinaryExpressionLeft(binary);
                    var binaryRight = Language.Syntax.BinaryExpressionRight(binary);

                    if (Language.ExpressionNumericConverter.TryGetConstantIntValue(c.SemanticModel, binaryLeft, out var left))
                    {
                        CheckExpression(c, binary, binaryRight, left, Language.Syntax.ComparisonKind(binary).Mirror());
                    }
                    else if (Language.ExpressionNumericConverter.TryGetConstantIntValue(c.SemanticModel, binaryRight, out var right))
                    {
                        CheckExpression(c, binary, binaryLeft, right, Language.Syntax.ComparisonKind(binary));
                    }
                },
                Language.SyntaxKind.ComparisonKinds);

        protected void CheckExpression(SonarSyntaxNodeReportingContext context, SyntaxNode issue, SyntaxNode expression, int constant, ComparisonKind comparison)
        {
            expression = Language.Syntax.RemoveConditionalAccess(expression);
            var result = comparison.Compare(constant);
            if (result.IsInvalid()
                && HasCandidateName(Language.Syntax.NodeIdentifier(expression)?.ValueText)
                && context.SemanticModel.GetSymbolInfo(expression).Symbol is ISymbol symbol
                && CollecionSizeTypeName(symbol) is { } symbolType)
            {
                context.ReportIssue(Rule, issue, symbol.Name, symbolType, (result == CountComparisonResult.AlwaysTrue).ToString());
            }
        }

        private bool HasCandidateName(string name) =>
            CountName.Equals(name, Language.NameComparison)
            || LengthName.Equals(name, Language.NameComparison)
            || LongLengthName.Equals(name, Language.NameComparison);

        private bool IsEnumerableCountMethod(ISymbol symbol) =>
            CountName.Equals(symbol.Name, Language.NameComparison)
            && symbol is IMethodSymbol methodSymbol
            && methodSymbol.IsExtensionMethod
            && methodSymbol.ReceiverType != null
            && methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);

        private bool IsArrayLengthProperty(ISymbol symbol) =>
            (LengthName.Equals(symbol.Name, Language.NameComparison) || LongLengthName.Equals(symbol.Name, Language.NameComparison))
            && symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.Is(KnownType.System_Array);

        private bool IsStringLengthProperty(ISymbol symbol) =>
            LengthName.Equals(symbol.Name, Language.NameComparison)
            && symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.Is(KnownType.System_String);

        private bool IsCollectionCountProperty(ISymbol symbol) =>
            CountName.Equals(symbol.Name, Language.NameComparison)
            && symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.DerivesOrImplements(KnownType.System_Collections_Generic_ICollection_T);

        private bool IsReadonlyCollectionCountProperty(ISymbol symbol) =>
            CountName.Equals(symbol.Name, Language.NameComparison)
            && symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.DerivesOrImplements(KnownType.System_Collections_Generic_IReadOnlyCollection_T);

        private string CollecionSizeTypeName(ISymbol symbol)
        {
            if (IsArrayLengthProperty(symbol))
            {
                return nameof(Array);
            }
            else if (IsStringLengthProperty(symbol))
            {
                return nameof(String);
            }
            else if (IsEnumerableCountMethod(symbol))
            {
                return IEnumerableTString;
            }
            else if (IsCollectionCountProperty(symbol))
            {
                return nameof(ICollection<object>);
            }
            else if (IsReadonlyCollectionCountProperty(symbol))
            {
                return nameof(IReadOnlyCollection<object>);
            }
            else
            {
                return null;
            }
        }
    }
}
