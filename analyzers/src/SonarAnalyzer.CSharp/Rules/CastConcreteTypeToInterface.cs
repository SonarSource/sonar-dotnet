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
    public sealed class CastConcreteTypeToInterface : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3215";
        private const string MessageFormat = "Remove this cast and edit the interface to add the missing functionality.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var castExpression = (CastExpressionSyntax)c.Node;
                    CheckForIssue(c, castExpression.Expression, castExpression.Type);
                },
                SyntaxKind.CastExpression);

            context.RegisterNodeAction(
                c =>
                {
                    var castExpression = (BinaryExpressionSyntax)c.Node;
                    CheckForIssue(c, castExpression.Left, castExpression.Right);
                },
                SyntaxKind.AsExpression);
        }

        private static void CheckForIssue(SonarSyntaxNodeReportingContext context, SyntaxNode fromExpression, SyntaxNode toExpression)
        {
            var castedFrom = context.SemanticModel.GetTypeInfo(fromExpression).Type;
            var castedTo = context.SemanticModel.GetTypeInfo(toExpression).Type;
            if (castedFrom.Is(TypeKind.Interface)
                && castedFrom.DeclaringSyntaxReferences.Any()
                && castedTo.Is(TypeKind.Class)
                && !castedTo.Is(KnownType.System_Object))
            {
                context.ReportIssue(Rule, context.Node);
            }
        }
    }
}
