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

namespace SonarAnalyzer.SymbolicExecution.Sonar.Analyzers
{
    internal sealed class PublicMethodArgumentsShouldBeCheckedForNull : ISymbolicExecutionAnalyzer
    {
        private const string DiagnosticId = "S3900";
        private const string MessageFormat = "Refactor this {0}.";
        private const string Constructor = "constructor to avoid using members of parameter '{0}' because it could be null";
        private const string Method = "method to add validation of parameter '{0}' before using it";

        public static readonly DiagnosticDescriptor S3900 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(S3900);

        public ISymbolicExecutionAnalysisContext CreateContext(SonarSyntaxNodeReportingContext context, SonarExplodedGraph explodedGraph) =>
            new AnalysisContext(context, explodedGraph);

        private static void CollectMemberAccesses(MemberAccessingEventArgs args, ISet<IdentifierNameSyntax> identifiers, SemanticModel semanticModel)
        {
            if (args.Symbol is IParameterSymbol
                && !semanticModel.IsExtensionMethod(args.Identifier.Parent)
                && !args.Symbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState)
                && !args.Symbol.GetAttributes(KnownType.Microsoft_AspNetCore_Mvc_FromServicesAttribute).Any())
            {
                identifiers.Add(args.Identifier);
            }
        }

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            public bool SupportsPartialResults => true;

            private readonly SonarSyntaxNodeReportingContext context;
            private readonly HashSet<IdentifierNameSyntax> identifiers = new();
            private readonly NullPointerDereference.NullPointerCheck nullPointerCheck;

            public AnalysisContext(SonarSyntaxNodeReportingContext context, SonarExplodedGraph explodedGraph)
            {
                if (!GetMethodSymbol(context).IsPubliclyAccessible())
                {
                    return;
                }

                this.context = context;
                nullPointerCheck = explodedGraph.NullPointerCheck;
                nullPointerCheck.MemberAccessing += MemberAccessingHandler;
            }

            public IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation) =>
                identifiers.Select(identifier => Diagnostic.Create(S3900, identifier.GetLocation().EnsureMappedLocation(), GetMessage(identifier)));

            public void Dispose()
            {
                if (nullPointerCheck != null)
                {
                    nullPointerCheck.MemberAccessing -= MemberAccessingHandler;
                }
            }

            private void MemberAccessingHandler(object sender, MemberAccessingEventArgs args) =>
                CollectMemberAccesses(args, identifiers, context.SemanticModel);

            private static string GetMessage(SimpleNameSyntax identifier) =>
                IsArgumentOfConstructorInitializer(identifier)
                    ? string.Format(Constructor, identifier.Identifier.ValueText)
                    : string.Format(Method, identifier.Identifier.ValueText);

            private static bool IsArgumentOfConstructorInitializer(SyntaxNode identifier) =>
                identifier.FirstAncestorOrSelf<ConstructorInitializerSyntax>() != null;

            private static ISymbol GetMethodSymbol(SonarSyntaxNodeReportingContext context) =>
                context.SemanticModel.GetSymbolInfo(context.Node).Symbol ?? context.SemanticModel.GetDeclaredSymbol(context.Node);
        }
    }
}
