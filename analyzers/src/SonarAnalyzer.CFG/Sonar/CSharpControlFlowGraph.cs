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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.CFG.Sonar
{
    public static class CSharpControlFlowGraph
    {
        public static bool TryGet(SyntaxNode node, SemanticModel semanticModel, out IControlFlowGraph cfg)
        {
            cfg = null;
            var body = node switch
            {
                BaseMethodDeclarationSyntax n => (SyntaxNode)n.Body ?? n.ExpressionBody(),
                PropertyDeclarationSyntax n => n.ExpressionBody?.Expression,
                IndexerDeclarationSyntax n => n.ExpressionBody?.Expression,
                AccessorDeclarationSyntax n => (SyntaxNode)n.Body ?? n.ExpressionBody(),
                AnonymousFunctionExpressionSyntax n => n.Body,
                ArrowExpressionClauseSyntax n => n,
                _ when node.IsKind(SyntaxKindEx.LocalFunctionStatement) && (LocalFunctionStatementSyntaxWrapper)node is var local => (SyntaxNode)local.Body ?? local.ExpressionBody,
                _ => null
            };
            try
            {
                if (body is not null)
                {
                    cfg = Create(body, semanticModel);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc) when (exc is InvalidOperationException ||
                                        exc is ArgumentException ||
                                        exc is NotSupportedException)
            {
                // historically, these have been considered as expected
                // but we should be aware of what syntax we do not yet support
                // https://github.com/SonarSource/sonar-dotnet/issues/2541
            }
            catch (Exception exc) when (exc is NotImplementedException)
            {
                Debug.Fail(exc.ToString());
            }

            return cfg != null;
        }

        internal /* for testing */ static IControlFlowGraph Create(SyntaxNode node, SemanticModel semanticModel) =>
            new CSharpControlFlowGraphBuilder(node, semanticModel).Build();
    }
}
