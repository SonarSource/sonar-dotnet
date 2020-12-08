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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    internal sealed class HashesShouldHaveUnpredictableSalt : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S2053";
        private const string MessageFormat = "{0}";
        private const string AddSaltMessage = "Add an unpredictable salt value to this hash.";
        private const string MakeSaltUnpredictableMessage = "Make this salt unpredictable.";
        private const string MakeThisSaltLongerMessage = "Make this salt longer.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly List<Location> locations = new List<Location>();

            public bool SupportsPartialResults => false;

            public AnalysisContext(AbstractExplodedGraph explodedGraph) =>
                explodedGraph.AddExplodedGraphCheck(new SaltCheck(explodedGraph, this));

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                locations.Distinct().Select(location => Diagnostic.Create(Rule, location));


            public void AddLocation(Location location) => locations.Add(location);

            public void Dispose()
            {
                // Nothing to do here.
            }
        }

        private sealed class SaltCheck : ExplodedGraphCheck
        {
            private readonly AnalysisContext context;

            public SaltCheck(AbstractExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;
        }
    }
}

