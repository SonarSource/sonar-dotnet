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
    public sealed class UnusedPrivateMember : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1144";
        private const string MessageFormat = "Remove the unused {0} {1} '{2}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> IgnoredTypes = new HashSet<KnownType>
        {
            KnownType.UnityEditor_AssetModificationProcessor,
            KnownType.UnityEditor_AssetPostprocessor,
            KnownType.UnityEngine_MonoBehaviour,
            KnownType.UnityEngine_ScriptableObject,
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    // Collect potentially removable internal types from the project to evaluate when
                    // the compilation is over, depending on whether InternalsVisibleTo attribute is present
                    // or not.
                    var removableInternalTypes = new ConcurrentBag<ISymbol>();
                    // Collect here all named types from the project to look for internal member usages.
                    var allNamedTypes = new ConcurrentBag<INamedTypeSymbol>();

                    c.RegisterSymbolAction(
                        cc =>
                        {
                            var namedType = (INamedTypeSymbol)cc.Symbol;
                            if (!namedType.IsClassOrStruct() ||
                                namedType.ContainingType != null ||
                                namedType.DerivesFromAny(IgnoredTypes))
                            {
                                return;
                            }

                            // Collect all symbols to try to look for used internal members when the compilation ends
                            allNamedTypes.Add(namedType);

                            // Collect symbols of private members that could potentially be removed
                            var removableSymbolsCollector = new RemovableSymbolCollector(c.Compilation.GetSemanticModel);

                            VisitDeclaringReferences(namedType, removableSymbolsCollector);

                            // Keep the removable internal types for when the compilation ends
                            foreach (var internalSymbol in removableSymbolsCollector.InternalSymbols.OfType<INamedTypeSymbol>())
                            {
                                removableInternalTypes.Add(internalSymbol);
                            }

                            var usageCollector = new SymbolUsageCollector(
                                c.Compilation.GetSemanticModel,
                                removableSymbolsCollector.PrivateSymbols.Select(s => s.Name).ToHashSet());

                            VisitDeclaringReferences(namedType, usageCollector);

                            var diagnostics = GetDiagnostics(usageCollector, removableSymbolsCollector.PrivateSymbols, "private",
                                removableSymbolsCollector.FieldLikeSymbols);
                            foreach (var diagnostic in diagnostics)
                            {
                                cc.ReportDiagnosticIfNonGenerated(diagnostic, cc.Compilation);
                            }
                        },
                        SymbolKind.NamedType);

                    c.RegisterCompilationEndAction(
                        cc =>
                        {
                            var foundInternalsVisibleTo = cc.Compilation.Assembly
                                .GetAttributes(KnownType.System_Runtime_CompilerServices_InternalsVisibleToAttribute)
                                .Any();

                            if (foundInternalsVisibleTo ||
                                removableInternalTypes.Count == 0)
                            {
                                return;
                            }

                            var usageCollector = new SymbolUsageCollector(
                                c.Compilation.GetSemanticModel,
                                removableInternalTypes.Select(s => s.Name).ToHashSet());

                            foreach (var symbol in allNamedTypes)
                            {
                                VisitDeclaringReferences(symbol, usageCollector);
                            }

                            var diagnostics = GetDiagnostics(usageCollector, removableInternalTypes.ToHashSet(), "internal",
                                new BidirectionalDictionary<ISymbol, SyntaxNode>());
                            foreach (var diagnostic in diagnostics)
                            {
                                cc.ReportDiagnosticIfNonGenerated(diagnostic, cc.Compilation);
                            }
                        });
                });
        }

        private static IEnumerable<Diagnostic> GetDiagnostics(SymbolUsageCollector usageCollector, ISet<ISymbol> removableSymbols,
            string accessibility, BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var unusedSymbols = removableSymbols
                .Except(usageCollector.UsedSymbols)
                .Where(symbol => !MentionedInDebuggerDisplay(symbol))
                .ToHashSet();

            var propertiesWithUnusedAccessor = removableSymbols
                .Intersect(usageCollector.UsedSymbols)
                .OfType<IPropertySymbol>()
                .Where(usageCollector.PropertyAccess.ContainsKey)
                .Where(symbol => !MentionedInDebuggerDisplay(symbol));

            return GetDiagnosticsForMembers(unusedSymbols, accessibility, fieldLikeSymbols)
                .Concat(propertiesWithUnusedAccessor.SelectMany(propertySymbol => GetDiagnosticsForProperty(propertySymbol, usageCollector.PropertyAccess)));

            bool MentionedInDebuggerDisplay(ISymbol symbol) =>
                usageCollector.DebuggerDisplayValues.Any(value => value.Contains(symbol.Name));
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsForMembers(HashSet<ISymbol> unusedSymbols, string accessibility,
            BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var alreadyReportedFieldLikeSymbols = new HashSet<ISymbol>();

            var unusedSymbolSyntaxPairs = unusedSymbols
                .SelectMany(symbol => symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax().ToSyntaxWithSymbol(symbol)));

            var diagnostics = new List<Diagnostic>();

            foreach (var unused in unusedSymbolSyntaxPairs)
            {
                var syntaxForLocation = unused.Syntax;

                // Report the whole field or event declaration when all variables in it are unused.
                var fieldOrEvent = unused.Symbol is IFieldSymbol || unused.Symbol is IEventSymbol;
                if (fieldOrEvent)
                {
                    if (alreadyReportedFieldLikeSymbols.Contains(unused.Symbol))
                    {
                        continue;
                    }

                    var declarations = GetSiblingDeclarators((VariableDeclaratorSyntax)unused.Syntax)
                        .Select(fieldLikeSymbols.GetByB)
                        .ToList();

                    if (declarations.All(unusedSymbols.Contains))
                    {
                        syntaxForLocation = unused.Syntax.Parent.Parent;
                        alreadyReportedFieldLikeSymbols.UnionWith(declarations);
                    }
                }

                diagnostics.Add(CreateDiagnostic(syntaxForLocation, unused.Symbol, accessibility));
            }

            return diagnostics;
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsForProperty(IPropertySymbol property,
            Dictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
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
                if (accessorSyntax != null && accessorSyntax.HasBodyOrExpressionBody())
                {
                    yield return Diagnostic.Create(rule, accessorSyntax.GetLocation(), "private",
                        "get accessor in property", property.Name);
                }
            }

            AccessorDeclarationSyntax GetAccessorSyntax(IMethodSymbol methodSymbol) =>
                methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
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

        private static string GetMemberName(ISymbol symbol) =>
            symbol.IsConstructor()
                ? symbol.ContainingType.Name
                : symbol.Name;

        private static void VisitDeclaringReferences(INamedTypeSymbol namedType, CSharpSyntaxVisitor visitor)
        {
            foreach (var reference in namedType.DeclaringSyntaxReferences)
            {
                visitor.Visit(reference.GetSyntax());
            }
        }

        private static IEnumerable<VariableDeclaratorSyntax> GetSiblingDeclarators(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Parent.Parent is FieldDeclarationSyntax fieldDeclaration)
            {
                return fieldDeclaration.Declaration.Variables;
            }
            else if (variableDeclarator.Parent.Parent is EventFieldDeclarationSyntax eventDeclaration)
            {
                return eventDeclaration.Declaration.Variables;
            }
            else
            {
                return Enumerable.Empty<VariableDeclaratorSyntax>();
            }
        }

        private static Diagnostic CreateDiagnostic(SyntaxNode syntaxNode, ISymbol symbol, string accessibility)
        {
            var memberType = GetMemberType(symbol);
            var memberName = GetMemberName(symbol);
            return Diagnostic.Create(rule, syntaxNode.GetLocation(), accessibility, memberType, memberName);
        }
    }
}
