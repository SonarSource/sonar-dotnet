/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
public sealed class CallModelStateIsValid : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6967";
    private const string MessageFormat = "ModelState.IsValid should be checked in controller actions.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly SyntaxKind[] SyntaxNodesToVisit = [
        SyntaxKind.ConditionalAccessExpression,
        SyntaxKind.SimpleMemberAccessExpression,
        SyntaxKind.InvocationExpression,
        SyntaxKindEx.Subpattern];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            if (compilationStart.Compilation.ReferencesControllers()
                && !compilationStart.Compilation.Assembly.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
            {
                compilationStart.RegisterSymbolStartAction(symbolStart =>
                {
                    var type = (INamedTypeSymbol)symbolStart.Symbol;
                    if (type.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase)
                        && !type.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute)
                        && !HasActionFilterAttribute(type))
                    {
                        symbolStart.RegisterCodeBlockStartAction<SyntaxKind>(ProcessCodeBlock);
                    }
                }, SymbolKind.NamedType);
            }
        });

    private static void ProcessCodeBlock(SonarCodeBlockStartAnalysisContext<SyntaxKind> codeBlockContext)
    {
        if (codeBlockContext.CodeBlock is MethodDeclarationSyntax methodDeclaration
            && IsControllerAction(methodDeclaration, codeBlockContext.SemanticModel))
        {
            var isModelValidated = false;
            codeBlockContext.RegisterNodeAction(nodeContext =>
            {
                if (IsCheckingValidityProperty(nodeContext.Node, nodeContext.SemanticModel)
                    || IsTryValidateInvocation(nodeContext.Node, nodeContext.SemanticModel))
                {
                    isModelValidated = true;
                }
            }, SyntaxNodesToVisit);
            codeBlockContext.RegisterCodeBlockEndAction(blockEnd =>
            {
                if (!isModelValidated)
                {
                    blockEnd.ReportIssue(Rule, methodDeclaration.Identifier);
                }
            });
        }
    }

    private static bool IsControllerAction(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel) =>
        semanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol
        && methodSymbol.Parameters.Length > 0
        && methodSymbol.IsControllerMethod()
        && !HasActionFilterAttribute(methodSymbol)
        && methodSymbol.Parameters.Length > 0;

    private static bool HasActionFilterAttribute(ISymbol symbol) =>
        symbol.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Filters_ActionFilterAttribute));

    private static bool IsCheckingValidityProperty(SyntaxNode node, SemanticModel semanticModel)
    {
        var propertyNode = node switch
        {
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.WhenNotNull,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            { } when SubpatternSyntaxWrapper.IsInstance(node) => ((SubpatternSyntaxWrapper)node).ExpressionColon.Expression,
            _ => null
        };
        return propertyNode?.GetName() is "IsValid" or "ValidationState"
            && semanticModel.GetSymbolInfo(propertyNode).Symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.Is(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_ModelStateDictionary);
    }

    private static bool IsTryValidateInvocation(SyntaxNode node, SemanticModel semanticModel) =>
        node is InvocationExpressionSyntax invocation
        && invocation.GetName() == "TryValidateModel"
        && semanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol method
        && method.ContainingType.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase);
}
