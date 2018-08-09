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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class UnusedPrivateMember : SonarDiagnosticAnalyzer
    {
        [Flags]
        public enum AccessorAccess
        {
            None = 0,
            Get = 1,
            Set = 2,
            Both = Get | Set
        }

        internal const string DiagnosticId = "S1144";
        private const string MessageFormat = "Remove the unused {0} {1} '{2}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    var containsInternalsVisibleToAttribute = false;
                    var unusedInternalMembers = new ConcurrentBag<Diagnostic>();

                    c.RegisterSemanticModelAction(
                        cc =>
                        {
                            if (containsInternalsVisibleToAttribute)
                            {
                                return;
                            }

                            containsInternalsVisibleToAttribute = cc.SemanticModel.SyntaxTree.GetRoot()
                                .DescendantNodes()
                                .OfType<AttributeListSyntax>()
                                .SelectMany(list => list.Attributes)
                                .Any(a => IsInternalVisibleToAttribute(a, cc.SemanticModel));
                        });

                    c.RegisterSymbolAction(
                        cc =>
                        {
                            var namedType = (INamedTypeSymbol)cc.Symbol;
                            if (!namedType.IsClassOrStruct() ||
                                namedType.ContainingType != null)
                            {
                                return;
                            }

                            var symbolCollector = new RemovableSymbolCollector(GetSemanticModel);
                            namedType.DeclaringSyntaxReferences
                                .Select(r => r.GetSyntax())
                                .ToList()
                                .ForEach(symbolCollector.Visit);

                            var removableSymbolNames = symbolCollector.RemovableSymbols.Select(s => s.Name).ToHashSet();

                            var usageCollector = new SymbolUsageCollector(GetSemanticModel, removableSymbolNames);
                            namedType.DeclaringSyntaxReferences
                                .Select(r => r.GetSyntax())
                                .ToList()
                                .ForEach(usageCollector.Visit);

                            var unusedSymbolsByAccessibility = symbolCollector.RemovableSymbols
                                .Except(usageCollector.UsedSymbols)
                                .ToLookup(symbol => symbol.GetEffectiveAccessibility(), symbol => symbol);

                            var unusedInternalSymbols = unusedSymbolsByAccessibility[Accessibility.Internal].ToHashSet();
                            var unusedPrivateSymbols = unusedSymbolsByAccessibility[Accessibility.Private].ToHashSet();

                            var onlyOneAccessorAccessed = symbolCollector.RemovableSymbols
                                .Intersect(usageCollector.UsedSymbols)
                                .OfType<IPropertySymbol>()
                                .Where(p => usageCollector.PropertyAccess.ContainsKey(p));

                            // Internals are reported when the compilation ends
                            GetDiagnostics(unusedInternalSymbols, Accessibility.Internal, symbolCollector.FieldLikeSymbols)
                                .ToList()
                                .ForEach(unusedInternalMembers.Add);

                            // TODO: should we report with ReportWhenActive?
                            GetDiagnostics(unusedPrivateSymbols, Accessibility.Private, symbolCollector.FieldLikeSymbols)
                                .ToList()
                                .ForEach(d => cc.ReportDiagnosticIfNonGenerated(d));

                            GetDiagnosticsForPropertyAccessors(onlyOneAccessorAccessed, usageCollector.PropertyAccess)
                                .ToList()
                                .ForEach(d => cc.ReportDiagnosticIfNonGenerated(d));

                            SemanticModel GetSemanticModel(SyntaxNode node) =>
                                c.Compilation.GetSemanticModel(node.SyntaxTree);
                        },
                        SymbolKind.NamedType);

                    c.RegisterCompilationEndAction(
                       cc =>
                       {
                           if (containsInternalsVisibleToAttribute)
                           {
                               return;
                           }

                           foreach (var diagnostic in unusedInternalMembers)
                           {
                               cc.ReportDiagnosticIfNonGenerated(diagnostic);
                           }
                       });
                });
        }

        private static bool IsInternalVisibleToAttribute(AttributeSyntax attribute, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructor &&
            attributeConstructor.ContainingType.Is(KnownType.System_Runtime_CompilerServices_InternalsVisibleToAttribute);

        private static List<Diagnostic> GetDiagnosticsForPropertyAccessors(IEnumerable<IPropertySymbol> onlyOneAccessorAccessed,
            Dictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
        {
            return onlyOneAccessorAccessed.SelectMany(GetDiagnosticsForProperty).ToList();

            IEnumerable<Diagnostic> GetDiagnosticsForProperty(IPropertySymbol property)
            {
                var access = propertyAccessorAccess[property];
                if (access == AccessorAccess.Get && property.SetMethod != null)
                {
                    var accessorSyntax = GetAccessorSyntax(property.SetMethod);
                    if (accessorSyntax != null)
                    {
                        yield return Diagnostic.Create(rule, accessorSyntax.GetLocation(), "private",
                            "set accessor in property", property.Name);
                    }
                }
                else if (access == AccessorAccess.Set && property.GetMethod != null)
                {
                    var accessorSyntax = GetAccessorSyntax(property.GetMethod);
                    if (accessorSyntax != null && accessorSyntax.Body != null)
                    {
                        yield return Diagnostic.Create(rule, accessorSyntax.GetLocation(), "private",
                            "get accessor in property", property.Name);
                    }
                }
            }

            AccessorDeclarationSyntax GetAccessorSyntax(IMethodSymbol methodSymbol) =>
                methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
        }

        private List<Diagnostic> GetDiagnostics(HashSet<ISymbol> unusedSymbols, Accessibility accessibility,
            BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var alreadyReportedFieldLikeSymbols = new HashSet<ISymbol>();

            var unusedSymbolSyntaxPairs = unusedSymbols.SelectMany(symbol =>
                symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax().ToSyntaxWithSymbol(symbol)));

            var diagnostics = new List<Diagnostic>();

            foreach (var unused in unusedSymbolSyntaxPairs)
            {
                var unusedSyntax = unused.Symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax());

                var location = unused.Syntax.GetLocation();

                var fieldOrEvent = unused.Symbol is IFieldSymbol || unused.Symbol is IEventSymbol;
                if (fieldOrEvent)
                {
                    // Report fields and events only once per declaration
                    if (alreadyReportedFieldLikeSymbols.Contains(unused.Symbol))
                    {
                        continue;
                    }

                    var variableDeclaration = GetVariableDeclaration(unused.Syntax);
                    if (variableDeclaration == null)
                    {
                        continue;
                    }

                    var declarations = variableDeclaration.Variables
                        .Select(fieldLikeSymbols.GetByB)
                        .ToList();

                    if (declarations.All(unusedSymbols.Contains))
                    {
                        location = unused.Syntax.Parent.Parent.GetLocation();
                        alreadyReportedFieldLikeSymbols.UnionWith(declarations);
                    }
                }

                var memberKind = GetMemberType(unused.Symbol);
                var memberName = GetMemberName(unused.Symbol);

                diagnostics.Add(Diagnostic.Create(rule, location, accessibility.ToString().ToLowerInvariant(), memberKind, memberName));
            }

            return diagnostics;

            VariableDeclarationSyntax GetVariableDeclaration(SyntaxNode syntax)
            {
                if (syntax.Parent.Parent is FieldDeclarationSyntax fieldDeclaration)
                {
                    return fieldDeclaration.Declaration;
                }

                var eventFieldDeclaration = syntax.Parent.Parent as EventFieldDeclarationSyntax;
                return eventFieldDeclaration?.Declaration;
            }
        }

        private static string GetMemberType(ISymbol symbol)
        {
            if (symbol.IsConstructor())
            {
                return "constructor";
            }

            switch (symbol.Kind)
            {
                case SymbolKind.Event:
                case SymbolKind.Field:
                case SymbolKind.Method:
                case SymbolKind.Property:
                    return symbol.Kind.ToString().ToLowerInvariant();
                case SymbolKind.NamedType:
                    return "type";
                default:
                    return "member";
            }
        }

        private static string GetMemberName(ISymbol symbol)
        {
            return symbol.IsConstructor() ? symbol.ContainingType.Name : symbol.Name;
        }
    }
}
