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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Metrics.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CognitiveComplexity : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3776";
        private const string MessageFormat = "Refactor this {0} to reduce its Cognitive Complexity from {1} to the {2} allowed.";
        private const int DefaultThreshold = 15;
        private const int DefaultPropertyThreshold = 3;

        [RuleParameter("threshold", PropertyType.Integer, "The maximum authorized complexity.", DefaultThreshold)]
        public int Threshold { get; set; } = DefaultThreshold;

        [RuleParameter("propertyThreshold ", PropertyType.Integer, "The maximum authorized complexity in a property.", DefaultPropertyThreshold)]
        public int PropertyThreshold { get; set; } = DefaultPropertyThreshold;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<MethodDeclarationSyntax>(c, m => m, m => m.Identifier.GetLocation(),
                    "method", Threshold),
                SyntaxKind.MethodDeclaration);

            // Here, we only care about arrowed properties, others will be handled by the accessor.
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<PropertyDeclarationSyntax>(c, p => p.ExpressionBody, p => p.Identifier.GetLocation(),
                    "property", PropertyThreshold),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<ConstructorDeclarationSyntax>(c, co => co, co => co.Identifier.GetLocation(),
                    "constructor", Threshold),
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<DestructorDeclarationSyntax>(c, d => d, d => d.Identifier.GetLocation(),
                    "destructor", Threshold),
                SyntaxKind.DestructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<OperatorDeclarationSyntax>(c, o => o, o => o.OperatorToken.GetLocation(),
                    "operator", Threshold),
                SyntaxKind.OperatorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckComplexity<AccessorDeclarationSyntax>(c, a => a, a => a.Keyword.GetLocation(),
                    "accessor", PropertyThreshold),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
               c => CheckComplexity<FieldDeclarationSyntax>(c, m => m, m => m.Declaration.Variables[0].Identifier.GetLocation(),
                    "field", Threshold),
               SyntaxKind.FieldDeclaration);
        }

        protected void CheckComplexity<TSyntax>(SyntaxNodeAnalysisContext context, Func<TSyntax, SyntaxNode> nodeSelector,
            Func<TSyntax, Location> getLocationToReport, string declarationType, int threshold)
            where TSyntax : SyntaxNode
        {
            var syntax = (TSyntax)context.Node;
            var nodeToAnalyze = nodeSelector(syntax);
            if (nodeToAnalyze == null)
            {
                return;
            }

            var cognitiveWalker = new CognitiveComplexityWalker();
            cognitiveWalker.Walk(nodeToAnalyze);
            cognitiveWalker.EnsureVisitEndedCorrectly();

            if (cognitiveWalker.Complexity > Threshold)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, getLocationToReport(syntax),
                    cognitiveWalker.IncrementLocations.ToAdditionalLocations(),
                    cognitiveWalker.IncrementLocations.ToProperties(),
                    new object[] { declarationType, cognitiveWalker.Complexity, threshold }));
            }
        }
    }
}
