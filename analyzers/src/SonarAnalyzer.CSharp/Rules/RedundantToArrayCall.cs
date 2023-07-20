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
                        c.ReportIssue(CreateDiagnostic(Rule, memberAccess.Name.GetLocation(), memberAccess.Name.Identifier.ValueText));
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
                && context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.IsInType(targetKnownType)
                && methodSymbol.Parameters.Length == 0;
        }
    }
}
