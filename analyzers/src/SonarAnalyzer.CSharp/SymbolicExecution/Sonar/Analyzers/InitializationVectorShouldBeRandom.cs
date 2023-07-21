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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar.Checks;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Sonar.Analyzers
{
    internal sealed class InitializationVectorShouldBeRandom : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S3329";
        private const string MessageFormat = "Use a dynamically-generated, random IV.";

        internal static readonly DiagnosticDescriptor S3329 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(S3329);

        public ISymbolicExecutionAnalysisContext CreateContext(SonarSyntaxNodeReportingContext context, SonarExplodedGraph explodedGraph) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : DefaultAnalysisContext<Location>
        {
            public AnalysisContext(AbstractExplodedGraph explodedGraph)
            {
                explodedGraph.AddExplodedGraphCheck(new ByteArrayCheck(explodedGraph));
                explodedGraph.AddExplodedGraphCheck(new InitializationVectorCheck(explodedGraph, this));
            }

            protected override Diagnostic CreateDiagnostic(Location location) =>
                Diagnostic.Create(S3329, location.EnsureMappedLocation());
        }

        private sealed class InitializationVectorCheck : ExplodedGraphCheck
        {
            private readonly AnalysisContext context;

            public InitializationVectorCheck(AbstractExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState) =>
                programPoint.CurrentInstruction is InvocationExpressionSyntax invocation
                    ? InvocationExpressionPreProcess(invocation, programState)
                    : programState;

            public override ProgramState PostProcessInstruction(ProgramPoint programPoint, ProgramState programState) =>
                programPoint.CurrentInstruction switch
                {
                    InvocationExpressionSyntax invocation => InvocationExpressionPostProcess(invocation, programState),
                    AssignmentExpressionSyntax assignment => AssignmentExpressionPostProcess(assignment, programState),
                    _ => programState
                };

            private ProgramState InvocationExpressionPostProcess(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                if (IsSymmetricAlgorithmGenerateIVMethod(invocation, semanticModel)
                    && GetSymbolicValue(invocation, programState) is { } ivSymbolicValue)
                {
                    programState = programState.SetConstraint(ivSymbolicValue, InitializationVectorConstraint.Initialized);
                }

                return programState;
            }

            private ProgramState AssignmentExpressionPostProcess(AssignmentExpressionSyntax assignment, ProgramState programState) =>
                assignment.Left is MemberAccessExpressionSyntax memberAccess
                && IsSymmetricAlgorithmIVMemberAccess(memberAccess)
                && GetSymbolicValue(memberAccess.Expression, programState) is { } leftSymbolicValue
                && GetSymbolicValue(assignment.Right, programState) is { } rightSymbolicValue
                && programState.HasConstraint(rightSymbolicValue, ByteCollectionConstraint.CryptographicallyWeak)
                    ? programState.SetConstraint(leftSymbolicValue, InitializationVectorConstraint.NotInitialized)
                    : programState;

            private ProgramState InvocationExpressionPreProcess(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                if (IsSymmetricAlgorithmCreateEncryptorMethod(invocation, semanticModel))
                {
                    if (invocation.ArgumentList.Arguments.Count == 0)
                    {
                        var symbolicValue = GetSymbolicValue(invocation, programState);
                        if (symbolicValue != null && HasNotInitializedIVConstraint(symbolicValue, programState))
                        {
                            context.AddLocation(invocation.GetLocation());
                        }
                    }
                    else if (IsInvalidIV(invocation.ArgumentList.Arguments[1], programState))
                    {
                        context.AddLocation(invocation.GetLocation());
                    }
                }

                return programState;
            }

            private bool IsInvalidIV(ArgumentSyntax argument, ProgramState programState) =>
                argument.Expression switch
                {
                    MemberAccessExpressionSyntax memberAccess => memberAccess.NameIs("IV")
                                                                 && programState.GetSymbolValue(semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol) is { } symbolicValue
                                                                 && HasNotInitializedIVConstraint(symbolicValue, programState),
                    IdentifierNameSyntax identifier => programState.GetSymbolValue(semanticModel.GetSymbolInfo(identifier).Symbol) is { } symbolicValue
                                                       && programState.HasConstraint(symbolicValue, ByteCollectionConstraint.CryptographicallyWeak),
                    ArrayCreationExpressionSyntax _ => true,
                    _ => false
                };

            private SymbolicValue GetSymbolicValue(InvocationExpressionSyntax invocation, ProgramState programState) =>
                GetSymbolicValue(((MemberAccessExpressionSyntax)invocation.Expression).Expression, programState);

            private SymbolicValue GetSymbolicValue(ExpressionSyntax expression, ProgramState programState) =>
                semanticModel.GetSymbolInfo(expression).Symbol is { } symbol
                && programState.GetSymbolValue(symbol) is { } symbolicValue
                    ? symbolicValue
                    : null;

            private bool IsSymmetricAlgorithmIVMemberAccess(MemberAccessExpressionSyntax memberAccess) =>
                memberAccess.IsMemberAccessOnKnownType("IV", KnownType.System_Security_Cryptography_SymmetricAlgorithm, semanticModel);

            private static bool HasNotInitializedIVConstraint(SymbolicValue value, ProgramState programState) =>
                programState.Constraints[value].HasConstraint(InitializationVectorConstraint.NotInitialized);

            private static bool IsSymmetricAlgorithmCreateEncryptorMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.IsMemberAccessOnKnownType("CreateEncryptor", KnownType.System_Security_Cryptography_SymmetricAlgorithm, semanticModel);

            private static bool IsSymmetricAlgorithmGenerateIVMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                invocation.IsMemberAccessOnKnownType("GenerateIV", KnownType.System_Security_Cryptography_SymmetricAlgorithm, semanticModel);
        }
    }
}
