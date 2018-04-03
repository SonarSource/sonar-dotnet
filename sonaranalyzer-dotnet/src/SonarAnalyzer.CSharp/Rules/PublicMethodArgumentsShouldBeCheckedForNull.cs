/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Constraints;
using CSharpExplodedGraph = SonarAnalyzer.SymbolicExecution.CSharpExplodedGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    public sealed class PublicMethodArgumentsShouldBeCheckedForNull : ISymbolicExecutionAnalyzerFactory
    {
        internal const string DiagnosticId = "S3900";
        private const string MessageFormat = "Refactor this method to add validation of parameter '{0}' before using it.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        ISymbolicExecutionAnalyzer ISymbolicExecutionAnalyzerFactory.Create(CSharpExplodedGraph explodedGraph) =>
            new SymbolicExecutionAnalyzer(explodedGraph);

        bool ISymbolicExecutionAnalyzerFactory.IsEnabled(SyntaxNodeAnalysisContext context)
        {
            return !context.IsTest() &&
                MethodIsPublic();

            bool MethodIsPublic()
            {
                var methodSymbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol
                    ?? context.SemanticModel.GetDeclaredSymbol(context.Node);
                return methodSymbol.IsPubliclyAccessible();
            };
        }

        private sealed class SymbolicExecutionAnalyzer : ISymbolicExecutionAnalyzer
        {
            private readonly HashSet<IdentifierNameSyntax> identifiers = new HashSet<IdentifierNameSyntax>();
            private readonly CSharpExplodedGraph explodedGraph;
            private readonly NullPointerDereference.NullPointerCheck check;

            public SymbolicExecutionAnalyzer(CSharpExplodedGraph explodedGraph)
            {
                this.explodedGraph = explodedGraph;
                check = explodedGraph.GetOrAddCheck(() => new NullPointerDereference.NullPointerCheck(explodedGraph));
                check.MemberAccessing += ObjectDisposedHandler;
            }

            public void Dispose()
            {
                check.MemberAccessing -= ObjectDisposedHandler;
            }

            public IEnumerable<Diagnostic> Diagnostics =>
                identifiers.Select(identifier => Diagnostic.Create(rule, identifier.GetLocation(), identifier.Identifier.ValueText));

            private void ObjectDisposedHandler(object sender, MemberAccessingEventArgs args)
            {
                if (args.Symbol is IParameterSymbol &&
                    !args.Identifier.Parent.IsExtensionMethod(explodedGraph.SemanticModel) &&
                    !args.Symbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState))
                {
                    identifiers.Add(args.Identifier);
                }
            }
        }
    }
}
