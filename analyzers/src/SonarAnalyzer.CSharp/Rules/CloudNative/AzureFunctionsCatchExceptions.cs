/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AzureFunctionsCatchExceptions : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6421";
        private const string MessageFormat = "Wrap Azure Function body in try/catch block.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    if (c.AzureFunctionMethod() is not null)
                    {
                        var method = (MethodDeclarationSyntax)c.Node;
                        var walker = new Walker();
                        if (walker.SafeVisit(c.Node) && walker.HasInvocationOutsideTryCatch)
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, method.Identifier.GetLocation()));
                        }
                    }
                },
                SyntaxKind.MethodDeclaration);

        private sealed class Walker : SafeCSharpSyntaxWalker
        {
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

            public override void VisitInvocationExpression(InvocationExpressionSyntax node) =>
                HasInvocationOutsideTryCatch = true;

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
