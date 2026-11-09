/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DebugAssertHasNoSideEffects : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3346";
    private const string MessageFormat = "Expressions used in 'Debug.Assert' should not produce side effects.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly HashSet<string> SideEffectWords =
    [
        "REMOVE", "DELETE", "PUT", "SET", "ADD", "POP", "UPDATE", "RETAIN",
        "INSERT", "PUSH", "APPEND", "CLEAR", "DEQUEUE", "ENQUEUE", "DISPOSE"
    ];

    private static readonly HashSet<string> SideEffectFreeNames = ["SetEquals", "PopCount"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invokedMethodSyntax = c.Node as InvocationExpressionSyntax;

                if (IsDebugAssert(c, invokedMethodSyntax)
                    && ContainsCallsWithSideEffects(invokedMethodSyntax))
                {
                    c.ReportIssue(Rule, invokedMethodSyntax.ArgumentList);
                }
            },
            SyntaxKind.InvocationExpression);

    private static string IdentifierName(InvocationExpressionSyntax invocation) =>
        invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
            _ => null
        };

    private static bool IsDebugAssert(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation) =>
        invocation.Expression is MemberAccessExpressionSyntax memberAccess
        && memberAccess.Name.Identifier.ValueText == nameof(Debug.Assert)
        && context.Model.GetSymbolInfo(invocation, context.Cancel).Symbol is IMethodSymbol symbol
        && symbol.IsDebugAssert();

    private static bool ContainsCallsWithSideEffects(InvocationExpressionSyntax invocation) =>
        invocation.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(IdentifierName)
            .Any(x => !string.IsNullOrEmpty(x)
                        && !SideEffectFreeNames.Contains(x)
                        && SideEffectWords.Contains(x.SplitCamelCaseToWords().First()));
}
