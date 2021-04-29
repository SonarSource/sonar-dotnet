/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Common.Checks;
using SonarAnalyzer.SymbolicExecution.Common.Constraints;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    internal sealed class HashesShouldHaveUnpredictableSalt : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S2053";
        private const string MessageFormat = "{0}";
        private const string MakeSaltUnpredictableMessage = "Make this salt unpredictable.";
        private const string MakeThisSaltLongerMessage = "Make this salt longer.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph);

        internal sealed class LocationContext
        {
            public Location Location { get; }

            public string Message { get; }

            public LocationContext(Location location, string message)
            {
                Location = location;
                Message = message;
            }

            public override int GetHashCode() =>
                Location.GetHashCode() * 397 ^ Message.GetHashCode();

            public override bool Equals(object obj) =>
                ReferenceEquals(this, obj) || (obj is LocationContext other && Equals(other));

            private bool Equals(LocationContext other) =>
                Equals(Location, other.Location) && Message == other.Message;
        }

        private sealed class AnalysisContext : DefaultAnalysisContext<LocationContext>
        {
            public AnalysisContext(AbstractExplodedGraph explodedGraph)
            {
                explodedGraph.AddExplodedGraphCheck(new ByteArrayCheck(explodedGraph));
                explodedGraph.AddExplodedGraphCheck(new SaltCheck(explodedGraph, this));
            }

            protected override Diagnostic CreateDiagnostic(LocationContext location) =>
                Diagnostic.Create(Rule, location.Location, location.Message);
        }

        private sealed class SaltCheck : ExplodedGraphCheck
        {
            private const int MinimumSafeLength = 16;
            private const int SaltParameterIndex = 2;

            private static readonly ImmutableArray<KnownType> VulnerableTypes =
                ImmutableArray.Create(KnownType.System_Security_Cryptography_PasswordDeriveBytes,
                                      KnownType.System_Security_Cryptography_Rfc2898DeriveBytes);

            private readonly AnalysisContext context;

            public SaltCheck(AbstractExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState) =>
                programPoint.CurrentInstruction is ObjectCreationExpressionSyntax objectCreation
                    ? ObjectCreationPreProcess(objectCreation, programState)
                    : programState;

            public override ProgramState PostProcessInstruction(ProgramPoint programPoint, ProgramState programState) =>
                programPoint.CurrentInstruction is ArrayCreationExpressionSyntax arrayCreation
                    ? ArrayCreationPostProcess(arrayCreation, programState)
                    : programState;

            private ProgramState ArrayCreationPostProcess(ArrayCreationExpressionSyntax arrayCreation, ProgramState programState)
            {
                if (programState.HasValue
                    && semanticModel.GetTypeInfo(arrayCreation).Type.Is(KnownType.System_Byte_Array)
                    && GetSize(arrayCreation) < MinimumSafeLength)
                {
                    programState = programState.SetConstraint(programState.PeekValue(), SaltSizeSymbolicValueConstraint.Short);
                }

                return programState;
            }

            private ProgramState ObjectCreationPreProcess(ObjectCreationExpressionSyntax objectCreation, ProgramState programState)
            {
                if (semanticModel.GetSymbolInfo(objectCreation).Symbol is {} symbol
                    && symbol.ContainingType is {} symbolType
                    && symbolType.IsAny(VulnerableTypes)
                    // There are cases when the symbol corresponding to the salt parameter does not exists (e.g. the byte[] is created directly when calling the ctor)
                    // but we should always have a symbolic value for it.
                    && programState.ExpressionStack.Skip(objectCreation.ArgumentList.Arguments.Count - SaltParameterIndex).FirstOrDefault() is {} symbolicValue)
                {
                    if (programState.HasConstraint(symbolicValue, SaltSizeSymbolicValueConstraint.Short))
                    {
                        context.AddLocation(new LocationContext(objectCreation.ArgumentList.Arguments[1].Expression.GetLocation(), MakeThisSaltLongerMessage));
                    }
                    else if (programState.HasConstraint(symbolicValue, ByteArraySymbolicValueConstraint.Constant))
                    {
                        context.AddLocation(new LocationContext(objectCreation.ArgumentList.Arguments[1].Expression.GetLocation(), MakeSaltUnpredictableMessage));
                    }
                }

                return programState;
            }

            private int? GetSize(ArrayCreationExpressionSyntax arrayCreation)
            {
                if (arrayCreation.Initializer != null)
                {
                    return arrayCreation.Initializer.Expressions.Count;
                }

                if (arrayCreation.Type.RankSpecifiers.Count == 1
                    && arrayCreation.Type.RankSpecifiers[0].Sizes[0] is {} rankSpecifierSize
                    && semanticModel.GetConstantValue(rankSpecifierSize) is {HasValue: true, Value: int size})
                {
                    return size;
                }

                return null;
            }
        }
    }
}
