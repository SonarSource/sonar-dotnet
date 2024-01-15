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
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using SonarAnalyzer.SymbolicExecution.Roslyn.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public class SymbolicExecutionRunner : SymbolicExecutionRunnerBase
{
    public SymbolicExecutionRunner() : base(AnalyzerConfiguration.AlwaysEnabled) { }

    protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; } = ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
        .Add(HashesShouldHaveUnpredictableSalt.S2053, CreateFactory<HashesShouldHaveUnpredictableSalt>())
        .Add(LocksReleasedAllPaths.S2222, CreateFactory<LocksReleasedAllPaths>())
        .Add(NullPointerDereference.S2259, CreateFactory<NullPointerDereference>())
        .Add(ConditionEvaluatesToConstant.S2583, CreateFactory<ConditionEvaluatesToConstant>())
        .Add(ConditionEvaluatesToConstant.S2589, CreateFactory<ConditionEvaluatesToConstant>())
        .Add(InitializationVectorShouldBeRandom.S3329, CreateFactory<InitializationVectorShouldBeRandom>())
        .Add(EmptyNullableValueAccess.S3655, CreateFactory<EmptyNullableValueAccess>())
        .Add(PublicMethodArgumentsShouldBeCheckedForNull.S3900, CreateFactory<PublicMethodArgumentsShouldBeCheckedForNull>())
        .Add(CalculationsShouldNotOverflow.S3949, CreateFactory<CalculationsShouldNotOverflow>())
        .Add(ObjectsShouldNotBeDisposedMoreThanOnce.S3966, CreateFactory<ObjectsShouldNotBeDisposedMoreThanOnce>())
        .Add(EmptyCollectionsShouldNotBeEnumerated.S4158, CreateFactory<EmptyCollectionsShouldNotBeEnumerated>())
        .Add(RestrictDeserializedTypes.S5773, CreateFactory<RestrictDeserializedTypes>());

    protected override SyntaxClassifierBase SyntaxClassifier => VisualBasicSyntaxClassifier.Instance;

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c => Analyze(context, c),
            SyntaxKind.ConstructorBlock,
            SyntaxKind.OperatorBlock,
            SyntaxKind.SubBlock,
            SyntaxKind.FunctionBlock,
            SyntaxKind.GetAccessorBlock,
            SyntaxKind.SetAccessorBlock,
            SyntaxKind.AddHandlerAccessorBlock,
            SyntaxKind.RemoveHandlerAccessorBlock,
            SyntaxKind.RaiseEventAccessorBlock);

        context.RegisterNodeAction(
            c =>
            {
                var declaration = (LambdaExpressionSyntax)c.Node;
                if (c.SemanticModel.GetSymbolInfo(declaration).Symbol is { } symbol && !c.IsInExpressionTree())
                {
                    Analyze(context, c, symbol);
                }
            },
            SyntaxKind.SingleLineFunctionLambdaExpression,
            SyntaxKind.SingleLineSubLambdaExpression,
            SyntaxKind.MultiLineFunctionLambdaExpression,
            SyntaxKind.MultiLineSubLambdaExpression);
    }

    protected override ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node, CancellationToken cancel) =>
        node.CreateCfg(model, cancel);

    protected override void AnalyzeSonar(SonarSyntaxNodeReportingContext context, ISymbol symbol)
    {
        // There are no old Sonar rules in VB.NET
    }
}
