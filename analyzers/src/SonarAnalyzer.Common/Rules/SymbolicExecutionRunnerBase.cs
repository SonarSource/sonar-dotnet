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
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Rules
{
    public abstract class SymbolicExecutionRunnerBase : SonarDiagnosticAnalyzer
    {
        protected abstract ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; }
        protected abstract ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node, CancellationToken cancel);
        protected abstract void AnalyzeSonar(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun, SyntaxNode body, ISymbol symbol);

        protected IAnalyzerConfiguration Configuration { get; }

        private protected /* for testing */ SymbolicExecutionRunnerBase(IAnalyzerConfiguration configuration) =>
            Configuration = configuration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => AllRules.Keys.ToImmutableArray();
        protected override bool EnableConcurrentExecution => false;

        protected static RuleFactory CreateFactory<TRuleCheck>() where TRuleCheck : SymbolicRuleCheck, new() =>
            new RuleFactory<TRuleCheck>();

        protected static RuleFactory CreateFactory<TRuleCheck, TSonarFallback>()
            where TRuleCheck : SymbolicRuleCheck, new()
            where TSonarFallback : new() =>
            new RuleFactory<TRuleCheck, TSonarFallback>();

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
                if (ControlFlowGraph.IsAvailable)
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
                .Select(x => x.First().Value.CreateInstance(Configuration, sonarContext, nodeContext))   // We need just one instance in that case
                .Where(x => x?.ShouldExecute() is true)
                .ToArray();
            if (checks.Any())
            {
                try
                {
                    if (CreateCfg(nodeContext.SemanticModel, body, nodeContext.CancellationToken) is { } cfg)
                    {
                        var engine = new RoslynSymbolicExecution(cfg, checks, nodeContext.CancellationToken);
                        engine.Execute();
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
            private readonly Func<SymbolicRuleCheck> createInstance;
            private readonly Func<object> createSonarFallbackInstance;

            public Type Type { get; }

            protected RuleFactory(Type type, Func<SymbolicRuleCheck> createInstance, Func<object> createSonarFallbackInstance)
            {
                Type = type;
                this.createInstance = createInstance;
                this.createSonarFallbackInstance = createSonarFallbackInstance;
            }

            public SymbolicRuleCheck CreateInstance(IAnalyzerConfiguration configuration, SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext)
            {
                if (configuration.ForceSonarCfg && createSonarFallbackInstance is not null)
                {
                    return null;
                }

                var ret = createInstance();
                ret.Init(sonarContext, nodeContext);
                return ret;
            }

            public object CreateSonarFallback(IAnalyzerConfiguration configuration) =>
                configuration.ForceSonarCfg && createSonarFallbackInstance is not null ? createSonarFallbackInstance() : null;
        }

        protected class RuleFactory<TCheck> : RuleFactory
            where TCheck : SymbolicRuleCheck, new()
        {
            public RuleFactory() : this(null) { }
            protected RuleFactory(Func<object> sonarFallbackFactory) : base(typeof(TCheck), () => new TCheck(), sonarFallbackFactory) { }
        }

        protected class RuleFactory<TCheck, TSonarFallback> : RuleFactory<TCheck>
            where TCheck : SymbolicRuleCheck, new()
            where TSonarFallback : new()
        {
            public RuleFactory() : base(() => new TSonarFallback()) { }
        }
    }
}
