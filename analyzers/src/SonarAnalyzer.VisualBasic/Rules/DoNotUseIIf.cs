/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.VisualBasic.Rules
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
                var invokedMethod = c.Model.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

                if (IsIIf(invokedMethod))
                {
                    c.ReportIssue(Rule, invocationExpression);
                }
            },
            SyntaxKind.InvocationExpression);

        private static bool IsIIf(IMethodSymbol method) =>
            method != null
            && method.Name == IIfOperatorName
            && method.ContainingType.Is(KnownType.Microsoft_VisualBasic_Interaction);
    }
}
