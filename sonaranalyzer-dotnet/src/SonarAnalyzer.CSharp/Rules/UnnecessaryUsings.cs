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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnnecessaryUsings : SonarDiagnosticAnalyzer
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
                    var globalUsingDirectives = simpleNamespaces.Select(x => new EquivalentNameSyntax(x.Name)).ToImmutableHashSet();

                    var visitor = new CSharpRemovableUsingWalker(c, globalUsingDirectives, null);
                    foreach (var member in compilationUnit.Members)
                    {
                        visitor.SafeVisit(member);
                    }
                    foreach (var attribute in compilationUnit.AttributeLists)
                    {
                        visitor.SafeVisit(attribute);
                    }

                    CheckDuplicateUsings(c, ImmutableHashSet.Create<EquivalentNameSyntax>(), simpleNamespaces);
                    CheckUnnecessaryUsings(c, simpleNamespaces, visitor.necessaryNamespaces);
                },
                SyntaxKind.CompilationUnit);

        }

        private static void CheckDuplicateUsings(SyntaxNodeAnalysisContext context, IImmutableSet<EquivalentNameSyntax> ancestorsUsingDirectives, IEnumerable<UsingDirectiveSyntax> usingDirectives)
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

        private static void CheckUnnecessaryUsings(SyntaxNodeAnalysisContext context, IEnumerable<UsingDirectiveSyntax> usingDirectives, HashSet<INamespaceSymbol> necessaryNamespaces)
        {
            foreach (var usingDirective in usingDirectives)
            {
                if (context.SemanticModel.GetSymbolInfo(usingDirective.Name).Symbol is INamespaceSymbol namespaceSymbol
                && !necessaryNamespaces.Any(usedNamespace => usedNamespace.IsSameNamespace(namespaceSymbol)))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, usingDirective.GetLocation(), "unnecessary"));
                }
            }
        }

        private class CSharpRemovableUsingWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext context;
            private readonly IImmutableSet<EquivalentNameSyntax> usingDirectivesFromParent;
            private readonly INamespaceSymbol currentNamespace;
            public readonly HashSet<INamespaceSymbol> necessaryNamespaces;

            public CSharpRemovableUsingWalker(SyntaxNodeAnalysisContext context, IImmutableSet<EquivalentNameSyntax> usingDirectives, INamespaceSymbol currentNamespace)
            {
                this.context = context;
                this.usingDirectivesFromParent = usingDirectives;
                this.necessaryNamespaces = new HashSet<INamespaceSymbol>();
                this.currentNamespace = currentNamespace;
            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                var simpleNamespaces = node.Usings.Where(usingDirective => usingDirective.Alias == null);
                var newUsingDirectives = new HashSet<EquivalentNameSyntax>();
                newUsingDirectives.UnionWith(usingDirectivesFromParent);
                newUsingDirectives.UnionWith(simpleNamespaces.Select(x => new EquivalentNameSyntax(x.Name)));

                // We visit the namespace declaration with the updated set of parent 'usings', this is needed in case of nested namespaces
                var visitingNamespace = context.SemanticModel.GetSymbolInfo(node.Name).Symbol as INamespaceSymbol;
                var visitor = new CSharpRemovableUsingWalker(context, newUsingDirectives.ToImmutableHashSet(), visitingNamespace);
                foreach (var member in node.Members)
                {
                    visitor.SafeVisit(member);
                }

                CheckDuplicateUsings(context, usingDirectivesFromParent, simpleNamespaces);
                CheckUnnecessaryUsings(context, simpleNamespaces, visitor.necessaryNamespaces);

                necessaryNamespaces.UnionWith(visitor.necessaryNamespaces);
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

            /// <summary>
            /// We check the symbol of each name node found in the code. If the containing namespace of the symbol is
            /// neither the current namespace or one of its parent, it is then added to the necessary namespace set, as
            /// importing that namespace is indeed necessary.
            /// </summary>
            private void VisitNameNode(SimpleNameSyntax node)
            {
                if (context.SemanticModel.GetSymbolInfo(node).Symbol is ISymbol symbol
                    && symbol.ContainingNamespace is INamespaceSymbol namespaceSymbol
                    && (currentNamespace == null || !namespaceSymbol.IsSameOrAncestorOf(currentNamespace)))
                {
                    necessaryNamespaces.Add(namespaceSymbol);
                }
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
