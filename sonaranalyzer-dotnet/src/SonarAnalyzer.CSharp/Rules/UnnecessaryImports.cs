/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using System;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnnecessaryImports : SonarDiagnosticAnalyzer
    {

        internal const string DiagnosticId = "S1128";
        private const string MessageFormat = "Remove this {0} 'using'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var compilationUnit = (CompilationUnitSyntax)c.Node;
                    var simpleNamespaces = compilationUnit.Usings.Where(usingDirective => usingDirective.Alias == null);
                    var globalUsingDirectives = new HashSet<EquivalentNameSyntax>();
                    globalUsingDirectives.UnionWith(simpleNamespaces.Select(x => new EquivalentNameSyntax(x.Name)));

                    var visitor = new CSharpRemovableUsingWalker(c, globalUsingDirectives, null);
                    foreach (var member in compilationUnit.Members)
                    {
                        visitor.SafeVisit(member);
                    }
                    foreach (var attribute in compilationUnit.AttributeLists)
                    {
                        visitor.SafeVisit(attribute);
                    }

                    CheckDuplicateUsings(c, new HashSet<EquivalentNameSyntax>(), simpleNamespaces);
                    CheckUnnecessaryUsings(c, simpleNamespaces, visitor.usedNamespaces);
                },
                SyntaxKind.CompilationUnit);

        }

        private static void CheckDuplicateUsings(SyntaxNodeAnalysisContext context, HashSet<EquivalentNameSyntax> ancestorsUsingDirectives, IEnumerable<UsingDirectiveSyntax> usingDirectives)
        {
            var groupingDirectives = usingDirectives
                .GroupBy(usingDirective => new EquivalentNameSyntax(usingDirective.Name))
                .ToList();

            foreach (var potentialDuplicate in groupingDirectives)
            {
                var duplicates = potentialDuplicate.Skip(1);
                foreach (var duplicate in duplicates)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, duplicate.GetLocation(), "duplicate"));
                }

                if (ancestorsUsingDirectives.Contains(potentialDuplicate.Key))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, potentialDuplicate.First().GetLocation(), "duplicate"));
                }
            }
        }

        private static void CheckUnnecessaryUsings(SyntaxNodeAnalysisContext context, IEnumerable<UsingDirectiveSyntax> usingDirectives, HashSet<INamespaceSymbol> usedNamespaces)
        {
            foreach (var usingDirective in usingDirectives)
            {
                if (context.SemanticModel.GetSymbolInfo(usingDirective.Name).Symbol is INamespaceSymbol namespaceSymbol
                && !usedNamespaces.Any(usedNamespace => IsSameNamespace(usedNamespace, namespaceSymbol)))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, usingDirective.GetLocation(), "unnecessary"));
                }
            }
        }

        private static bool IsSameNamespace(INamespaceSymbol namespace1, INamespaceSymbol namespace2)
        {
            return namespace1.IsGlobalNamespace && namespace2.IsGlobalNamespace
                || (namespace1.Name.Equals(namespace2.Name)
                && namespace1.ContainingNamespace != null
                && namespace2.ContainingNamespace != null
                && IsSameNamespace(namespace1.ContainingNamespace, namespace2.ContainingNamespace));
        }

        private class CSharpRemovableUsingWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext context;
            private readonly HashSet<EquivalentNameSyntax> usingDirectivesFromParent;
            private readonly INamespaceSymbol currentNamespace;
            public readonly HashSet<INamespaceSymbol> usedNamespaces;

            public CSharpRemovableUsingWalker(SyntaxNodeAnalysisContext context, HashSet<EquivalentNameSyntax> usingDirectives, INamespaceSymbol currentNamespace)
            {
                this.context = context;
                this.usingDirectivesFromParent = usingDirectives;
                this.usedNamespaces = new HashSet<INamespaceSymbol>();
                this.currentNamespace = currentNamespace;
            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                var simpleNamespaces = node.Usings.Where(usingDirective => usingDirective.Alias == null);
                var newUsingDirectives = new HashSet<EquivalentNameSyntax>();
                newUsingDirectives.UnionWith(usingDirectivesFromParent);
                newUsingDirectives.UnionWith(simpleNamespaces.Select(x => new EquivalentNameSyntax(x.Name)));

                var visitingNamespace = context.SemanticModel.GetSymbolInfo(node.Name).Symbol as INamespaceSymbol;
                var visitor = new CSharpRemovableUsingWalker(context, newUsingDirectives, visitingNamespace);
                foreach (var member in node.Members)
                {
                    visitor.SafeVisit(member);
                }

                CheckDuplicateUsings(context, usingDirectivesFromParent, simpleNamespaces);
                CheckUnnecessaryUsings(context, simpleNamespaces, visitor.usedNamespaces);

                usedNamespaces.UnionWith(visitor.usedNamespaces);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                VisitNameNode(node);
            }

            public override void VisitGenericName(GenericNameSyntax node)
            {
                VisitNameNode(node);
                base.VisitGenericName(node);
            }

            private void VisitNameNode(SimpleNameSyntax node)
            {
                if (context.SemanticModel.GetSymbolInfo(node).Symbol is ISymbol symbol
                    && symbol.ContainingNamespace is INamespaceSymbol namespaceSymbol
                    && (currentNamespace == null || !IsSelfOrAncestorNamespace(namespaceSymbol, currentNamespace)))
                {
                    usedNamespaces.Add(namespaceSymbol);
                }
            }


            private bool IsSelfOrAncestorNamespace(INamespaceSymbol namespaceSymbol, INamespaceSymbol currentNamespace)
            {
                return IsSameNamespace(namespaceSymbol, currentNamespace)
                    || (currentNamespace.ContainingNamespace != null && IsSelfOrAncestorNamespace(namespaceSymbol, currentNamespace.ContainingNamespace));
            }
        }
    }

    internal sealed class EquivalentNameSyntax : IEquatable<EquivalentNameSyntax>
    {
        private readonly NameSyntax name;

        public EquivalentNameSyntax(NameSyntax name)
        {
            this.name = name;
        }

        public override int GetHashCode()
        {
            return name.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is EquivalentNameSyntax equivalentName
                && Equals(equivalentName);
        }

        public bool Equals(EquivalentNameSyntax other)
        {
            return other != null && CSharpEquivalenceChecker.AreEquivalent(this.name, other.name);
        }
    }
}
