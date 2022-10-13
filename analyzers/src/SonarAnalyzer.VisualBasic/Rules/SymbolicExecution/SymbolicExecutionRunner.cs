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

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class SymbolicExecutionRunner : SymbolicExecutionRunnerBase
    {
        public SymbolicExecutionRunner() : base(AnalyzerConfiguration.AlwaysEnabled) { }

        protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; } = ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
            .Add(LocksReleasedAllPaths.S2222, CreateFactory<LocksReleasedAllPaths>())
            .Add(NullPointerDereference.S2259, CreateFactory<NullPointerDereference>());

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<MethodBlockBaseSyntax>(context, c, x => x),
                SyntaxKind.ConstructorBlock,
                SyntaxKind.OperatorBlock,
                SyntaxKind.SubBlock,
                SyntaxKind.FunctionBlock,
                SyntaxKind.GetAccessorBlock,
                SyntaxKind.SetAccessorBlock,
                SyntaxKind.AddHandlerAccessorBlock,
                SyntaxKind.RemoveHandlerAccessorBlock,
                SyntaxKind.RaiseEventAccessorBlock);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (LambdaExpressionSyntax)c.Node;
                    if (c.SemanticModel.GetSymbolInfo(declaration).Symbol is { } symbol && !c.IsInExpressionTree())
                    {
                        Analyze(context, c, declaration, symbol);
                    }
                },
                SyntaxKind.SingleLineFunctionLambdaExpression,
                SyntaxKind.SingleLineSubLambdaExpression,
                SyntaxKind.MultiLineFunctionLambdaExpression,
                SyntaxKind.MultiLineSubLambdaExpression);
        }

        protected override ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node, CancellationToken cancel) =>
            node.CreateCfg(model, cancel);

        protected override void AnalyzeSonar(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun, SyntaxNode body, ISymbol symbol)
        {
            // There are no old Sonar rules in VB.NET
        }
    }
}
