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
                context.ReportIssue(rule, location);
            }
        }
    }
}
