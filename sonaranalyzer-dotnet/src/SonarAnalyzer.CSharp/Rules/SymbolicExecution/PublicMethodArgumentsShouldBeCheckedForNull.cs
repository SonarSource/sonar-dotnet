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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using CSharpExplodedGraph = SonarAnalyzer.SymbolicExecution.CSharpExplodedGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    internal sealed class PublicMethodArgumentsShouldBeCheckedForNull : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S3900";
        private const string MessageFormat = "Refactor this {0}.";
        private const string Constructor = "constructor to avoid using members of parameter '{0}' because it could be null";
        private const string Method = "method to add validation of parameter '{0}' before using it";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph, context);

       private static void CollectMemberAccesses(MemberAccessingEventArgs args, ISet<IdentifierNameSyntax> identifiers,
            SemanticModel semanticModel)
        {
            if (args.Symbol is IParameterSymbol &&
                !NullPointerDereference.NullPointerCheck.IsExtensionMethod(args.Identifier.Parent, semanticModel) &&
                !args.Symbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState))
            {
                identifiers.Add(args.Identifier);
            }
        }

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly HashSet<IdentifierNameSyntax> identifiers = new HashSet<IdentifierNameSyntax>();
            private readonly NullPointerDereference.NullPointerCheck nullPointerCheck;
            private readonly SyntaxNodeAnalysisContext syntaxNodeAnalysisContext;

            public AnalysisContext(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
            {
                if (!GetMethodSymbol(context).IsPubliclyAccessible())
                {
                    return;
                }

                this.syntaxNodeAnalysisContext = context;

                this.nullPointerCheck = new NullPointerDereference.NullPointerCheck(explodedGraph);
                this.nullPointerCheck.MemberAccessing += MemberAccessingHandler;
                explodedGraph.AddExplodedGraphCheck(this.nullPointerCheck);
            }

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                this.identifiers.Select(identifier => Diagnostic.Create(rule, identifier.GetLocation(), GetMessage(identifier)));

            public void Dispose()
            {
                if (this.nullPointerCheck != null)
                {
                    this.nullPointerCheck.MemberAccessing -= MemberAccessingHandler;
                }
            }

            private void MemberAccessingHandler(object sender, MemberAccessingEventArgs args) =>
                CollectMemberAccesses(args, this.identifiers, this.syntaxNodeAnalysisContext.SemanticModel);

            private static string GetMessage(SimpleNameSyntax identifier) =>
                IsArgumentOfConstructorInitializer(identifier)
                    ? string.Format(Constructor, identifier.Identifier.ValueText)
                    : string.Format(Method, identifier.Identifier.ValueText);

            private static bool IsArgumentOfConstructorInitializer(SyntaxNode identifier) =>
                identifier.FirstAncestorOrSelf<ConstructorInitializerSyntax>() != null;

            private static ISymbol GetMethodSymbol(SyntaxNodeAnalysisContext context) =>
                context.SemanticModel.GetSymbolInfo(context.Node).Symbol ?? context.SemanticModel.GetDeclaredSymbol(context.Node);
        }
    }
}
