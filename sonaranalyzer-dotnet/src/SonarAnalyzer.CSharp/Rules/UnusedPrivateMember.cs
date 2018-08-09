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

        private ConcurrentBag<Diagnostic> unusedInternalMembers;

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    var containsInternalsVisibleToAttribute = false;
                    unusedInternalMembers = new ConcurrentBag<Diagnostic>();

                    c.RegisterSemanticModelAction(
                        cc =>
                        {
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

                            ReportIssues(cc, usageCollector.Usages, symbolCollector.RemovableSymbols,
                                usageCollector.EmptyConstructors, symbolCollector.FieldLikeSymbols);

                            ReportUnusedPropertyAccessors(cc, usageCollector.Usages, symbolCollector.RemovableSymbols,
                                usageCollector.PropertyAccess);

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
                               cc.ReportDiagnosticIfNonGenerated(diagnostic, cc.Compilation);
                           }
                       });
                });
        }

        private static bool IsInternalVisibleToAttribute(AttributeSyntax attribute, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructor &&
            attributeConstructor.ContainingType.Is(KnownType.System_Runtime_CompilerServices_InternalsVisibleToAttribute);

        private static void ReportUnusedPropertyAccessors(SymbolAnalysisContext context, HashSet<ISymbol> usedSymbols,
            HashSet<ISymbol> declaredPrivateSymbols,
            Dictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
        {
            var usedPrivatePropertis = declaredPrivateSymbols.Intersect(usedSymbols).OfType<IPropertySymbol>();
            var onlyOneAccessorAccessed = usedPrivatePropertis.Where(p => propertyAccessorAccess.ContainsKey(p));

            foreach (var property in onlyOneAccessorAccessed)
            {
                var access = propertyAccessorAccess[property];
                if (access == AccessorAccess.None || access == AccessorAccess.Both)
                {
                    continue;
                }

                if (access == AccessorAccess.Get && property.SetMethod != null)
                {
                    var accessorSyntax = GetAccessorSyntax(property.SetMethod);
                    if (accessorSyntax != null)
                    {
                        context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, accessorSyntax.GetLocation(),
                            "private", "set accessor in property", property.Name), context.Compilation);
                    }
                    continue;
                }

                if (access == AccessorAccess.Set && property.GetMethod != null)
                {
                    var accessorSyntax = GetAccessorSyntax(property.GetMethod);
                    if (accessorSyntax != null && accessorSyntax.Body != null)
                    {
                        context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, accessorSyntax.GetLocation(),
                            "private", "get accessor in property", property.Name), context.Compilation);
                    }
                }
            }

            AccessorDeclarationSyntax GetAccessorSyntax(IMethodSymbol methodSymbol) =>
                methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
        }

        private void ReportIssues(SymbolAnalysisContext context, HashSet<ISymbol> usedSymbols,
            HashSet<ISymbol> declaredPrivateSymbols, HashSet<ISymbol> emptyConstructors,
            BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var unusedSymbols = declaredPrivateSymbols
                .Except(usedSymbols.Union(emptyConstructors))
                .ToList();

            var alreadyReportedFieldLikeSymbols = new HashSet<ISymbol>();

            var unusedSymbolSyntaxPairs = unusedSymbols
                .SelectMany(unusedSymbol => unusedSymbol.DeclaringSyntaxReferences
                    .Select(r =>
                        new
                        {
                            Syntax = r.GetSyntax(),
                            Symbol = unusedSymbol
                        }));

            foreach (var unused in unusedSymbolSyntaxPairs)
            {
                var location = unused.Syntax.GetLocation();

                var canBeFieldLike = unused.Symbol is IFieldSymbol || unused.Symbol is IEventSymbol;
                if (canBeFieldLike)
                {
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
                        .Select(v => fieldLikeSymbols.GetByB(v))
                        .ToList();

                    if (declarations.All(d => unusedSymbols.Contains(d)))
                    {
                        location = unused.Syntax.Parent.Parent.GetLocation();
                        alreadyReportedFieldLikeSymbols.UnionWith(declarations);
                    }
                }

                var memberKind = GetMemberType(unused.Symbol);
                var memberName = GetMemberName(unused.Symbol);
                var effectiveAccessibility = unused.Symbol.GetEffectiveAccessibility();

                if (effectiveAccessibility == Accessibility.Internal)
                {
                    // We can't report internal members directly because they might be used through another assembly.
                    unusedInternalMembers.Add(
                        Diagnostic.Create(rule, location, "internal", memberKind, memberName));
                    continue;
                }

                if (effectiveAccessibility == Accessibility.Private)
                {
                    context.ReportDiagnosticIfNonGenerated(
                        Diagnostic.Create(rule, location, "private", memberKind, memberName), context.Compilation);
                }
            }

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

        ////private IEnumerable<Diagnostic> GetDiagnostics(HashSet<ISymbol> usedSymbols, HashSet<ISymbol> declaredPrivateSymbols)
        ////{
        ////    var unusedSymbols = declaredPrivateSymbols
        ////        .Except(usedSymbols)
        ////        .SelectMany(unused => unused.DeclaringSyntaxReferences.Select(r => unused.ToSymbolWithSyntax(r.GetSyntax())))
        ////        .Select(unused =>
        ////            Diagnostic.Create(rule, unused.Syntax.GetLocation(), "private", GetMemberKindString(unused.Symbol), GetMemberName(unused.Symbol)));

        ////    string GetMemberKindString(ISymbol symbol)
        ////    {
        ////        switch (symbol)
        ////        {
        ////            case IMethodSymbol method:
        ////                if (method.MethodKind == MethodKind.Constructor)
        ////                {
        ////                    return "constructor";
        ////                }
        ////                else if (method.MethodKind == MethodKind.PropertyGet)
        ////                {
        ////                    return "get accessor in property";
        ////                }
        ////                else if (method.MethodKind == MethodKind.PropertySet)
        ////                {
        ////                    return "set accessor in property";
        ////                }
        ////                else
        ////                {
        ////                    return "method";
        ////                }
        ////            case IEventSymbol @event:
        ////            case IFieldSymbol field:
        ////            case IPropertySymbol property:
        ////                return symbol.Kind.ToString().ToLowerInvariant();
        ////            case INamedTypeSymbol namedType:
        ////                return "type";
        ////            default:
        ////                return "member";
        ////        }
        ////    }

        ////    string GetMemberName(ISymbol symbol)
        ////    {
        ////        if (symbol is IMethodSymbol method)
        ////        {
        ////            if (method.MethodKind == MethodKind.Constructor)
        ////            {
        ////                return method.ContainingType.Name;
        ////            }
        ////            if (method.MethodKind == MethodKind.PropertyGet ||
        ////                method.MethodKind == MethodKind.PropertySet)
        ////            {
        ////                return method.AssociatedSymbol.Name;
        ////            }
        ////        }
        ////        return symbol.Name;
        ////    }

        ////    return unusedSymbols;
        ////}

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
