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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(NullPointerDereference.DiagnosticId)]
    [Rule(ObjectsShouldNotBeDisposedMoreThanOnce.DiagnosticId)]
    [Rule(EmptyNullableValueAccess.DiagnosticId)]
    [Rule(EmptyCollectionsShouldNotBeEnumerated.DiagnosticId)]
    [Rule(PublicMethodArgumentsShouldBeCheckedForNull.DiagnosticId)]
    [Rule(ConditionEvaluatesToConstant.S2583DiagnosticId)]
    [Rule(ConditionEvaluatesToConstant.S2589DiagnosticId)]
    [Rule(InvalidCastToInterface.DiagnosticId)]
    public class SymbolicExecutionAnalyzer : SonarDiagnosticAnalyzer
    {
        private readonly List<ISymbolicExecutionAnalyzerFactory> analyzerFactories;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            analyzerFactories.SelectMany(factory => factory.SupportedDiagnostics).ToImmutableArray();

        public SymbolicExecutionAnalyzer() : this(
            new ConditionEvaluatesToConstant(),
            new EmptyCollectionsShouldNotBeEnumerated(),
            new EmptyNullableValueAccess(),
            new InvalidCastToInterface(),
            new NullPointerDereference(),
            new ObjectsShouldNotBeDisposedMoreThanOnce(),
            new PublicMethodArgumentsShouldBeCheckedForNull())
        {
        }

        internal SymbolicExecutionAnalyzer(params ISymbolicExecutionAnalyzerFactory[] analyzers)
        {
            analyzerFactories = new List<ISymbolicExecutionAnalyzerFactory>(analyzers);
        }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis(Analyze);
        }

        private void Analyze(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var analyzerInstances = analyzerFactories
                .Where(factory => factory.IsEnabled(context))
                .Select(factory => factory.Create(explodedGraph))
                .ToList();

            explodedGraph.ExplorationEnded += OnExplorationEnded;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                explodedGraph.ExplorationEnded -= OnExplorationEnded;

                analyzerInstances.ForEach(analyzer => analyzer.Dispose());
            }

            void OnExplorationEnded(object sender, EventArgs e) =>
                analyzerInstances
                    .SelectMany(a => a.Diagnostics)
                    .ToList()
                    .ForEach(diagnostic => context.ReportDiagnosticWhenActive(diagnostic));
        }
    }
}
