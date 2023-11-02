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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLambdaExpressionInLoops : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6802";
    private const string MessageFormat = "Avoid using lambda expressions in loops.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            // If we are not in a Blazor project, we don't need to register for lambda expressions.
            if (cc.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Components_Rendering_RenderTreeBuilder) is null)
            {
                return;
            }

            cc.RegisterNodeAction(c =>
                {
                    var node = (LambdaExpressionSyntax)c.Node;

                    if (IsWithinLoop(node)
                        && IsWithinRenderTreeBuilderInvocation(node, c.SemanticModel))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, node.GetLocation()));
                    }
                },
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        });

    private static bool IsWithinRenderTreeBuilderInvocation(SyntaxNode node, SemanticModel semanticModel)
    {
        while (node != null)
        {
            if (node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } }
                && semanticModel.GetSymbolInfo(identifier) is { Symbol: { } symbol }
                && KnownType.Microsoft_AspNetCore_Components_Rendering_RenderTreeBuilder.Matches(symbol.GetSymbolType()))
            {
                return true;
            }

            node = node.Parent;
        }

        return false;
    }

    private static bool IsWithinLoop(SyntaxNode node)
    {
        while (node != null)
        {
            if (node is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax)
            {
                return true;
            }

            node = node.Parent;
        }

        return false;
    }
}
