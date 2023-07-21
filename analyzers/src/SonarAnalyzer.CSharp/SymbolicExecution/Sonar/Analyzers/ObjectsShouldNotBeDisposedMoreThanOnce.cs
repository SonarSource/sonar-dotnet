/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Sonar.Analyzers
{
    internal sealed class ObjectsShouldNotBeDisposedMoreThanOnce : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S3966";
        private const string MessageFormat = "Refactor this code to make sure '{0}' is disposed only once.";

        internal static readonly DiagnosticDescriptor S3966 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(S3966);

        public ISymbolicExecutionAnalysisContext CreateContext(SonarSyntaxNodeReportingContext context, SonarExplodedGraph explodedGraph) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            // Store the nodes that should be reported and ignore duplicate reports for the same node.
            // This is needed because we generate two CFG blocks for the finally statements and even
            // though the syntax nodes are the same, when there is a return inside a try/catch block
            // the walked CFG paths could be different and FPs will appear.
            private readonly Dictionary<SyntaxNode, string> nodesToReport = new();

            public AnalysisContext(SonarExplodedGraph explodedGraph) =>
                explodedGraph.AddExplodedGraphCheck(new ObjectDisposedPointerCheck(explodedGraph, this));

            public bool SupportsPartialResults => true;

            public IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation) =>
                nodesToReport.Select(item => Diagnostic.Create(S3966, item.Key.GetLocation().EnsureMappedLocation(), item.Value));

            public void Dispose()
            {
                // Nothing to dispose
            }

            public void AddDisposed(string symbolName, SyntaxNode node) =>
                nodesToReport[node] = symbolName;
        }

        private sealed class ObjectDisposedPointerCheck : ExplodedGraphCheck
        {
            private readonly AnalysisContext context;

            public ObjectDisposedPointerCheck(SonarExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;

            public override ProgramState PreProcessUsingStatement(ProgramPoint programPoint, ProgramState programState)
            {
                var newProgramState = programState;
                var usingFinalizer = (UsingEndBlock)programPoint.Block;
                var disposables = usingFinalizer.Identifiers
                    .Select(i =>
                    new
                    {
                        SyntaxNode = i.Parent,
                        Symbol = semanticModel.GetDeclaredSymbol(i.Parent)
                            ?? semanticModel.GetSymbolInfo(i.Parent).Symbol
                    });

                foreach (var disposable in disposables)
                {
                    newProgramState = ProcessDisposableSymbol(newProgramState, disposable.SyntaxNode, disposable.Symbol);
                }

                return newProgramState;
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.CurrentInstruction;

                if (instruction is InvocationExpressionSyntax invocation)
                {
                    return VisitInvocationExpression(invocation, programState);
                }

                if (instruction is VariableDeclaratorSyntax declarator &&
                    declarator.Parent?.Parent is LocalDeclarationStatementSyntax declaration &&
                    declaration.UsingKeyword().IsKind(SyntaxKind.UsingKeyword))
                {
                    return PreProcessVariableDeclarator(programState, declarator);
                }

                return programState;
            }

            private ProgramState PreProcessVariableDeclarator(ProgramState programState, VariableDeclaratorSyntax declarator)
            {
                if (declarator.Initializer?.Value == null || !programState.HasValue)
                {
                    return programState;
                }

                // We need to associate the symbolic value to the symbol here first, as it hasn't been done yet, since we
                // are are pre-processing the VariableDeclarator instruction
                var disposableSymbol = semanticModel.GetDeclaredSymbol(declarator);
                var newProgramState = programState.StoreSymbolicValue(disposableSymbol, programState.PeekValue());

                return ProcessDisposableSymbol(newProgramState, declarator, disposableSymbol);
            }

            private ProgramState VisitInvocationExpression(InvocationExpressionSyntax instruction, ProgramState programState)
            {
                var newProgramState = programState;

                var disposeMethodSymbol = semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                if (disposeMethodSymbol.IsIDisposableDispose())
                {
                    var disposedObject =
                        // Direct call to Dispose()
                        instruction.Expression as IdentifierNameSyntax
                        // Call to Dispose on local variable, field or this
                        ?? (instruction.Expression as MemberAccessExpressionSyntax)?.Expression;
                    if (disposedObject != null)
                    {
                        var disposableSymbol = semanticModel.GetSymbolInfo(disposedObject).Symbol;

                        if (disposableSymbol is IMethodSymbol
                            // Special case - if the parameter symbol is "this" then resolve it to the containing type
                            || (disposableSymbol is IParameterSymbol parameter && parameter.IsThis))
                        {
                            disposableSymbol = disposableSymbol.ContainingType;
                        }
                        newProgramState = ProcessDisposableSymbol(newProgramState, disposedObject, disposableSymbol);
                    }
                }

                return newProgramState;
            }

            private ProgramState ProcessDisposableSymbol(ProgramState programState, SyntaxNode disposeInstruction, ISymbol disposableSymbol)
            {
                if (disposableSymbol == null) // DisposableSymbol is null when we invoke an array element
                {
                    return programState;
                }

                if (disposableSymbol.HasConstraint(DisposableConstraint.Disposed, programState))
                {
                    context.AddDisposed(disposableSymbol.Name, disposeInstruction);
                    return programState;
                }

                // We should not replace Null constraint because having Disposed constraint
                // implies having NotNull constraint, which is incorrect.
                if (disposableSymbol.HasConstraint(ObjectConstraint.Null, programState))
                {
                    return programState;
                }

                var newProgramState = programState;
                if (disposableSymbol is INamedTypeSymbol && newProgramState.GetSymbolValue(disposableSymbol) == null)
                {
                    // Dispose is called on current instance but we don't usually store a symbol for this
                    // so we store it and then associate the Disposed constraint.
                    newProgramState = newProgramState.StoreSymbolicValue(disposableSymbol, SymbolicValue.This);
                }
                return disposableSymbol.SetConstraint(DisposableConstraint.Disposed, newProgramState);
            }
        }
    }
}
