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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UseTrueForAll : UseMethodAInsteadOfMethodB<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = c.Node as InvocationExpressionSyntax;

                if (invocation.NameIs(GetMethodName)
                    && invocation.TryGetOperands(out var left, out var right)
                    && IsCorrectCall(right, c.SemanticModel)
                    && IsCorrectType(left, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            SyntaxKind.InvocationExpression);

    private bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type
        && TypeCondition(type);

    private bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && MethodCondition(method);

    protected string GetMethodName => "ToString";
    protected bool MethodCondition(IMethodSymbol method) => method.Is(KnownType.System_Text_StringBuilder, "ToString");
    protected bool TypeCondition(ITypeSymbol type) => type.DerivesFrom(KnownType.System_Text_StringBuilder);
}
