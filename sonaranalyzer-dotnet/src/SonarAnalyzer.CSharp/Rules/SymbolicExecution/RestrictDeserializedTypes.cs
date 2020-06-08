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

using System;
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
using SonarAnalyzer.SymbolicExecution.Common.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    internal sealed class RestrictDeserializedTypes : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S5773";
        private const string MessageFormat = "Restrict types of objects allowed to be deserialized.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly List<SyntaxNode> nodes = new List<SyntaxNode>();

            public AnalysisContext(AbstractExplodedGraph explodedGraph)
            {
                explodedGraph.AddExplodedGraphCheck(new SerializationBinderCheck(explodedGraph, AddNode));
            }

            public bool SupportsPartialResults => true;

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                nodes.Select(node => Diagnostic.Create(rule, node.GetLocation()));

            public void Dispose()
            {
                // Nothing to do here.
            }

            private void AddNode(SyntaxNode syntaxNode) => nodes.Add(syntaxNode);
        }

        private sealed class SerializationBinderCheck : ExplodedGraphCheck
        {
            private readonly Action<SyntaxNode> addNode;
            private readonly ImmutableArray<KnownType> typesWithBinder =
                ImmutableArray.Create(
                    KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter);

            public SerializationBinderCheck(AbstractExplodedGraph explodedGraph, Action<SyntaxNode> addNode)
                : base(explodedGraph)
            {
                this.addNode = addNode ?? throw new ArgumentNullException(nameof(addNode));
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue,
                SyntaxNode instruction)
            {
                if (instruction.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var typeSymbol = semanticModel.GetTypeInfo(instruction).Type;
                    if (typeSymbol.IsAny(typesWithBinder))
                    {
                        programState = AddInvalidSerializationBinderConstraint(instruction, symbolicValue, programState);
                    }
                }

                return base.ObjectCreated(programState, symbolicValue, instruction);
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];
                if (instruction is InvocationExpressionSyntax invocation)
                {
                    return VisitInvocationExpression(invocation, programState);
                }
                return base.PreProcessInstruction(programPoint, programState);
            }

            private ProgramState VisitInvocationExpression(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                var symbol = semanticModel.GetSymbolInfo(invocation).Symbol.ContainingSymbol;
                var type = symbol.GetSymbolType();
                if (type.IsAny(typesWithBinder))
                {
                    var symbolicValue = programState.GetSymbolValue(symbol);

                    if (programState.HasConstraint(symbolicValue, InvalidSerializationBinder.Invalid))
                    {
                        addNode(invocation);
                    }
                }

                // Check constraint and raise issue
                return programState;
            }

            private ProgramState AddInvalidSerializationBinderConstraint(SyntaxNode instruction,
                SymbolicValue symbolicValue, ProgramState programState)
            {
                var symbol = semanticModel.GetSymbolInfo(instruction).Symbol.ContainingSymbol;

                programState = programState.StoreSymbolicValue(symbol, symbolicValue);

                return symbol.SetConstraint(InvalidSerializationBinder.Invalid, programState);
            }

            // ToDo:
            // - add symbolic value constraint when objects are created   - done
            // - on property set update binder status
            //      - check if the binder exists and is not null
            //      - validate binder
            // - when Deserialize is called check the binder status       - done
        }
    }
}

