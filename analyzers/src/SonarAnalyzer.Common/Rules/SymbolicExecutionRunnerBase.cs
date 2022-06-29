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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Rules
{
    public abstract class SymbolicExecutionRunnerBase : SonarDiagnosticAnalyzer
    {
        protected abstract ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; }
        protected abstract ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node);
        protected abstract void AnalyzeSonar(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun, SyntaxNode body, ISymbol symbol);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => AllRules.Keys.ToImmutableArray();
        protected override bool EnableConcurrentExecution => false;

        protected static RuleFactory CreateFactory<TRuleCheck>() where TRuleCheck : SymbolicRuleCheck, new() =>
            new RuleFactory<TRuleCheck>();

        // We need to rewrite this https://github.com/SonarSource/sonar-dotnet/issues/4824
        protected static bool IsEnabled(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun, DiagnosticDescriptor descriptor) =>
            SonarAnalysisContext.IsAnalysisScopeMatching(context.Compilation, isTestProject, isScannerRun, new[] { descriptor })
            && descriptor.GetEffectiveSeverity(context.Compilation.Options) != ReportDiagnostic.Suppress;

        protected void Analyze<TNode>(SonarAnalysisContext analysisContext, SyntaxNodeAnalysisContext context, Func<TNode, SyntaxNode> getBody) where TNode : SyntaxNode
        {
            if (getBody((TNode)context.Node) is { } body && context.SemanticModel.GetDeclaredSymbol(context.Node) is { } symbol)
            {
                Analyze(analysisContext, context, body, symbol);
            }
        }

        protected void Analyze(SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext, SyntaxNode body, ISymbol symbol)
        {
            if (body is { ContainsDiagnostics: false })
            {
                var isTestProject = sonarContext.IsTestProject(nodeContext.Compilation, nodeContext.Options);
                var isScannerRun = sonarContext.IsScannerRun(nodeContext.Options);
                AnalyzeSonar(nodeContext, isTestProject, isScannerRun, body, symbol);
                if (ControlFlowGraph.IsAvailable)   // ToDo: Make this configurable for UTs when migrating other rules, see DeadStores
                {
                    AnalyzeRoslyn(sonarContext, nodeContext, isTestProject, isScannerRun, body, symbol);
                }
            }
        }

        private void AnalyzeRoslyn(SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext, bool isTestProject, bool isScannerRun, SyntaxNode body, ISymbol symbol)
        {
            var checks = AllRules
                .Where(x => IsEnabled(nodeContext, isTestProject, isScannerRun, x.Key))
                .GroupBy(x => x.Value.Type)                             // Multiple DiagnosticDescriptors (S2583, S2589) can share the same check type
                .Select(x => x.First().Value.CreateInstance(sonarContext, nodeContext))   // We need just one instance in that case
                .Where(x => x.ShouldExecute())
                .ToArray();
            if (checks.Any())
            {
                try
                {
                    if (CreateCfg(nodeContext.SemanticModel, body) is { } cfg)
                    {
                        var engine = new RoslynSymbolicExecution(cfg, checks);
                        engine.Execute(nodeContext.CancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    throw new SymbolicExecutionException(ex, symbol, body.GetLocation());
                }
            }
        }

        protected class RuleFactory
        {
            public Type Type { get; }
            private readonly Func<SymbolicRuleCheck> createInstance;

            protected RuleFactory(Type type, Func<SymbolicRuleCheck> createInstance)
            {
                Type = type;
                this.createInstance = createInstance;
            }

            public SymbolicRuleCheck CreateInstance(SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext)
            {
                var ret = createInstance();
                ret.Init(sonarContext, nodeContext);
                return ret;
            }
        }

        private sealed class RuleFactory<TCheck> : RuleFactory
            where TCheck : SymbolicRuleCheck, new()
        {
            public RuleFactory() : base(typeof(TCheck), () => new TCheck()) { }
        }
    }
}
