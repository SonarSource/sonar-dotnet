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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.Rules.CSharp
{
    internal sealed class EmptyNullableValueAccess : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S3655";
        private const string MessageFormat = "'{0}' is null on at least one execution path.";
        private const string ValueLiteral = "Value";
        private const string HasValueLiteral = "HasValue";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph) => new AnalysisContext(explodedGraph);

        internal sealed class NullValueAccessedCheck : ExplodedGraphCheck
        {
            public event EventHandler<MemberAccessedEventArgs> ValuePropertyAccessed;

            public NullValueAccessedCheck(CSharpExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            private void OnValuePropertyAccessed(IdentifierNameSyntax identifier)
            {
                ValuePropertyAccessed?.Invoke(this, new MemberAccessedEventArgs(identifier));
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                return instruction.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    ? ProcessMemberAccess(programState, (MemberAccessExpressionSyntax)instruction)
                    : programState;
            }

            private ProgramState ProcessMemberAccess(ProgramState programState, MemberAccessExpressionSyntax memberAccess)
            {
                if (!(memberAccess.Expression.RemoveParentheses() is IdentifierNameSyntax identifier) ||
                    memberAccess.Name.Identifier.ValueText != ValueLiteral)
                {
                    return programState;
                }

                var symbol = this.semanticModel.GetSymbolInfo(identifier).Symbol;
                if (!IsNullableLocalScoped(symbol))
                {
                    return programState;
                }

                if (symbol.HasConstraint(ObjectConstraint.Null, programState))
                {
                    OnValuePropertyAccessed(identifier);
                    return null;
                }

                return programState;
            }

            private bool IsNullableLocalScoped(ISymbol symbol)
            {
                var type = symbol.GetSymbolType();
                return type != null &&
                    type.OriginalDefinition.Is(KnownType.System_Nullable_T) &&
                    this.explodedGraph.IsSymbolTracked(symbol);
            }

            private bool IsHasValueAccess(MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.ValueText == HasValueLiteral &&
                    (this.semanticModel.GetTypeInfo(memberAccess.Expression).Type?.OriginalDefinition).Is(KnownType.System_Nullable_T);
            }

            internal bool TryProcessInstruction(MemberAccessExpressionSyntax instruction, ProgramState programState, out ProgramState newProgramState)
            {
                if (IsHasValueAccess(instruction))
                {
                    newProgramState = programState.PopValue(out var nullable);
                    newProgramState = newProgramState.PushValue(new HasValueAccessSymbolicValue(nullable));
                    return true;
                }

                newProgramState = programState;
                return false;
            }
        }

        private sealed class HasValueAccessSymbolicValue : MemberAccessSymbolicValue
        {
            public HasValueAccessSymbolicValue(SymbolicValue nullable)
                : base(nullable, HasValueLiteral)
            {
            }

            public override IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState programState)
            {
                if (!(constraint is BoolConstraint boolConstraint))
                {
                    return new[] { programState };
                }

                var nullabilityConstraint = boolConstraint == BoolConstraint.True
                    ? ObjectConstraint.NotNull
                    : ObjectConstraint.Null;

                return MemberExpression.TrySetConstraint(nullabilityConstraint, programState);
            }
        }

        private class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly HashSet<IdentifierNameSyntax> nullIdentifiers = new HashSet<IdentifierNameSyntax>();
            private readonly NullValueAccessedCheck nullPointerCheck;

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                this.nullIdentifiers.Select(nullIdentifier => Diagnostic.Create(rule, nullIdentifier.Parent.GetLocation(), nullIdentifier.Identifier.ValueText));

            public AnalysisContext(CSharpExplodedGraph explodedGraph)
            {
                this.nullPointerCheck = new NullValueAccessedCheck(explodedGraph);
                this.nullPointerCheck.ValuePropertyAccessed += AddIdentifier;

                explodedGraph.AddExplodedGraphCheck(this.nullPointerCheck);
            }

            private void AddIdentifier(object sender, MemberAccessedEventArgs args) => this.nullIdentifiers.Add(args.Identifier);

            public void Dispose() => this.nullPointerCheck.ValuePropertyAccessed -= AddIdentifier;
        }
    }
}
