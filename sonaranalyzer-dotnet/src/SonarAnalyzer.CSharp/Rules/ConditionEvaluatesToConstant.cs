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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.FlowAnalysis.Common;
using SonarAnalyzer.Helpers.FlowAnalysis.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    using ExplodedGraph = Helpers.FlowAnalysis.CSharp.ExplodedGraph;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ConditionEvaluatesToConstant : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2583";
        private const string MessageFormat = "Change this condition so that it does not always evaluate to '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis((e, c) => CheckForRedundantConditions(e, c));
        }

        private static void CheckForRedundantConditions(ExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var conditionTrue = new HashSet<SyntaxNode>();
            var conditionFalse = new HashSet<SyntaxNode>();

            EventHandler<ConditionEvaluatedEventArgs> collectConditions =
                (sender, args) => CollectConditions(args, conditionTrue, conditionFalse);

            EventHandler explorationEnded =
                (sender, args) => ProcessVisitedBlocks(conditionTrue, conditionFalse, context);

            explodedGraph.ExplorationEnded += explorationEnded;
            explodedGraph.ConditionEvaluated += collectConditions;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                explodedGraph.ExplorationEnded -= explorationEnded;
                explodedGraph.ConditionEvaluated -= collectConditions;
            }
        }

        private static void ProcessVisitedBlocks(HashSet<SyntaxNode> conditionTrue, HashSet<SyntaxNode> conditionFalse, SyntaxNodeAnalysisContext context)
        {
            foreach (var alwaysTrue in conditionTrue.Except(conditionFalse))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, alwaysTrue.GetLocation(), "true"));
            }

            foreach (var alwaysFalse in conditionFalse.Except(conditionTrue))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, alwaysFalse.GetLocation(), "false"));
            }
        }

        private static void CollectConditions(ConditionEvaluatedEventArgs args, HashSet<SyntaxNode> conditionTrue, HashSet<SyntaxNode> conditionFalse)
        {
            if (args.Condition == null ||
                OmittedSyntaxKinds.Contains(args.Condition.Kind()))
            {
                return;
            }

            if (args.EvaluationValue)
            {
                conditionTrue.Add(args.Condition);
            }
            else
            {
                conditionFalse.Add(args.Condition);
            }
        }

        private static readonly ISet<SyntaxKind> OmittedSyntaxKinds = ImmutableHashSet.Create(
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.TrueLiteralExpression,
            SyntaxKind.FalseLiteralExpression);
    }
}
