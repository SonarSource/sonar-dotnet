/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TrackNotImplementedException : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3717";
        private const string MessageFormat = "Implement this method or throw 'NotSupportedException' instead.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var throwStatement = (ThrowStatementSyntax)c.Node;
                    if (throwStatement.Expression == null)
                    {
                        return;
                    }

                    ReportDiagnostic(c, throwStatement.Expression, throwStatement);
                },
                SyntaxKind.ThrowStatement);

            context.RegisterNodeAction(
                 c =>
                 {
                     var throwExpression = (ThrowExpressionSyntaxWrapper)c.Node;
                     ReportDiagnostic(c, throwExpression.Expression, throwExpression);
                 },
                 SyntaxKindEx.ThrowExpression);
        }

        private static void ReportDiagnostic(SonarSyntaxNodeReportingContext c, ExpressionSyntax newExceptionExpression, SyntaxNode throwExpression)
        {
            if (c.Model.GetTypeInfo(newExceptionExpression).Type.Is(KnownType.System_NotImplementedException))
            {
                c.ReportIssue(rule, throwExpression);
            }
        }
    }
}
