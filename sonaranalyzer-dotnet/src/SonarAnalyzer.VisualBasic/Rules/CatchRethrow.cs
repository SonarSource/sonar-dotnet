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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class CatchRethrow : CatchRethrowBase<CatchBlockSyntax>
    {
        private static readonly SyntaxList<StatementSyntax> ThrowBlock = new SyntaxList<StatementSyntax>().Add(SyntaxFactory.ThrowStatement());

        protected override DiagnosticDescriptor Rule { get; } =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override bool ContainsOnlyThrow(CatchBlockSyntax currentCatch) =>
            VisualBasicEquivalenceChecker.AreEquivalent(currentCatch.Statements, ThrowBlock);

        protected override IReadOnlyList<CatchBlockSyntax> GetCatches(SyntaxNode syntaxNode) =>
            ((TryBlockSyntax)syntaxNode).CatchBlocks;

        protected override SyntaxNode GetDeclarationType(CatchBlockSyntax catchClause) =>
            catchClause.CatchStatement?.AsClause?.Type;

        protected override bool HasFilter(CatchBlockSyntax catchClause) =>
            catchClause.CatchStatement?.WhenClause != null;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnInvalidCatch, SyntaxKind.TryBlock);
    }
}
