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

using System.Collections.Immutable;
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
    public sealed class ValuesUselesslyIncremented : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2123";
        private const string MessageFormat = "Remove this {0} or correct the code not to waste it.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var increment = (PostfixUnaryExpressionSyntax)c.Node;
                    var symbol = c.SemanticModel.GetSymbolInfo(increment.Operand).Symbol;

                    if (!(symbol is ILocalSymbol || symbol is IParameterSymbol {RefKind: RefKind.None}))
                    {
                        return;
                    }

                    var operatorText = increment.OperatorToken.IsKind(SyntaxKind.PlusPlusToken)
                        ? "increment"
                        : "decrement";

                    switch (increment.Parent)
                    {
                        case ReturnStatementSyntax _:
                        case ArrowExpressionClauseSyntax _:
                        case AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                                                                        && assignment.Right == increment
                                                                        && CSharpEquivalenceChecker.AreEquivalent(assignment.Left, increment.Operand):

                            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, increment.GetLocation(), operatorText));
                            return;
                    }
                },
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PostDecrementExpression);
    }
}
