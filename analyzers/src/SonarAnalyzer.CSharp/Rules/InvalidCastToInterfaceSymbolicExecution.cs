/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    internal sealed class InvalidCastToInterfaceSymbolicExecution : ISymbolicExecutionAnalyzer
    {
        private const string MessageDefinite = "Nullable is known to be empty, this cast throws an exception.";

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics  { get; } = ImmutableArray.Create(InvalidCastToInterfaceRuleConstants.Rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph, context);

        internal sealed class NullableCastCheck : ExplodedGraphCheck
        {
            private readonly SyntaxNodeAnalysisContext context;

            public NullableCastCheck(CSharpExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            public NullableCastCheck(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
                : this(explodedGraph)
            {
                this.context = context;
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                return instruction.IsKind(SyntaxKind.CastExpression)
                    ? ProcessCastAccess(programState, (CastExpressionSyntax)instruction)
                    : programState;
            }

            private ProgramState ProcessCastAccess(ProgramState programState, CastExpressionSyntax castExpression)
            {
                var typeExpression = semanticModel.GetTypeInfo(castExpression.Expression).Type;
                if (typeExpression == null ||
                    !typeExpression.OriginalDefinition.Is(KnownType.System_Nullable_T))
                {
                    return programState;
                }

                var type = semanticModel.GetTypeInfo(castExpression.Type).Type;

                if (type == null ||
                    type.OriginalDefinition.Is(KnownType.System_Nullable_T) ||
                    !semanticModel.Compilation.ClassifyConversion(typeExpression, type).IsNullable ||
                    !programState.HasConstraint(programState.PeekValue(), ObjectConstraint.Null))
                {
                    return programState;
                }

                if (!context.Equals(default(SyntaxNodeAnalysisContext)))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(InvalidCastToInterfaceRuleConstants.Rule, castExpression.GetLocation(), MessageDefinite));
                }

                return null;
            }
        }

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            public bool SupportsPartialResults => true;

            public AnalysisContext(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
                explodedGraph.AddExplodedGraphCheck(new NullableCastCheck(explodedGraph, context));

            public void Dispose()
            {
                // Needed in order to implement ISymbolicExecutionAnalysisContext.
            }

            // Nothing to return since the issues are raised during analysis.
            public IEnumerable<Diagnostic> GetDiagnostics() => Enumerable.Empty<Diagnostic>();
        }
    }
}
