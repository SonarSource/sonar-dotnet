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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class LoopsAndLinq : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3267";
        private const string MessageFormat = "{0}";
        private const string WhereMessageFormat = @"Loops should be simplified with ""LINQ"" expressions";
        private const string SelectMessageFormat = "Loop should be simplified by calling Select({0} => {0}.{1}))";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var forEachStatementSyntax = (ForEachStatementSyntax)c.Node;
                    if (CanBeSimplifiedUsingWhere(forEachStatementSyntax.Statement))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, forEachStatementSyntax.Expression.GetLocation(), WhereMessageFormat));
                    }
                    else
                    {
                        CheckIfCanBeSimplifiedUsingSelect(forEachStatementSyntax, c);
                    }
                },
                SyntaxKind.ForEachStatement);

        private static bool CanBeSimplifiedUsingWhere(SyntaxNode statement) =>
            GetIfStatement(statement) is { } ifStatementSyntax
            && CanIfStatementBeMoved(ifStatementSyntax);

        private static IfStatementSyntax GetIfStatement(SyntaxNode node) =>
            node switch
            {
                IfStatementSyntax ifStatementSyntax => ifStatementSyntax,
                BlockSyntax blockSyntax when blockSyntax.ChildNodes().Count() == 1 => GetIfStatement(blockSyntax.ChildNodes().Single()),
                _ => null
            };

        private static bool CanIfStatementBeMoved(IfStatementSyntax ifStatementSyntax) =>
            ifStatementSyntax.Else == null
            && !ContainsExitPoints(ifStatementSyntax)
            && ifStatementSyntax.Condition is InvocationExpressionSyntax invocationExpressionSyntax
            && !invocationExpressionSyntax.DescendantNodes().OfType<ArgumentSyntax>().Any(argument => argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword));

        private static bool ContainsExitPoints(SyntaxNode syntaxNode) =>
            syntaxNode.DescendantNodes()
                      .Any(node => node.IsAnyKind(SyntaxKind.ReturnStatement,
                                                  SyntaxKind.YieldReturnStatement,
                                                  SyntaxKind.BreakStatement,
                                                  SyntaxKind.YieldBreakStatement,
                                                  SyntaxKind.GotoStatement));

        private static void CheckIfCanBeSimplifiedUsingSelect(ForEachStatementSyntax forEachStatementSyntax, SyntaxNodeAnalysisContext c)
        {
            // There are multiple scenarios where the code can be simplified using LINQ.
            // For simplicity, in the first version of the rule, we consider that Select() can be used
            // only when a single property from the foreach variable is used.
            // This property needs to be used more than once or if it is the right side of a variable declaration.
            var declaredSymbol = c.SemanticModel.GetDeclaredSymbol(forEachStatementSyntax);
            var accessedProperties = new Dictionary<ISymbol, UsageStats>();

            foreach (var identifierSyntax in GetIdentifiers(forEachStatementSyntax))
            {
                if (identifierSyntax.Parent is MemberAccessExpressionSyntax { Parent: not InvocationExpressionSyntax and not AssignmentExpressionSyntax } memberAccessExpressionSyntax
                    && c.SemanticModel.GetSymbolInfo(identifierSyntax).Symbol.Equals(declaredSymbol))
                {
                    var symbol = c.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name).Symbol;
                    var usageStats = accessedProperties.GetOrAdd(symbol, _ => new UsageStats());

                    if (identifierSyntax.Parent.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax })
                    {
                        usageStats.IsInVarDeclarator = true;
                    }

                    usageStats.Count++;
                }
                else
                {
                    return;
                }
            }

            if (accessedProperties.Count == 1
                && accessedProperties.First().Value is var stats
                && (stats.IsInVarDeclarator || stats.Count > 1))
            {
                var diagnostic = Diagnostic.Create(Rule,
                                                   forEachStatementSyntax.Expression.GetLocation(),
                                                   string.Format(SelectMessageFormat, forEachStatementSyntax.Identifier.ValueText, accessedProperties.Single().Key.Name));
                c.ReportDiagnosticWhenActive(diagnostic);
            }

            static IEnumerable<IdentifierNameSyntax> GetIdentifiers(ForEachStatementSyntax forEachStatementSyntax) =>
                forEachStatementSyntax.Statement
                                      .DescendantNodes()
                                      .OfType<IdentifierNameSyntax>()
                                      .Where(identifierNameSyntax => identifierNameSyntax.Identifier.ValueText == forEachStatementSyntax.Identifier.ValueText);
        }

        private class UsageStats
        {
            public int Count { get; set; }

            public bool IsInVarDeclarator { get; set; }
        }
    }
}
