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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class DeclareTypesInNamespaces : DeclareTypesInNamespacesBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        protected override SyntaxToken GetTypeIdentifier(SyntaxNode declaration) =>
            ((TypeStatementSyntax)declaration).Identifier;

        protected override bool IsOuterTypeWithoutNamespace(SyntaxNode declaration, SemanticModel semanticModel)
        {
            if (declaration.Parent.Parent is TypeBlockSyntax || // Inner type
                declaration.Parent.Parent is NamespaceBlockSyntax) // Outer type within a namespace
            {
                return false;
            }

            // If we are an outer type without a declared namespace,
            // make sure there is no Root Namespace set in the project
            var typeSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(declaration);
            return typeSymbol != null && typeSymbol.ContainingNamespace.IsGlobalNamespace;
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                GetAnalysisAction(rule),
                SyntaxKind.ClassStatement,
                SyntaxKind.StructureStatement);
    }
}
