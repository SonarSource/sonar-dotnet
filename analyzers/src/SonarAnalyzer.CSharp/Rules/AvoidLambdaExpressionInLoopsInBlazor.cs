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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLambdaExpressionInLoopsInBlazor : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6802";
    private const string MessageFormat = "Avoid using lambda expressions in loops in Blazor markup.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<string> AddAttributeMethods = new HashSet<string> { "AddAttribute", "AddMultipleAttributes" };

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

                    if (IsWithinLoopBody(node)
                        && IsWithinRenderTreeBuilderInvocation(node, c.Model))
                    {
                        c.ReportIssue(Rule, node);
                    }
                },
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        });

    private static bool IsWithinRenderTreeBuilderInvocation(SyntaxNode node, SemanticModel semanticModel) =>
        node.AncestorsAndSelf().Any(x => x is InvocationExpressionSyntax invocation
                                         && AddAttributeMethods.Contains(invocation.GetName())
                                         && semanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol symbol
                                         && symbol.ContainingType.GetSymbolType().Is(KnownType.Microsoft_AspNetCore_Components_Rendering_RenderTreeBuilder));

    private static bool IsWithinLoopBody(SyntaxNode node) =>
        node.AncestorsAndSelf().Any(x => x is BlockSyntax && x.Parent is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax);
}
