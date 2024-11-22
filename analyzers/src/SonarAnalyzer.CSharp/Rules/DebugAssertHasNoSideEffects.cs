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

using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DebugAssertHasNoSideEffects : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3346";
        private const string MessageFormat = "Expressions used in 'Debug.Assert' should not produce side effects.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> sideEffectWords = new HashSet<string>
        {
            "REMOVE", "DELETE", "PUT", "SET", "ADD", "POP", "UPDATE", "RETAIN",
            "INSERT", "PUSH", "APPEND", "CLEAR", "DEQUEUE", "ENQUEUE", "DISPOSE"
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var invokedMethodSyntax = c.Node as InvocationExpressionSyntax;

                    if (IsDebugAssert(c, invokedMethodSyntax)
                        && ContainsCallsWithSideEffects(invokedMethodSyntax))
                    {
                        c.ReportIssue(rule, invokedMethodSyntax.ArgumentList);
                    }
                },
                SyntaxKind.InvocationExpression);

        private static string GetIdentifierName(InvocationExpressionSyntax invocation)
        {

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name?.Identifier.ValueText;
            }

            var memberBinding = invocation.Expression as MemberBindingExpressionSyntax;
            return memberBinding?.Name?.Identifier.ValueText;
        }

        private static bool IsDebugAssert(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation) =>
            invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.ValueText == nameof(System.Diagnostics.Debug.Assert)
            && context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol symbol
            && symbol.IsDebugAssert();

        private static bool ContainsCallsWithSideEffects(InvocationExpressionSyntax invocation) =>
            invocation.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(GetIdentifierName)
                .Any(name => !string.IsNullOrEmpty(name)
                        && name != "SetEquals"
                        && sideEffectWords.Contains(name.SplitCamelCaseToWords().First()));
    }
}
