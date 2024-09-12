/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Enterprise.Core.Rules;

public abstract class SymbolicExecutionRunnerBase : SonarDiagnosticAnalyzer
{
    protected abstract ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; }
    protected abstract ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node, CancellationToken cancel);
    protected abstract void AnalyzeSonar(SonarSyntaxNodeReportingContext context, ISymbol symbol);
    protected abstract SyntaxClassifierBase SyntaxClassifier { get; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => AllRules.Keys.ToImmutableArray();

    protected IAnalyzerConfiguration Configuration { get; }
    protected override bool EnableConcurrentExecution => false;

    protected SymbolicExecutionRunnerBase(IAnalyzerConfiguration configuration) =>
        Configuration = configuration;

    protected static RuleFactory CreateFactory<TRuleCheck>() where TRuleCheck : SymbolicRuleCheck, new() =>
        new RuleFactory<TRuleCheck>();

    protected static RuleFactory CreateFactory<TRuleCheck, TSonarFallback>()
        where TRuleCheck : SymbolicRuleCheck, new()
        where TSonarFallback : new() =>
        new RuleFactory<TRuleCheck, TSonarFallback>();

    protected void Analyze(SonarAnalysisContext analysisContext, SonarSyntaxNodeReportingContext nodeContext)
    {
        if (nodeContext.SemanticModel.GetDeclaredSymbol(nodeContext.Node) is { } symbol)
        {
            Analyze(analysisContext, nodeContext, symbol);
        }
    }

    protected void Analyze(SonarAnalysisContext analysisContext, SonarSyntaxNodeReportingContext nodeContext, ISymbol symbol)
    {
        if (nodeContext.Node is { ContainsDiagnostics: false })
        {
            AnalyzeSonar(nodeContext, symbol);
            if (ControlFlowGraph.IsAvailable)
            {
                AnalyzeRoslyn(analysisContext, nodeContext, symbol);
            }
        }
    }

    private void AnalyzeRoslyn(SonarAnalysisContext analysisContext, SonarSyntaxNodeReportingContext nodeContext, ISymbol symbol)
    {
        var checks = AllRules
            .Where(x => x.Key.IsEnabled(nodeContext))
            .GroupBy(x => x.Value.Type)                                                                 // Multiple DiagnosticDescriptors (S2583, S2589) can share the same check type
            .Select(x => x.First().Value.CreateInstance(Configuration, analysisContext, nodeContext))   // We need just one instance in that case
            .Where(x => x?.ShouldExecute() is true)
            .ToArray();
        if (checks.Any())
        {
            try
            {
                if (CreateCfg(nodeContext.SemanticModel, nodeContext.Node, nodeContext.Cancel) is { } cfg)
                {
                    var engine = new RoslynSymbolicExecution(cfg, SyntaxClassifier, checks, nodeContext.Cancel);
                    engine.Execute();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new SymbolicExecutionException(ex, symbol, nodeContext.Node.GetLocation());
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

        public SymbolicRuleCheck CreateInstance(IAnalyzerConfiguration configuration, SonarAnalysisContext analysisContext, SonarSyntaxNodeReportingContext nodeContext)
        {
            if (configuration.ForceSonarCfg && createSonarFallbackInstance is not null)
            {
                return null;
            }

            var ret = createInstance();
            ret.Init(analysisContext, nodeContext);
            return ret;
        }

        public object CreateSonarFallback(IAnalyzerConfiguration configuration) =>
            createSonarFallbackInstance is not null
            && (configuration.ForceSonarCfg || !ControlFlowGraph.IsAvailable)   // ControlFlowGraph.IsAvailable is not unit testable. There's a "Roslyn.1.3.1" .NET IT to test it.
                ? createSonarFallbackInstance()
                : null;
    }

    protected sealed class RuleFactory<TCheck> : RuleFactory
        where TCheck : SymbolicRuleCheck, new()
    {
        public RuleFactory() : base(typeof(TCheck), () => new TCheck(), null) { }
    }

    protected sealed class RuleFactory<TCheck, TSonarFallback> : RuleFactory
        where TCheck : SymbolicRuleCheck, new()
        where TSonarFallback : new()
    {
        public RuleFactory() : base(typeof(TCheck), () => new TCheck(), () => new TSonarFallback()) { }
    }
}
