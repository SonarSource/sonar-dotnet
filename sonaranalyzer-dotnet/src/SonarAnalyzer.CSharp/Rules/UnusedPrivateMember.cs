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
    public sealed class UnusedPrivateMember : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1144";
        private const string MessageFormat = "Remove the unused {0} {1} '{2}'.";

        private static readonly ImmutableArray<KnownType> IgnoredTypes =
            ImmutableArray.Create(
                KnownType.UnityEditor_AssetModificationProcessor,
                KnownType.UnityEditor_AssetPostprocessor,
                KnownType.UnityEngine_MonoBehaviour,
                KnownType.UnityEngine_ScriptableObject
            );

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                fadeOutCode: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
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
                            var removableSymbolsCollector = new CSharpRemovableSymbolWalker(c.Compilation.GetSemanticModel);

                            if (!VisitDeclaringReferences(namedType, removableSymbolsCollector))
                            {
                                return;
                            }

                            // Keep the removable internal types for when the compilation ends
                            foreach (var internalSymbol in removableSymbolsCollector.InternalSymbols.OfType<INamedTypeSymbol>())
                            {
                                removableInternalTypes.Add(internalSymbol);
                            }

                            var usageCollector = new CSharpSymbolUsageCollector(
                                c.Compilation.GetSemanticModel,
                                removableSymbolsCollector.PrivateSymbols.Select(s => s.Name).ToHashSet());

                            if (!VisitDeclaringReferences(namedType, usageCollector))
                            {
                                return;
                            }

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

                            var usageCollector = new CSharpSymbolUsageCollector(
                                c.Compilation.GetSemanticModel,
                                removableInternalTypes.Select(s => s.Name).ToHashSet());

                            foreach (var symbol in allNamedTypes)
                            {
                                if (!VisitDeclaringReferences(symbol, usageCollector))
                                {
                                    return;
                                }
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

        private static Diagnostic CreateDiagnostic(SyntaxNode syntaxNode, ISymbol symbol, string accessibility)
        {
            var memberType = GetMemberType(symbol);
            var memberName = GetMemberName(symbol);
            return Diagnostic.Create(rule, syntaxNode.GetLocation(), accessibility, memberType, memberName);
        }

        private static IEnumerable<Diagnostic> GetDiagnostics(CSharpSymbolUsageCollector usageCollector, ISet<ISymbol> removableSymbols,
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

                var isFieldOrEvent = unused.Symbol.Kind == SymbolKind.Field || unused.Symbol.Kind == SymbolKind.Event;
                if (isFieldOrEvent &&
                    unused.Syntax.IsKind(SyntaxKind.VariableDeclarator))
                {
                    if (alreadyReportedFieldLikeSymbols.Contains(unused.Symbol))
                    {
                        continue;
                    }

                    var declarations = GetSiblingDeclarators(unused.Syntax)
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

            IEnumerable<VariableDeclaratorSyntax> GetSiblingDeclarators(SyntaxNode variableDeclarator)
            {
                var nodeGrandParent = variableDeclarator.Parent.Parent;

                switch (nodeGrandParent.Kind())
                {
                    case SyntaxKind.FieldDeclaration:
                        return ((FieldDeclarationSyntax)nodeGrandParent).Declaration.Variables;

                    case SyntaxKind.EventFieldDeclaration:
                        return ((EventFieldDeclarationSyntax)nodeGrandParent).Declaration.Variables;

                    default:
                        return Enumerable.Empty<VariableDeclaratorSyntax>();
                }
            }
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
            else
            {
                // do nothing
            }

            AccessorDeclarationSyntax GetAccessorSyntax(IMethodSymbol methodSymbol) =>
                methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
        }

        private static string GetMemberName(ISymbol symbol) =>
            symbol.IsConstructor()
                ? symbol.ContainingType.Name
                : symbol.Name;

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
        private static bool VisitDeclaringReferences(INamedTypeSymbol namedType, CSharpSyntaxWalker visitor)
        {
            foreach (var reference in namedType.DeclaringSyntaxReferences)
            {
                if (!visitor.SafeVisit(reference.GetSyntax()))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Collects private or internal member symbols that could potentially be removed if they are not used.
        /// Members that are overriden, overridable, have specific use, etc. are not removable.
        /// </summary>
        private class CSharpRemovableSymbolWalker : CSharpSyntaxWalker
        {
            private readonly Func<SyntaxNode, SemanticModel> getSemanticModel;

            public CSharpRemovableSymbolWalker(Func<SyntaxTree, bool, SemanticModel> getSemanticModel)
            {
                this.getSemanticModel = node => getSemanticModel(node.SyntaxTree, false);
            }

            public BidirectionalDictionary<ISymbol, SyntaxNode> FieldLikeSymbols { get; } =
                new BidirectionalDictionary<ISymbol, SyntaxNode>();

            public HashSet<ISymbol> InternalSymbols { get; } =
                new HashSet<ISymbol>();

            public HashSet<ISymbol> PrivateSymbols { get; } =
                                                    new HashSet<ISymbol>();
            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                base.VisitClassDeclaration(node);
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                if (!IsEmptyConstructor(node))
                {
                    ConditionalStore((IMethodSymbol)GetDeclaredSymbol(node), IsRemovableMethod);
                }
                base.VisitConstructorDeclaration(node);
            }

            public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                base.VisitDelegateDeclaration(node);
            }

            public override void VisitEventDeclaration(EventDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                base.VisitEventDeclaration(node);
            }

            public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
            {
                StoreRemovableVariableDeclarations(node);
                base.VisitEventFieldDeclaration(node);
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                StoreRemovableVariableDeclarations(node);
                base.VisitFieldDeclaration(node);
            }

            public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                base.VisitIndexerDeclaration(node);
            }

            public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                base.VisitInterfaceDeclaration(node);
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var symbol = (IMethodSymbol)GetDeclaredSymbol(node);
                ConditionalStore(symbol.PartialDefinitionPart ?? symbol, IsRemovableMethod);
                base.VisitMethodDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                base.VisitPropertyDeclaration(node);
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                base.VisitStructDeclaration(node);
            }

            private static bool IsEmptyConstructor(ConstructorDeclarationSyntax constructorDeclaration) =>
                !constructorDeclaration.HasBodyOrExpressionBody() ||
                (constructorDeclaration.Body != null && constructorDeclaration.Body.Statements.Count == 0);

            private void ConditionalStore<TSymbol>(TSymbol symbol, Func<TSymbol, bool> condition)
                where TSymbol : ISymbol
            {
                if (condition(symbol))
                {
                    if (symbol.GetEffectiveAccessibility() == Accessibility.Private)
                    {
                        PrivateSymbols.Add(symbol);
                    }
                    else
                    {
                        InternalSymbols.Add(symbol);
                    }
                }
            }

            private ISymbol GetDeclaredSymbol(SyntaxNode syntaxNode) =>
                this.getSemanticModel(syntaxNode).GetDeclaredSymbol(syntaxNode);

            private bool IsRemovable(ISymbol symbol) =>
                symbol != null &&
                !symbol.IsImplicitlyDeclared &&
                !symbol.IsAbstract &&
                !symbol.IsVirtual &&
                !symbol.GetAttributes().Any() &&
                !symbol.ContainingType.IsInterface() &&
                symbol.GetInterfaceMember() == null &&
                symbol.GetOverriddenMember() == null;

            private bool IsRemovableMember(ISymbol symbol) =>
                symbol.GetEffectiveAccessibility() == Accessibility.Private &&
                IsRemovable(symbol);

            private bool IsRemovableMethod(IMethodSymbol methodSymbol) =>
                IsRemovableMember(methodSymbol) &&
                (methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor) &&
                !methodSymbol.IsMainMethod() &&
                (!methodSymbol.IsEventHandler() || !IsDeclaredInPartialClass(methodSymbol)) && // Event handlers could be added in XAML and no method reference will be generated in the .g.cs file.
                !methodSymbol.IsSerializationConstructor();

            private bool IsDeclaredInPartialClass(IMethodSymbol methodSymbol)
            {
                return methodSymbol.DeclaringSyntaxReferences
                    .Select(GetContainingTypeDeclaration)
                    .Any(IsPartial);

                TypeDeclarationSyntax GetContainingTypeDeclaration(SyntaxReference syntaxReference) =>
                    syntaxReference.GetSyntax().FirstAncestorOrSelf<TypeDeclarationSyntax>();

                bool IsPartial(TypeDeclarationSyntax typeDeclaration) =>
                    typeDeclaration.Modifiers.AnyOfKind(SyntaxKind.PartialKeyword);
            }

            private bool IsRemovableType(ISymbol typeSymbol)
            {
                var accessibility = typeSymbol.GetEffectiveAccessibility();
                return typeSymbol.ContainingType != null
                    && (accessibility == Accessibility.Private || accessibility == Accessibility.Internal)
                    && IsRemovable(typeSymbol);
            }

            private void StoreRemovableVariableDeclarations(BaseFieldDeclarationSyntax node)
            {
                foreach (var variable in node.Declaration.Variables)
                {
                    var symbol = GetDeclaredSymbol(variable);
                    if (IsRemovableMember(symbol))
                    {
                        PrivateSymbols.Add(symbol);
                        FieldLikeSymbols.Add(symbol, variable);
                    }
                }
            }
        }
    }
}
