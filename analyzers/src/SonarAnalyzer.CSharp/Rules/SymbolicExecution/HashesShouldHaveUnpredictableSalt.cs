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

        private sealed class AnalysisContext : DefaultAnalysisContext<LocationContext>
        {
            public AnalysisContext(AbstractExplodedGraph explodedGraph)
            {
                explodedGraph.AddExplodedGraphCheck(new ByteArrayCheck(explodedGraph));
                explodedGraph.AddExplodedGraphCheck(new SaltCheck(explodedGraph, this));
            }

            protected override Diagnostic CreateDiagnostic(LocationContext locationContext) =>
                Diagnostic.Create(Rule, locationContext.Location, locationContext.Message);
        }

        private sealed class LocationContext
        {
            public Location Location { get; }

            public string Message { get; }

            public LocationContext(Location location, string message)
            {
                Location = location;
                Message = message;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Location != null ? Location.GetHashCode() : 0) * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                }
            }

            public override bool Equals(object obj) =>
                ReferenceEquals(this, obj) || (obj is LocationContext other && Equals(other));

            private bool Equals(LocationContext other) =>
                Equals(Location, other.Location) && Message == other.Message;
        }

        private sealed class SaltCheck : ExplodedGraphCheck
        {
            private static readonly ImmutableArray<KnownType> VulnerableTypes =
                ImmutableArray.Create(KnownType.System_Security_Cryptography_PasswordDeriveBytes,
                                      KnownType.System_Security_Cryptography_Rfc2898DeriveBytes);

            private readonly AnalysisContext context;

            public SaltCheck(AbstractExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;

            public override ProgramState PostProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                programState = programPoint.CurrentInstruction switch
                {
                    ArrayCreationExpressionSyntax arrayCreation => ArrayCreationPostProcess(arrayCreation, programState),
                    ObjectCreationExpressionSyntax objectCreation => ObjectCreationPostProcess(objectCreation, programState),
                    _ => programState
                };

                return base.PostProcessInstruction(programPoint, programState);
            }

            private ProgramState ArrayCreationPostProcess(ArrayCreationExpressionSyntax arrayCreation, ProgramState programState)
            {
                if (programState.HasValue && semanticModel.GetTypeInfo(arrayCreation).Type.Is(KnownType.System_Byte_Array))
                {
                    var size = GetSize(arrayCreation);
                    if (size.HasValue && size.Value < 32)
                    {
                        programState = programState.SetConstraint(programState.PeekValue(), SaltSizeSymbolicValueConstraint.Short);
                    }
                }

                return programState;
            }

            private ProgramState ObjectCreationPostProcess(ObjectCreationExpressionSyntax objectCreation, ProgramState programState)
            {
                if (semanticModel.GetSymbolInfo(objectCreation).Symbol is {} symbol
                    && symbol.ContainingType is {} symbolType
                    && symbolType.IsAny(VulnerableTypes)
                    && semanticModel.GetSymbolInfo(objectCreation.ArgumentList.Arguments[1].Expression).Symbol is {} saltSymbol
                    && programState.GetSymbolValue(saltSymbol) is {} symbolicValue)
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

            private Optional<int> GetSize(ArrayCreationExpressionSyntax arrayCreation) =>
                arrayCreation.Type.RankSpecifiers is {} rankSpecifiers &&
                rankSpecifiers.Count == 1 &&
                rankSpecifiers[0].Sizes[0] is {} rankSpecifier &&
                rankSpecifier.IsKind(SyntaxKind.NumericLiteralExpression) &&
                semanticModel.GetConstantValue(rankSpecifier) is {} constantValue &&
                constantValue.HasValue &&
                constantValue.Value is int size
                    ? new Optional<int>(size)
                    : new Optional<int>();
        }
    }
}

