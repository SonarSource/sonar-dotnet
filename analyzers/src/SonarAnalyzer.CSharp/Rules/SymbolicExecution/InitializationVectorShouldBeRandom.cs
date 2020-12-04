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
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Common.Constraints;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    internal sealed class InitializationVectorShouldBeRandom : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S3329";
        private const string MessageFormat = "Use a dynamically-generated, random IV.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
            => new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly List<Location> locations = new List<Location>();

            public bool SupportsPartialResults => false;

            public AnalysisContext(AbstractExplodedGraph explodedGraph) =>
                explodedGraph.AddExplodedGraphCheck(new InitializationVectorCheck(explodedGraph, this));

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                locations.Distinct().Select(location => Diagnostic.Create(rule, location));

            public void AddLocation(Location location) => locations.Add(location);

            public void Dispose()
            {
                // Nothing to do here.
            }
        }

        private sealed class InitializationVectorCheck : ExplodedGraphCheck
        {
            private static readonly ImmutableArray<KnownType> vulnerableTypes =
                // ToDo: implement for all types.
                ImmutableArray.Create(KnownType.System_Security_Cryptography_SymmetricAlgorithm,
                                      KnownType.System_Security_Cryptography_AesCryptoServiceProvider);

            private readonly AnalysisContext context;

            public InitializationVectorCheck(AbstractExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;

            public override ProgramState PostProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.GetCurrentInstruction();

                programState = instruction switch
                {
                    ObjectCreationExpressionSyntax objectCreation => ObjectCreationPostProcess(objectCreation, programState),
                    InvocationExpressionSyntax invocation => InvocationExpressionPostProcess(invocation, programState),
                    ArrayCreationExpressionSyntax arrayCreation => ArrayCreationPostProcess(arrayCreation, programState),
                    AssignmentExpressionSyntax assignment => AssignmentExpressionPostProcess(assignment, programState),
                    _ => programState
                };

                return base.PostProcessInstruction(programPoint, programState);
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                if (programPoint.GetCurrentInstruction() is {} instruction && instruction is InvocationExpressionSyntax invocation)
                {
                    programState = InvocationExpressionPreProcess(invocation, programState);
                }

                return base.PreProcessInstruction(programPoint, programState);
            }

            private ProgramState ObjectCreationPostProcess(ObjectCreationExpressionSyntax objectCreation, ProgramState programState)
            {
                if (semanticModel.GetSymbolInfo(objectCreation).Symbol is {} symbol
                    && symbol.ContainingType is {} symbolType
                    && symbolType.IsAny(vulnerableTypes))
                {
                    var symbolicValue = programState.PeekValue();
                    programState = programState.SetConstraint(symbolicValue, IVInitializationSymbolicValueConstraint.NotInitialized);
                    programState = programState.SetConstraint(symbolicValue, KeyInitializationSymbolicValueConstraint.NotInitialized);
                }

                return programState;
            }

            private ProgramState InvocationExpressionPostProcess(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                if (IsCreateMethod(invocation, semanticModel))
                {
                    var symbolicValue = programState.PeekValue();
                    programState = programState.SetConstraint(symbolicValue, IVInitializationSymbolicValueConstraint.NotInitialized);
                    programState = programState.SetConstraint(symbolicValue, KeyInitializationSymbolicValueConstraint.NotInitialized);
                }
                else if (IsGenerateKeyMethod(invocation, semanticModel))
                {
                    var symbolicValue = GetSymbolicValue(invocation, programState);
                    programState = programState.SetConstraint(symbolicValue, KeyInitializationSymbolicValueConstraint.Initialized);
                }
                else if (IsGenerateIVMethod(invocation, semanticModel))
                {
                    var symbolicValue = GetSymbolicValue(invocation, programState);
                    programState = programState.SetConstraint(symbolicValue, IVInitializationSymbolicValueConstraint.Initialized);
                }
                else if (IsRNGCryptoServiceProviderSanitizer(invocation, semanticModel))
                {
                    if (semanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[0].Expression).Symbol is {} symbol
                        && symbol.GetSymbolType().Is(KnownType.System_Byte_Array)
                        && symbol.HasConstraint(ConstantByteArraySymbolicValueConstraint.Constant, programState))
                    {
                        var symbolicValue = programState.GetSymbolValue(symbol);
                        programState = programState.RemoveConstraint(symbolicValue, ConstantByteArraySymbolicValueConstraint.Constant);
                    }
                }

                return programState;
            }

            private ProgramState AssignmentExpressionPostProcess(AssignmentExpressionSyntax assignment, ProgramState programState) =>
                assignment.Left is MemberAccessExpressionSyntax memberAccess
                && IsIVMemberAccess(memberAccess)
                && semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is {} leftSymbol
                && programState.GetSymbolValue(leftSymbol) is {} leftSymbolicValue
                && semanticModel.GetSymbolInfo(assignment.Right).Symbol is {} rightSymbol
                && programState.GetSymbolValue(rightSymbol) is {} rightSymbolicValue
                && programState.HasConstraint(rightSymbolicValue, ConstantByteArraySymbolicValueConstraint.Constant)
                    ? programState.SetConstraint(leftSymbolicValue, IVInitializationSymbolicValueConstraint.NotInitialized)
                    : programState;

            private ProgramState InvocationExpressionPreProcess(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                if (IsCreateEncryptorMethod(invocation, semanticModel))
                {
                    if (invocation.ArgumentList.Arguments.Count == 0)
                    {
                        var symbolicValue = GetSymbolicValue(invocation, programState);
                        if (symbolicValue != null && HasInvalidConstraints(symbolicValue, programState))
                        {
                            context.AddLocation(invocation.GetLocation());
                        }
                    }
                    else
                    {
                        if (IsInvalidKey(invocation.ArgumentList.Arguments[0], programState))
                        {
                            context.AddLocation(invocation.GetLocation());
                        }
                        else if (IsInvalidIV(invocation.ArgumentList.Arguments[1], programState))
                        {
                            context.AddLocation(invocation.GetLocation());
                        }
                    }
                }

                return programState;
            }

            private ProgramState ArrayCreationPostProcess(ArrayCreationExpressionSyntax arrayCreation, ProgramState programState) =>
                semanticModel.GetTypeInfo(arrayCreation).Type.Is(KnownType.System_Byte_Array) && programState.HasValue
                    ? programState.SetConstraint(programState.PeekValue(), ConstantByteArraySymbolicValueConstraint.Constant)
                    : programState;

            private bool IsInvalidKey(ArgumentSyntax argument, ProgramState programState) =>
                argument.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.NameIs("Key")
                && programState.GetSymbolValue(semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol) is {} symbolicValue
                && HasNotInitializedKeyConstraint(symbolicValue, programState);

            private bool IsInvalidIV(ArgumentSyntax argument, ProgramState programState) =>
                argument.Expression switch
                {
                    MemberAccessExpressionSyntax memberAccess => memberAccess.NameIs("IV")
                                                                 && programState.GetSymbolValue(semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol) is {} symbolicValue
                                                                 && HasNotInitializedIVConstraint(symbolicValue, programState),
                    IdentifierNameSyntax identifier => programState.GetSymbolValue(semanticModel.GetSymbolInfo(identifier).Symbol) is {} symbolicValue
                                                       && programState.HasConstraint(symbolicValue, ConstantByteArraySymbolicValueConstraint.Constant),
                    _ => false
                };

            private SymbolicValue GetSymbolicValue(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                var invocationTarget = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
                var targetSymbol = semanticModel.GetSymbolInfo(invocationTarget).Symbol;

                return programState.GetSymbolValue(targetSymbol);
            }

            private bool IsIVMemberAccess(MemberAccessExpressionSyntax memberAccess) =>
                memberAccess.NameIs("IV")
                && semanticModel.GetSymbolInfo(memberAccess).Symbol.ContainingType.IsAny(vulnerableTypes);

            private static bool HasInvalidConstraints(SymbolicValue value, ProgramState programState) =>
                HasNotInitializedIVConstraint(value, programState) || HasNotInitializedKeyConstraint(value, programState);

            private static bool HasNotInitializedIVConstraint(SymbolicValue value, ProgramState programState) =>
                programState.Constraints[value].HasConstraint(IVInitializationSymbolicValueConstraint.NotInitialized);

            private static bool HasNotInitializedKeyConstraint(SymbolicValue value, ProgramState programState) =>
                programState.Constraints[value].HasConstraint(KeyInitializationSymbolicValueConstraint.NotInitialized);

            private static bool IsCreateMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.IsMemberAccessOnKnownType("Create", vulnerableTypes, semanticModel);

            private static bool IsCreateEncryptorMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.IsMemberAccessOnKnownType("CreateEncryptor", vulnerableTypes, semanticModel);

            private static bool IsGenerateKeyMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.IsMemberAccessOnKnownType("GenerateKey", vulnerableTypes, semanticModel);

            private static bool IsGenerateIVMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.IsMemberAccessOnKnownType("GenerateIV", vulnerableTypes, semanticModel);

            private static bool IsRNGCryptoServiceProviderSanitizer(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                && (memberAccessExpressionSyntax.NameIs("GetBytes") || memberAccessExpressionSyntax.NameIs("GetNonZeroBytes"))
                && semanticModel.GetSymbolInfo(invocation).Symbol.ContainingType.Is(KnownType.System_Security_Cryptography_RNGCryptoServiceProvider);
        }
    }
}
