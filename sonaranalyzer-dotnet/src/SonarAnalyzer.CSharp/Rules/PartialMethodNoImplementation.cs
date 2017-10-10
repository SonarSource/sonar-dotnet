/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class PartialMethodNoImplementation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3251";
        private const string MessageFormat = "Supply an implementation for {0} partial method{1}.";
        internal const string MessageAdditional = ", otherwise this call will be ignored";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForCandidatePartialInvocation(c),
                SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForCandidatePartialDeclaration(c),
                SyntaxKind.MethodDeclaration);
        }

        private static void CheckForCandidatePartialDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (MethodDeclarationSyntax)context.Node;
            var partialKeyword = declaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.PartialKeyword));
            if (declaration.Body != null ||
                partialKeyword == default(SyntaxToken))
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(declaration);
            if (methodSymbol != null &&
                methodSymbol.PartialImplementationPart == null)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, partialKeyword.GetLocation(), "this", string.Empty));
            }
        }

        private static void CheckForCandidatePartialInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            // from the method symbol it's not possible to tell if it's a partial method or not.
            // https://github.com/dotnet/roslyn/issues/48

            var partialDeclarations = methodSymbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Body == null && method.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

            if (methodSymbol.PartialImplementationPart != null ||
                !partialDeclarations.Any())
            {
                return;
            }

            var statement = invocation.Parent as StatementSyntax;
            if (statement == null)
            {
                return;
            }
            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, statement.GetLocation(), "the", MessageAdditional));
        }
    }
}

