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
    public sealed class RedundantToArrayCall : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3456";
        private const string MessageFormat = "Remove this redundant '{0}' call.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var memberAccess = GetRedundantMemberAccess(c, "ToCharArray", KnownType.System_String)
                        ?? GetRedundantMemberAccess(c, "ToArray", KnownType.System_ReadOnlySpan_T);

                    if (memberAccess is not null)
                    {
                        c.ReportIssue(Rule, memberAccess.Name, memberAccess.Name.Identifier.ValueText);
                    }
                },
                SyntaxKind.InvocationExpression);

        private static MemberAccessExpressionSyntax GetRedundantMemberAccess(SonarSyntaxNodeReportingContext context, string targetMethodName, KnownType targetKnownType)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if ((invocation.Parent is ElementAccessExpressionSyntax || invocation.Parent is ForEachStatementSyntax)
                && invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && IsTargetMethod(memberAccess))
            {
                return memberAccess;
            }

            return null;

            bool IsTargetMethod(MemberAccessExpressionSyntax memberAccess) =>
                memberAccess.Name.Identifier.ValueText == targetMethodName
                && context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.IsInType(targetKnownType)
                && methodSymbol.Parameters.Length == 0;
        }
    }
}
