/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnconditionalJumpStatement : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1751";
        private const string MessageFormat = "Remove this '{0}' statement or make it conditional.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> methodOrPropertyDeclarations = new HashSet<SyntaxKind>
        {
            SyntaxKind.MethodDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.SimpleLambdaExpression
        };

        private static readonly ISet<SyntaxKind> conditionalStatements = new HashSet<SyntaxKind>
        {
            SyntaxKind.IfStatement,
            SyntaxKind.SwitchStatement,
            SyntaxKind.CatchClause
        };

        private static readonly ISet<SyntaxKind> loopStatements = new HashSet<SyntaxKind>
        {
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var jumpStatement = (StatementSyntax)c.Node;

                var conditionalOrLoopStatement = jumpStatement
                    .Ancestors()
                    .TakeWhile(IsWithinMethodPropertyOrLambda)
                    .FirstOrDefault(IsConditionalOrLoopStatement);

                if (IsLoopStatement(conditionalOrLoopStatement))
                {
                    c.ReportDiagnostic(Diagnostic.Create(rule, jumpStatement.GetLocation(), GetKeywordText(jumpStatement)));
                }
            },
            SyntaxKind.BreakStatement,
            SyntaxKind.ContinueStatement,
            SyntaxKind.ReturnStatement,
            SyntaxKind.ThrowStatement);
        }

        private static string GetKeywordText(StatementSyntax statement) =>
            (statement as BreakStatementSyntax)?.BreakKeyword.ToString() ??
            (statement as ContinueStatementSyntax)?.ContinueKeyword.ToString() ??
            (statement as ReturnStatementSyntax)?.ReturnKeyword.ToString() ??
            (statement as ThrowStatementSyntax)?.ThrowKeyword.ToString();

        private static bool IsWithinMethodPropertyOrLambda(SyntaxNode node) =>
            !node.IsAnyKind(methodOrPropertyDeclarations);

        private static bool IsConditionalOrLoopStatement(SyntaxNode node) =>
            node.IsAnyKind(conditionalStatements) || IsLoopStatement(node);

        private static bool IsLoopStatement(SyntaxNode node) =>
            node.IsAnyKind(loopStatements);
    }
}
