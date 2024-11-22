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
    public sealed class DoNotCatchNullReferenceException : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1696";
        private const string MessageFormat = "Do not catch NullReferenceException; test for null instead.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;
                    if (IsCatchingNullReferenceException(catchClause.Declaration, c.SemanticModel))
                    {
                        c.ReportIssue(rule, catchClause.Declaration.Type);
                        return;
                    }

                    if (HasIsNullReferenceExceptionFilter(catchClause.Filter, c.SemanticModel, out var locationToReportOn))
                    {
                        c.ReportIssue(rule, locationToReportOn);
                    }
                },
                SyntaxKind.CatchClause);
        }

        private static bool IsCatchingNullReferenceException(CatchDeclarationSyntax catchDeclaration,
            SemanticModel semanticModel)
        {
            var caughtTypeSyntax = catchDeclaration?.Type;
            return caughtTypeSyntax != null &&
                   semanticModel.GetTypeInfo(caughtTypeSyntax).Type.Is(KnownType.System_NullReferenceException);
        }

        private static bool HasIsNullReferenceExceptionFilter(CatchFilterClauseSyntax catchFilterClause,
            SemanticModel semanticModel, out Location location)
        {
            var whenExpression = catchFilterClause?.FilterExpression.RemoveParentheses();

            var rightSideOfIsExpression = whenExpression != null && whenExpression.IsKind(SyntaxKind.IsExpression)
                ? ((BinaryExpressionSyntax)whenExpression).Right
                : null;

            if (rightSideOfIsExpression != null &&
                semanticModel.GetTypeInfo(rightSideOfIsExpression).Type.Is(KnownType.System_NullReferenceException))
            {
                location = rightSideOfIsExpression.GetLocation();
                return true;
            }

            location = null;
            return false;
        }
    }
}
