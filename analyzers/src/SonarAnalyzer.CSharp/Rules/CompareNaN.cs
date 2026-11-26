/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public sealed class CompareNaN : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2688";
        private const string MessageFormat = "{0}";
        private const string MessageFormatEquality = "Use {0}.IsNaN() instead.";
        private const string MessageFormatComparison = "Do not compare a number with {0}.NaN.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var binaryExpressionSyntax = (BinaryExpressionSyntax)c.Node;

                    if (TryGetFloatingPointType(binaryExpressionSyntax.Left, c.Model, out var floatingPointType)
                        || TryGetFloatingPointType(binaryExpressionSyntax.Right, c.Model, out floatingPointType))
                    {
                        var messageFormat = c.Node.Kind() is SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression
                            ? MessageFormatEquality
                            : MessageFormatComparison;

                        c.ReportIssue(
                            Rule,
                            binaryExpressionSyntax,
                            string.Format(messageFormat, TypeName(floatingPointType)));
                    }
                },
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

        private static bool TryGetFloatingPointType(SyntaxNode expression, SemanticModel model, out ITypeSymbol floatingPointType)
        {
            floatingPointType = null;

            if (expression is not MemberAccessExpressionSyntax memberAccess)
            {
                return false;
            }

            var symbol = model.GetSymbolInfo(memberAccess).Symbol;
            var type = symbol switch
            {
                IFieldSymbol { Name: nameof(double.NaN) } field => field.Type,
                IPropertySymbol { Name: nameof(double.NaN), IsStatic: true } property => property.Type,
                _ => null,
            };
            if (type is null)
            {
                return false;
            }

            floatingPointType = type;
            return IsFloatingPoint(type);
        }

        // The list is needed on TFMs without generic math (double/float on net48 and netstandard, Half on .NET 5, NFloat on .NET 6), the interface only exists from .NET 7.
        private static bool IsFloatingPoint(ITypeSymbol type) =>
            type.IsAny(KnownType.FloatingPointNumbers)
            || type.DerivesOrImplements(KnownType.System_Numerics_IFloatingPointIeee754_TSelf)
            || (type is ITypeParameterSymbol typeParameter && typeParameter.ConstraintTypes.Any(IsFloatingPoint));

        private static string TypeName(ITypeSymbol type) =>
            type.SpecialType switch
            {
                SpecialType.System_Double => "double",
                SpecialType.System_Single => "float",
                _ => type.Name,
            };
    }
}
