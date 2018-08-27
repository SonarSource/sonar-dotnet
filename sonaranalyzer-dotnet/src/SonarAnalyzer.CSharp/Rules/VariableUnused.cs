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
    public sealed class VariableUnused : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1481";
        private const string MessageFormat = "Remove this unused '{0}' local variable.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(cbc =>
            {
                var declaredLocals = new HashSet<ISymbol>();
                var usedLocals = new HashSet<ISymbol>();

                cbc.RegisterSyntaxNodeAction(c =>
                {
                    declaredLocals.UnionWith(
                        ((LocalDeclarationStatementSyntax)c.Node).Declaration.Variables
                            .Select(variable => c.SemanticModel.GetDeclaredSymbol(variable))
                            .WhereNotNull());
                },
                SyntaxKind.LocalDeclarationStatement);

                cbc.RegisterSyntaxNodeAction(c =>
                {
                    usedLocals.UnionWith(GetUsedSymbols(c.Node, c.SemanticModel));
                },
                SyntaxKind.IdentifierName);

                cbc.RegisterCodeBlockEndAction(c =>
                {
                    declaredLocals.ExceptWith(usedLocals);
                    foreach (var unused in declaredLocals)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, unused.Locations.First(), unused.Name));
                    }
                });
            });
        }

        internal static IEnumerable<ISymbol> GetUsedSymbols(SyntaxNode node, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol != null)
            {
                yield return symbolInfo.Symbol;
            }

            foreach (var candidate in symbolInfo.CandidateSymbols.WhereNotNull())
            {
                yield return candidate;
            }
        }
    }
}
