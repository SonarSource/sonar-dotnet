/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CatchRethrow : CatchRethrowBase<CatchClauseSyntax>
    {
        private static readonly BlockSyntax ThrowBlock = SyntaxFactory.Block(SyntaxFactory.ThrowStatement());

        protected override DiagnosticDescriptor Rule { get; } =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override bool ContainsOnlyThrow(CatchClauseSyntax currentCatch) =>
            EquivalenceChecker.AreEquivalent(currentCatch.Block, ThrowBlock);

        protected override IReadOnlyList<CatchClauseSyntax> GetCatches(SyntaxNode syntaxNode) =>
            ((TryStatementSyntax)syntaxNode).Catches;

        protected override SyntaxNode GetDeclarationType(CatchClauseSyntax catchClause) =>
            catchClause.Declaration?.Type;

        protected override bool HasFilter(CatchClauseSyntax catchClause) =>
            catchClause.Filter != null;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnInvalidCatch, SyntaxKind.TryStatement);
    }
}
