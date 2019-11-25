/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class IfConditionalAlwaysTrueOrFalseCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove useless 'if' statement";
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(IfConditionalAlwaysTrueOrFalse.DiagnosticId);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return DocumentBasedFixAllProvider.Instance;
        }

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var ifStatement = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<IfStatementSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = CalculateNewRoot(root, ifStatement);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

            return TaskHelper.CompletedTask;
        }

        private static SyntaxNode CalculateNewRoot(SyntaxNode root, IfStatementSyntax ifStatement)
        {
            SyntaxNode newRoot;
            var isTrue = ifStatement.Condition.IsKind(SyntaxKind.TrueLiteralExpression);

            if (isTrue)
            {
                var block = ifStatement.Statement as BlockSyntax;
                newRoot = block == null
                    ? root.ReplaceNode(ifStatement, ifStatement.Statement.WithAdditionalAnnotations(Formatter.Annotation))
                    : root.ReplaceNode(ifStatement, block.Statements.Select(st => st.WithAdditionalAnnotations(Formatter.Annotation)));
            }
            else
            {
                if (ifStatement.Else == null)
                {
                    newRoot = root.RemoveNode(ifStatement, SyntaxRemoveOptions.KeepNoTrivia);
                }
                else
                {
                    var block = ifStatement.Else.Statement as BlockSyntax;
                    newRoot = block == null
                        ? root.ReplaceNode(ifStatement, ifStatement.Else.Statement.WithAdditionalAnnotations(Formatter.Annotation))
                        : root.ReplaceNode(ifStatement, block.Statements.Select(st => st.WithAdditionalAnnotations(Formatter.Annotation)));
                }
            }

            return newRoot;
        }
    }
}

