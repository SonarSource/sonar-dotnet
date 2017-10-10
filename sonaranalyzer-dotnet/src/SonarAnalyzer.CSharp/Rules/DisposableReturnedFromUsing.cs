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
    public class DisposableReturnedFromUsing : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2997";
        private const string MessageFormat = "Remove the 'using' statement; it will cause automatic disposal of {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var usingStatement = (UsingStatementSyntax) c.Node;
                    var declaration = usingStatement.Declaration;
                    var declaredSymbols = ImmutableHashSet<ISymbol>.Empty;
                    if (declaration != null)
                    {
                        declaredSymbols =
                            declaration.Variables.Select(syntax => c.SemanticModel.GetDeclaredSymbol(syntax))
                                .Where(symbol => symbol != null)
                                .ToImmutableHashSet();
                    }
                    else
                    {
                        var assignment = usingStatement.Expression as AssignmentExpressionSyntax;
                        if (assignment != null)
                        {
                            var identifierName = assignment.Left as IdentifierNameSyntax;
                            if (identifierName == null)
                            {
                                return;
                            }
                            var symbol = c.SemanticModel.GetSymbolInfo(identifierName).Symbol;
                            if (symbol == null)
                            {
                                return;
                            }
                            declaredSymbols = (new[] { symbol }).ToImmutableHashSet();
                        }
                    }

                    if (declaredSymbols.IsEmpty)
                    {
                        return;
                    }

                    var returnedSymbols = GetReturnedSymbols(usingStatement.Statement, c.SemanticModel);
                    returnedSymbols = returnedSymbols.Intersect(declaredSymbols);

                    if (returnedSymbols.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, usingStatement.UsingKeyword.GetLocation(),
                            string.Join(", ",
                                returnedSymbols.Select(s => $"'{s.Name}'").OrderBy(s => s))));
                    }
                },
                SyntaxKind.UsingStatement);
        }

        private static ImmutableHashSet<ISymbol> GetReturnedSymbols(StatementSyntax usingStatement,
            SemanticModel semanticModel)
        {
            var enclosingSymbol = semanticModel.GetEnclosingSymbol(usingStatement.SpanStart);

            return usingStatement.DescendantNodesAndSelf()
                .OfType<ReturnStatementSyntax>()
                .Where(ret => semanticModel.GetEnclosingSymbol(ret.SpanStart).Equals(enclosingSymbol))
                .Select(ret => ret.Expression)
                .OfType<IdentifierNameSyntax>()
                .Select(identifier => semanticModel.GetSymbolInfo(identifier).Symbol)
                .Where(symbol => symbol != null)
                .ToImmutableHashSet();
        }
    }
}
