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
    public sealed class CompareNaN : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2688";
        private const string MessageFormat = "{0}";
        private const string MessageFormatEquality = "Use {0}.IsNaN() instead.";
        private const string MessageFormatComparison = "Do not compare a number with {0}.NaN.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly Dictionary<KnownType, string> KnownTypeAliasMap = new()
        {
            { KnownType.System_Double, "double" },
            { KnownType.System_Single, "float" },
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var binaryExpressionSyntax = (BinaryExpressionSyntax)c.Node;

                    if (TryGetFloatingPointType(binaryExpressionSyntax.Left, c.SemanticModel, out var floatingPointType)
                        || TryGetFloatingPointType(binaryExpressionSyntax.Right, c.SemanticModel, out floatingPointType))
                    {
                        var messageFormat = c.Node.IsAnyKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression)
                            ? MessageFormatEquality
                            : MessageFormatComparison;

                        c.ReportIssue(
                            CreateDiagnostic(Rule,
                            binaryExpressionSyntax.GetLocation(),
                            string.Format(messageFormat, KnownTypeAliasMap[floatingPointType])));
                    }
                },
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

        private static bool TryGetFloatingPointType(SyntaxNode expression, SemanticModel semanticModel, out KnownType floatingPointType)
        {
            floatingPointType = null;

            if (expression is not MemberAccessExpressionSyntax memberAccess)
            {
                return false;
            }

            var fieldSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol as IFieldSymbol;
            if (fieldSymbol?.Name != nameof(double.NaN))
            {
                return false;
            }

            floatingPointType = KnownType.FloatingPointNumbers.FirstOrDefault(fieldSymbol.Type.Is);

            return floatingPointType != null;
        }
    }
}
