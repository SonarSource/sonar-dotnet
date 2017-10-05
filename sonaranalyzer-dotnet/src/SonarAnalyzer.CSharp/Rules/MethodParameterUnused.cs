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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using SonarAnalyzer.SymbolicExecution.LiveVariableAnalysis;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class MethodParameterUnused : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1172";
        private const string MessageFormat = "Remove this {0}.";
        internal const string MessageUnused = "unused method parameter '{0}'";
        internal const string MessageDead = "parameter '{0}', whose value is ignored in the method";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        internal const string IsRemovableKey = "IsRemovable";

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (BaseMethodDeclarationSyntax)c.Node;

                    if (declaration.Body == null ||
                        declaration.Body.Statements.Count == 0) // Don't report on empty methods
                    {
                        return;
                    }

                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (symbol == null ||
                        !symbol.ContainingType.IsClassOrStruct() ||
                        symbol.IsMainMethod() ||
                        IsBodyOnlyThrowNotImplementedException(declaration, c.SemanticModel))
                    {
                        return;
                    }

                    ReportUnusedParametersOnMethod(declaration, symbol, c);
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);
        }

        private static bool IsBodyOnlyThrowNotImplementedException(BaseMethodDeclarationSyntax declaration,
            SemanticModel semanticModel)
        {
            return declaration.Body.Statements.Count == 1 &&
                declaration.Body.Statements
                           .OfType<ThrowStatementSyntax>()
                           .Select(tss => tss.Expression)
                           .OfType<ObjectCreationExpressionSyntax>()
                           .Select(oces => semanticModel.GetSymbolInfo(oces).Symbol)
                           .OfType<IMethodSymbol>()
                           .Any(s => s != null && s.ContainingType.Is(KnownType.System_NotImplementedException));
        }

        private static void ReportUnusedParametersOnMethod(BaseMethodDeclarationSyntax declaration, IMethodSymbol methodSymbol,
            SyntaxNodeAnalysisContext context)
        {
            if (!MethodCanBeSafelyChanged(methodSymbol))
            {
                return;
            }

            var unusedParameters = GetUnusedParameters(declaration, methodSymbol, context.SemanticModel);
            if (unusedParameters.Any() &&
                !IsUsedAsEventHandlerFunctionOrAction(methodSymbol, context.SemanticModel.Compilation) &&
                !IsCandidateSerializableConstructor(unusedParameters, methodSymbol))
            {
                ReportOnUnusedParameters(declaration, unusedParameters, MessageUnused, context);
            }

            ReportOnDeadParametersAtEntry(declaration, methodSymbol, unusedParameters, context);
        }

        private static void ReportOnDeadParametersAtEntry(BaseMethodDeclarationSyntax declaration, IMethodSymbol methodSymbol,
            IImmutableList<IParameterSymbol> noReportOnParameters, SyntaxNodeAnalysisContext context)
        {
            if (!declaration.IsKind(SyntaxKind.MethodDeclaration) ||
                declaration.Body == null)
            {
                return;
            }

            var excludedParameters = noReportOnParameters;
            if (methodSymbol.IsExtensionMethod)
            {
                excludedParameters = excludedParameters.Add(methodSymbol.Parameters.First());
            }

            excludedParameters = excludedParameters.AddRange(methodSymbol.Parameters.Where(p => p.RefKind != RefKind.None));

            var candidateParameters = methodSymbol.Parameters.Except(excludedParameters);
            if (!candidateParameters.Any())
            {
                return;
            }

            IControlFlowGraph cfg;
            if (!CSharpControlFlowGraph.TryGet(declaration.Body, context.SemanticModel, out cfg))
            {
                return;
            }

            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, context.SemanticModel);
            var liveParameters = lva.GetLiveIn(cfg.EntryBlock).OfType<IParameterSymbol>();

            ReportOnUnusedParameters(declaration, candidateParameters.Except(liveParameters).Except(lva.CapturedVariables), MessageDead,
                context, isRemovable: false);
        }

        private static void ReportOnUnusedParameters(BaseMethodDeclarationSyntax declaration, IEnumerable<ISymbol> parametersToReportOn,
            string messagePattern, SyntaxNodeAnalysisContext context, bool isRemovable = true)
        {
            if (declaration.ParameterList == null)
            {
                return;
            }

            var parameters = declaration.ParameterList.Parameters
                .Select(p => new
                {
                    Syntax = p,
                    Symbol = context.SemanticModel.GetDeclaredSymbol(p)
                })
                .Where(p => p.Symbol != null);

            foreach (var parameter in parameters)
            {
                if (parametersToReportOn.Contains(parameter.Symbol))
                {
                    context.ReportDiagnosticWhenActive(
                        Diagnostic.Create(rule, parameter.Syntax.GetLocation(),
                        ImmutableDictionary<string, string>.Empty.Add(IsRemovableKey, isRemovable.ToString()),
                        string.Format(messagePattern, parameter.Symbol.Name)));
                }
            }
        }

        private static bool MethodCanBeSafelyChanged(IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetEffectiveAccessibility() == Accessibility.Private &&
                !methodSymbol.GetAttributes().Any() &&
                methodSymbol.IsChangeable() &&
                !methodSymbol.IsProbablyEventHandler();
        }

        private static IImmutableList<IParameterSymbol> GetUnusedParameters(BaseMethodDeclarationSyntax declaration, IMethodSymbol methodSymbol,
            SemanticModel semanticModel)
        {
            var usedParameters = new HashSet<IParameterSymbol>();

            SyntaxNode[] bodies;

            if (declaration.IsKind(SyntaxKind.MethodDeclaration))
            {
                var methodDeclararion = (MethodDeclarationSyntax)declaration;
                bodies = new SyntaxNode[] { methodDeclararion.Body, methodDeclararion.ExpressionBody };
            }
            else
            {
                var constructorDeclaration = (ConstructorDeclarationSyntax)declaration;
                bodies = new SyntaxNode[] { constructorDeclaration.Body, constructorDeclaration.Initializer };
            }

            foreach (var body in bodies.Where(b => b != null))
            {
                usedParameters.UnionWith(GetUsedParameters(methodSymbol.Parameters, body, semanticModel));
            }

            var unusedParameter = methodSymbol.Parameters.Except(usedParameters);
            if (methodSymbol.IsExtensionMethod)
            {
                unusedParameter = unusedParameter.Except(new[] { methodSymbol.Parameters.First() });
            }

            return unusedParameter.Except(usedParameters).ToImmutableArray();
        }

        private static IImmutableSet<IParameterSymbol> GetUsedParameters(ImmutableArray<IParameterSymbol> parameters, SyntaxNode body, SemanticModel semanticModel)
        {
            return body.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                .Select(identierName => semanticModel.GetSymbolInfo(identierName).Symbol as IParameterSymbol)
                .Where(symbol => symbol != null && parameters.Contains(symbol))
                .ToImmutableHashSet();
        }

        private static bool IsUsedAsEventHandlerFunctionOrAction(IMethodSymbol methodSymbol, Compilation compilation)
        {
            return methodSymbol.ContainingType.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .Any(n => IsMethodUsedAsEventHandlerFunctionOrActionWithinNode(methodSymbol, n, compilation.GetSemanticModel(n.SyntaxTree)));
        }

        private static bool IsMethodUsedAsEventHandlerFunctionOrActionWithinNode(IMethodSymbol methodSymbol, SyntaxNode typeDeclaration, SemanticModel semanticModel)
        {
            return typeDeclaration.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Any(n => IsMethodUsedAsEventHandlerFunctionOrActionInExpression(methodSymbol, n, semanticModel));
        }

        private static bool IsMethodUsedAsEventHandlerFunctionOrActionInExpression(IMethodSymbol methodSymbol, ExpressionSyntax expression, SemanticModel semanticModel)
        {
            return !expression.IsKind(SyntaxKind.InvocationExpression) &&
                IsStandaloneExpression(expression) &&
                methodSymbol.Equals(semanticModel.GetSymbolInfo(expression).Symbol?.OriginalDefinition);
        }

        private static bool IsStandaloneExpression(ExpressionSyntax expression)
        {
            var parentAsAssignment = expression.Parent as AssignmentExpressionSyntax;

            return !(expression.Parent is ExpressionSyntax) ||
                (parentAsAssignment != null && object.ReferenceEquals(expression, parentAsAssignment.Right));
        }

        private static bool IsCandidateSerializableConstructor(IImmutableList<IParameterSymbol> unusedParameters, IMethodSymbol methodSymbol)
        {
            return unusedParameters.Count == 1 &&
                methodSymbol.MethodKind == MethodKind.Constructor &&
                methodSymbol.Parameters.Length == 2 &&
                methodSymbol.Parameters.All(parameter => !parameter.IsOptional) &&
                unusedParameters[0].Equals(methodSymbol.Parameters[1]) &&
                methodSymbol.ContainingType.Implements(KnownType.System_Runtime_Serialization_ISerializable) &&
                methodSymbol.Parameters[0].IsType(KnownType.System_Runtime_Serialization_SerializationInfo) &&
                methodSymbol.Parameters[1].IsType(KnownType.System_Runtime_Serialization_StreamingContext);
        }
    }
}
