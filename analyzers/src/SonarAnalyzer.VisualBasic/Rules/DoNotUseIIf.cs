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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class DoNotUseIIf : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3866";
        private const string MessageFormat = "Use the 'If' operator here instead of 'IIf'.";

        private const string IIfOperatorName = "IIf";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var invocationExpression = (InvocationExpressionSyntax)c.Node;
                var invokedMethod = c.SemanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

                if (IsIIf(invokedMethod))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, invocationExpression.GetLocation()));
                }
            },
            SyntaxKind.InvocationExpression);

        private static bool IsIIf(IMethodSymbol method) =>
            method != null
            && method.Name == IIfOperatorName
            && method.ContainingType.Is(KnownType.Microsoft_VisualBasic_Interaction);
    }
}
