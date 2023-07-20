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
    public sealed class RedundantNullableTypeComparison : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3610";
        private const string MessageFormat = "Remove this redundant type comparison.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    CheckGetTypeAndTypeOfEquality(c, binary.Left, binary.Right, binary.GetLocation());
                    CheckGetTypeAndTypeOfEquality(c, binary.Right, binary.Left, binary.GetLocation());
                },
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }

        private static void CheckGetTypeAndTypeOfEquality(SonarSyntaxNodeReportingContext context, ExpressionSyntax sideA, ExpressionSyntax sideB, Location location)
        {
            if (!(sideA as InvocationExpressionSyntax).IsGetTypeCall(context.SemanticModel))
            {
                return;
            }

            var typeSyntax = (sideB as TypeOfExpressionSyntax)?.Type;
            if (typeSyntax == null)
            {
                return;
            }

            var typeSymbol = context.SemanticModel.GetTypeInfo(typeSyntax).Type;
            if (typeSymbol != null &&
                typeSymbol.OriginalDefinition.Is(KnownType.System_Nullable_T))
            {
                context.ReportIssue(CreateDiagnostic(rule, location));
            }
        }
    }
}
