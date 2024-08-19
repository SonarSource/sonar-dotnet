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

using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodParameterUnused : MethodParameterUnusedBase
    {
        internal const string IsRemovableKey = "IsRemovable";
        private const string MessageUnused = "unused method parameter '{0}'";
        private const string MessageDead = "parameter '{0}', whose value is ignored in the method";
        private const string MessageFormat = "Remove this {0}.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private readonly bool useSonarCfg;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public MethodParameterUnused() : this(AnalyzerConfiguration.AlwaysEnabled) { }

        internal /* for testing */ MethodParameterUnused(IAnalyzerConfiguration configuration) =>
            useSonarCfg = configuration.UseSonarCfg();

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var declaration = CreateContext(c);
                    if ((declaration.Body == null && declaration.ExpressionBody == null)
                        || declaration.Body?.Statements.Count == 0  // Don't report on empty methods
                        || declaration.Symbol == null
                        || !declaration.Symbol.ContainingType.IsClassOrStruct()
                        || declaration.Symbol.IsMainMethod()
                        || OnlyThrowsNotImplementedException(declaration))
                    {
                        return;
                    }

                    ReportUnusedParametersOnMethod(declaration);
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKindEx.LocalFunctionStatement);

        private static MethodContext CreateContext(SonarSyntaxNodeReportingContext c)
        {
            if (c.Node is BaseMethodDeclarationSyntax method)
            {
                return new MethodContext(c, method);
            }
            else if (c.Node.IsKind(SyntaxKindEx.LocalFunctionStatement))
            {
                return new MethodContext(c, (LocalFunctionStatementSyntaxWrapper)c.Node);
            }
            else
            {
                throw new InvalidOperationException("Unexpected Node: " + c.Node);
            }
        }

        private static bool OnlyThrowsNotImplementedException(MethodContext declaration)
        {
            if (declaration.Body is not null && declaration.Body.Statements.Count != 1)
            {
                return false;
            }

            var throwExpressions = Enumerable.Empty<ExpressionSyntax>();
            if (declaration.ExpressionBody is not null)
            {
                if (ThrowExpressionSyntaxWrapper.IsInstance(declaration.ExpressionBody.Expression))
                {
                    throwExpressions = new[] { ((ThrowExpressionSyntaxWrapper)declaration.ExpressionBody.Expression).Expression };
                }
            }
            else
            {
                throwExpressions = declaration.Body.Statements.OfType<ThrowStatementSyntax>().Select(tss => tss.Expression);
            }

            return throwExpressions
                .OfType<ObjectCreationExpressionSyntax>()
                .Select(x => declaration.Context.SemanticModel.GetSymbolInfo(x).Symbol)
                .OfType<IMethodSymbol>()
                .Any(x => x is not null && x.ContainingType.Is(KnownType.System_NotImplementedException));
        }

        private void ReportUnusedParametersOnMethod(MethodContext declaration)
        {
            if (!MethodCanBeSafelyChanged(declaration.Symbol))
            {
                return;
            }

            var unusedParameters = GetUnusedParameters(declaration);
            if (unusedParameters.Any()
                && !IsUsedAsEventHandlerFunctionOrAction(declaration)
                && !IsCandidateSerializableConstructor(unusedParameters, declaration.Symbol))
            {
                ReportOnUnusedParameters(declaration, unusedParameters, MessageUnused);
            }

            ReportOnDeadParametersAtEntry(declaration, unusedParameters);
        }

        private void ReportOnDeadParametersAtEntry(MethodContext declaration, IImmutableList<IParameterSymbol> noReportOnParameters)
        {
            if (declaration.Context.Node.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                return;
            }

            var excludedParameters = noReportOnParameters;
            if (declaration.Symbol.IsExtensionMethod)
            {
                excludedParameters = excludedParameters.Add(declaration.Symbol.Parameters.First());
            }
            excludedParameters = excludedParameters.AddRange(declaration.Symbol.Parameters.Where(p => p.RefKind != RefKind.None));

            var candidateParameters = declaration.Symbol.Parameters.Except(excludedParameters);
            if (candidateParameters.Any() && ComputeLva(declaration) is { } lva)
            {
                ReportOnUnusedParameters(declaration, candidateParameters.Except(lva.LiveInEntryBlock).Except(lva.CapturedVariables), MessageDead, isRemovable: false);
            }
        }

        private LvaResult ComputeLva(MethodContext declaration)
        {
            if (useSonarCfg)
            {
                return CSharpControlFlowGraph.TryGet(declaration.Context.Node, declaration.Context.SemanticModel, out var cfg)
                    ? new LvaResult(declaration, cfg)
                    : null;
            }
            else
            {
                return declaration.Context.Node.CreateCfg(declaration.Context.SemanticModel, declaration.Context.Cancel) is { } cfg
                    ? new LvaResult(cfg, declaration.Context.Cancel)
                    : null;
            }
        }

        private static void ReportOnUnusedParameters(MethodContext declaration, IEnumerable<ISymbol> parametersToReportOn, string messagePattern, bool isRemovable = true)
        {
            if (declaration.ParameterList == null)
            {
                return;
            }

            var parameters = declaration.ParameterList.Parameters
                .Select(x => new NodeAndSymbol(x, declaration.Context.SemanticModel.GetDeclaredSymbol(x)))
                .Where(x => x.Symbol is not null);

            foreach (var parameter in parameters.Where(x => parametersToReportOn.Contains(x.Symbol)))
            {
                declaration.Context.ReportIssue(
                    Rule,
                    parameter.Node,
                    ImmutableDictionary<string, string>.Empty.Add(IsRemovableKey, isRemovable.ToString()),
                    string.Format(messagePattern, parameter.Symbol.Name));
            }
        }

        private static bool MethodCanBeSafelyChanged(IMethodSymbol methodSymbol) =>
            methodSymbol.GetEffectiveAccessibility() == Accessibility.Private
            && !methodSymbol.GetAttributes().Any()
            && methodSymbol.IsChangeable()
            && !methodSymbol.IsEventHandler();

        private static IImmutableList<IParameterSymbol> GetUnusedParameters(MethodContext declaration)
        {
            var usedParameters = new HashSet<IParameterSymbol>();
            var bodies = declaration.Context.Node.IsKind(SyntaxKind.ConstructorDeclaration)
                ? new SyntaxNode[] { declaration.Body, declaration.ExpressionBody, ((ConstructorDeclarationSyntax)declaration.Context.Node).Initializer }
                : new SyntaxNode[] { declaration.Body, declaration.ExpressionBody };

            foreach (var body in bodies.WhereNotNull())
            {
                usedParameters.UnionWith(GetUsedParameters(declaration.Symbol.Parameters, body, declaration.Context.SemanticModel));
            }

            var unusedParameter = declaration.Symbol.Parameters.Except(usedParameters);
            if (declaration.Symbol.IsExtensionMethod)
            {
                unusedParameter = unusedParameter.Except(new[] { declaration.Symbol.Parameters.First() });
            }

            return unusedParameter.Except(usedParameters).ToImmutableArray();
        }

        private static ISet<IParameterSymbol> GetUsedParameters(ImmutableArray<IParameterSymbol> parameters, SyntaxNode body, SemanticModel semanticModel) =>
            body.DescendantNodes()
                .Where(x => x.IsKind(SyntaxKind.IdentifierName))
                .Select(x => semanticModel.GetSymbolInfo(x).Symbol as IParameterSymbol)
                .Where(x => x is not null && parameters.Contains(x))
                .ToHashSet();

        private static bool IsUsedAsEventHandlerFunctionOrAction(MethodContext declaration) =>
            declaration.Symbol.ContainingType.DeclaringSyntaxReferences.Select(x => x.GetSyntax())
                .Any(x => IsMethodUsedAsEventHandlerFunctionOrActionWithinNode(declaration.Symbol, x, x.EnsureCorrectSemanticModelOrDefault(declaration.Context.SemanticModel)));

        private static bool IsMethodUsedAsEventHandlerFunctionOrActionWithinNode(IMethodSymbol methodSymbol, SyntaxNode typeDeclaration, SemanticModel semanticModel) =>
            typeDeclaration.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Any(x => IsMethodUsedAsEventHandlerFunctionOrActionInExpression(methodSymbol, x, semanticModel));

        private static bool IsMethodUsedAsEventHandlerFunctionOrActionInExpression(IMethodSymbol methodSymbol, ExpressionSyntax expression, SemanticModel semanticModel) =>
            !expression.IsKind(SyntaxKind.InvocationExpression)
            && semanticModel is not null
            && IsStandaloneExpression(expression)
            && methodSymbol.Equals(semanticModel.GetSymbolInfo(expression).Symbol?.OriginalDefinition);

        private static bool IsStandaloneExpression(ExpressionSyntax expression)
        {
            var parentAsAssignment = expression.Parent as AssignmentExpressionSyntax;

            return expression.Parent is not ExpressionSyntax
                || (parentAsAssignment is not null && ReferenceEquals(expression, parentAsAssignment.Right));
        }

        private static bool IsCandidateSerializableConstructor(IImmutableList<IParameterSymbol> unusedParameters, IMethodSymbol methodSymbol) =>
            unusedParameters.Count == 1
            && methodSymbol.MethodKind == MethodKind.Constructor
            && methodSymbol.Parameters.Length == 2
            && methodSymbol.Parameters.All(parameter => !parameter.IsOptional)
            && unusedParameters[0].Equals(methodSymbol.Parameters[1])
            && methodSymbol.ContainingType.Implements(KnownType.System_Runtime_Serialization_ISerializable)
            && methodSymbol.Parameters[0].IsType(KnownType.System_Runtime_Serialization_SerializationInfo)
            && methodSymbol.Parameters[1].IsType(KnownType.System_Runtime_Serialization_StreamingContext);

        private sealed class MethodContext
        {
            public readonly SonarSyntaxNodeReportingContext Context;
            public readonly IMethodSymbol Symbol;
            public readonly ParameterListSyntax ParameterList;
            public readonly BlockSyntax Body;
            public readonly ArrowExpressionClauseSyntax ExpressionBody;

            public MethodContext(SonarSyntaxNodeReportingContext context, BaseMethodDeclarationSyntax declaration)
                : this(context, declaration.ParameterList, declaration.Body, declaration.ExpressionBody()) { }

            public MethodContext(SonarSyntaxNodeReportingContext context, LocalFunctionStatementSyntaxWrapper declaration)
                : this(context, declaration.ParameterList, declaration.Body, declaration.ExpressionBody) { }

            public MethodContext(SonarSyntaxNodeReportingContext context, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody)
            {
                Context = context;
                Symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as IMethodSymbol;
                ParameterList = parameterList;
                Body = body;
                ExpressionBody = expressionBody;
            }
        }

        private sealed class LvaResult
        {
            public readonly IReadOnlyCollection<ISymbol> LiveInEntryBlock;
            public readonly IReadOnlyCollection<ISymbol> CapturedVariables;

            public LvaResult(MethodContext declaration, IControlFlowGraph cfg)
            {
                var lva = new SonarCSharpLiveVariableAnalysis(cfg, declaration.Symbol, declaration.Context.SemanticModel, declaration.Context.Cancel);
                LiveInEntryBlock = lva.LiveIn(cfg.EntryBlock).OfType<IParameterSymbol>().ToImmutableArray();
                CapturedVariables = lva.CapturedVariables;
            }

            public LvaResult(ControlFlowGraph cfg, CancellationToken cancel)
            {
                var lva = new RoslynLiveVariableAnalysis(cfg, cancel);
                LiveInEntryBlock = lva.LiveIn(cfg.EntryBlock).OfType<IParameterSymbol>().ToImmutableArray();
                CapturedVariables = lva.CapturedVariables;
            }
        }
    }
}
