/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Metrics.CSharp;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FunctionComplexity : ParameterLoadingDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1541";
        private const string MessageFormat = "The Cyclomatic Complexity of this {2} is {1} which is greater than {0} authorized.";
        private const int DefaultValueMaximum = 10;

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        [RuleParameter("maximumFunctionComplexityThreshold", PropertyType.Integer, "The maximum authorized complexity.", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    if (c.Node.IsTopLevelMain())
                    {
                        CheckComplexity<CompilationUnitSyntax>(c, m => Location.Create(c.Node.SyntaxTree, TextSpan.FromBounds(0, 0)), "top-level file", true);
                    }
                },
                SyntaxKind.CompilationUnit);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<MethodDeclarationSyntax>(c, m => m.Identifier.GetLocation(), "method"),
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<PropertyDeclarationSyntax>(c, p => p.Identifier.GetLocation(), p => p.ExpressionBody, "property"),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<OperatorDeclarationSyntax>(c, o => o.OperatorKeyword.GetLocation(), "operator"),
                SyntaxKind.OperatorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<ConstructorDeclarationSyntax>(c, co => co.Identifier.GetLocation(), "constructor"),
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<DestructorDeclarationSyntax>(c, d => d.Identifier.GetLocation(), "destructor"),
                SyntaxKind.DestructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (((LocalFunctionStatementSyntaxWrapper)c.Node).Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    CheckComplexity<SyntaxNode>(c, d => ((LocalFunctionStatementSyntaxWrapper)d).Identifier.GetLocation(), "static local function");
                }
            },
            SyntaxKindEx.LocalFunctionStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<AccessorDeclarationSyntax>(c, a => a.Keyword.GetLocation(), "accessor"),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration);
        }

        private void CheckComplexity<TSyntax>(SyntaxNodeAnalysisContext context, Func<TSyntax, Location> getLocation, string declarationType, bool onlyGlobalStatements = false)
            where TSyntax : SyntaxNode =>
            CheckComplexity(context, getLocation, n => n, declarationType, onlyGlobalStatements);

        private void CheckComplexity<TSyntax>(
            SyntaxNodeAnalysisContext context,
            Func<TSyntax, Location> getLocation,
            Func<TSyntax, SyntaxNode> getNodeToCheck,
            string declarationType,
            bool onlyGlobalStatements = false)
            where TSyntax : SyntaxNode
        {
            var node = (TSyntax)context.Node;

            var nodeToCheck = getNodeToCheck(node);
            if (nodeToCheck == null)
            {
                return;
            }

            var complexityMetric = CSharpCyclomaticComplexityMetric.GetComplexity(nodeToCheck, onlyGlobalStatements);
            if (complexityMetric.Complexity > Maximum)
            {
                context.ReportIssue(
                    Diagnostic.Create(Rule,
                                      getLocation(node),
                                      complexityMetric.Locations.ToAdditionalLocations(),
                                      complexityMetric.Locations.ToProperties(),
                                      Maximum,
                                      complexityMetric.Complexity,
                                      declarationType));
            }
        }
    }
}
