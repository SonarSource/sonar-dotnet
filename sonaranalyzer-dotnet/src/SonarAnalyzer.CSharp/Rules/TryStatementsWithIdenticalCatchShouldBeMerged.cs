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
    public sealed class TryStatementsWithIdenticalCatchShouldBeMerged : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2327";
        private const string MessageFormat = "Combine this 'try' with the one starting on line {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var tryStatement = (TryStatementSyntax)c.Node;

                var mergeableTry = GetPreviousStatements(tryStatement)
                    .OfType<TryStatementSyntax>()
                    .FirstOrDefault(t => SameCatches(t.Catches) && SameFinally(t));

                if (mergeableTry != null)
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, tryStatement.GetLocation(), messageArgs: mergeableTry.GetLineNumberToReport()));
                }

                bool SameCatches(IReadOnlyCollection<CatchClauseSyntax> other) =>
                    tryStatement.Catches.Count == other.Count &&
                    tryStatement.Catches.All(x => other.Any(o => o.IsEquivalentTo(x)));

                bool SameFinally(TryStatementSyntax other) =>
                    tryStatement.Finally == null && other.Finally == null ||
                    tryStatement.Finally != null && other.Finally != null && tryStatement.Finally.IsEquivalentTo(other.Finally);

            },
            SyntaxKind.TryStatement);
        }

        private static IEnumerable<StatementSyntax> GetPreviousStatements(SyntaxNode expression)
        {
            var statement = expression.FirstAncestorOrSelf<StatementSyntax>();
            return statement == null
                ? Enumerable.Empty<StatementSyntax>()
                : statement.Parent.ChildNodes().OfType<StatementSyntax>().TakeWhile(x => x != statement).Reverse();
        }
    }
}
