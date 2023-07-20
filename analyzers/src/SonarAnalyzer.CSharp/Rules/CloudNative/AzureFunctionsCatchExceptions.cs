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
    public sealed class AzureFunctionsCatchExceptions : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6421";
        private const string MessageFormat = "Wrap Azure Function body in try/catch block.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.IsAzureFunction())
                    {
                        var method = (MethodDeclarationSyntax)c.Node;
                        var walker = new Walker(c.SemanticModel);
                        if (walker.SafeVisit(method.GetBodyOrExpressionBody()) && walker.HasInvocationOutsideTryCatch)
                        {
                            c.ReportIssue(CreateDiagnostic(Rule, method.Identifier.GetLocation()));
                        }
                    }
                },
                SyntaxKind.MethodDeclaration);

        private sealed class Walker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;

            public Walker(SemanticModel semanticModel) =>
                this.semanticModel = semanticModel;

            public bool HasInvocationOutsideTryCatch { get; private set; }

            public override void Visit(SyntaxNode node)
            {
                if (!HasInvocationOutsideTryCatch   // Stop walking when we know the answer
                    && !node.IsAnyKind(
                        SyntaxKind.CatchClause,     // Do not visit content of "catch". It doesn't make sense to wrap logging in catch in another try/catch.
                        SyntaxKind.AnonymousMethodExpression,
                        SyntaxKind.SimpleLambdaExpression,
                        SyntaxKind.ParenthesizedLambdaExpression,
                        SyntaxKindEx.LocalFunctionStatement))
                {
                    base.Visit(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (!node.IsNameof(semanticModel))
                {
                    HasInvocationOutsideTryCatch = true;
                }
            }

            public override void VisitTryStatement(TryStatementSyntax node)
            {
                if (!node.Catches.Any(CatchesAllExceptions))
                {
                    base.VisitTryStatement(node);
                }
            }

            private static bool CatchesAllExceptions(CatchClauseSyntax catchClause) =>
                catchClause.Declaration is null
                || (catchClause.Declaration.Type.NameIs(nameof(Exception)) && catchClause.Filter is null);
        }
    }
}
