/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class SillyBitwiseOperationCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove bitwise operation";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SillyBitwiseOperationBase.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            DocumentBasedFixAllProvider.Instance;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var isReportingOnLeft = diagnostic.Properties.ContainsKey(SillyBitwiseOperation.IsReportingOnLeftKey) && bool.Parse(diagnostic.Properties[SillyBitwiseOperation.IsReportingOnLeftKey]);
            Func<SyntaxNode> createNewRoot = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) switch
            {
                StatementSyntax statement => () => root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia),
                AssignmentExpressionSyntax assignment => () => root.ReplaceNode(assignment, assignment.Left.WithAdditionalAnnotations(Formatter.Annotation)),
                BinaryExpressionSyntax binary => () => root.ReplaceNode(binary, (isReportingOnLeft ? binary.Right : binary.Left).WithAdditionalAnnotations(Formatter.Annotation)),
                _ => null
            };
            if (createNewRoot != null)
            {
                context.RegisterCodeFix(CodeAction.Create(Title, c => Task.FromResult(context.Document.WithSyntaxRoot(createNewRoot()))), context.Diagnostics);
            }
            return TaskHelper.CompletedTask;
        }
    }
}
