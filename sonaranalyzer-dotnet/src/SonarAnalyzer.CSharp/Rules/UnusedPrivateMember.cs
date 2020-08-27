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
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S1144DiagnosticId)]
    [Rule(S4487DiagnosticId)]
    public sealed class UnusedPrivateMember : SonarDiagnosticAnalyzer
    {
        internal const string S1144DiagnosticId = "S1144";
        private const string S1144MessageFormat = "Remove the unused {0} {1} '{2}'.";

        private const string S4487DiagnosticId = "S4487";
        private const string S4487MessageFormat = "Remove this unread {0} field '{1}' or refactor the code to use its value.";

        private static readonly DiagnosticDescriptor ruleS1144 =
            DiagnosticDescriptorBuilder.GetDescriptor(S1144DiagnosticId, S1144MessageFormat, RspecStrings.ResourceManager,
                fadeOutCode: true);
        private static readonly DiagnosticDescriptor ruleS4487 =
            DiagnosticDescriptorBuilder.GetDescriptor(S4487DiagnosticId, S4487MessageFormat, RspecStrings.ResourceManager,
                fadeOutCode: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ruleS1144, ruleS4487);

        private static readonly ImmutableArray<KnownType> ignoredTypes =
            ImmutableArray.Create(
                KnownType.UnityEditor_AssetModificationProcessor,
                KnownType.UnityEditor_AssetPostprocessor,
                KnownType.UnityEngine_MonoBehaviour,
                KnownType.UnityEngine_ScriptableObject,
                KnownType.Microsoft_EntityFrameworkCore_Migrations_Migration
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    // Collect potentially removable internal types from the project to evaluate when
                    // the compilation is over, depending on whether InternalsVisibleTo attribute is present
                    // or not.
                    var removableInternalTypes = new ConcurrentBag<ISymbol>();

                    c.RegisterSymbolAction(
                        cc =>
                        {
                            var namedType = (INamedTypeSymbol)cc.Symbol;

                            if (namedType.TypeKind != TypeKind.Struct
                                && namedType.TypeKind != TypeKind.Class
                                && namedType.TypeKind != TypeKind.Delegate
                                && namedType.TypeKind != TypeKind.Enum
                                && namedType.TypeKind != TypeKind.Interface)
                            {
                                return;
                            }

                            if (namedType.ContainingType != null
                                || namedType.DerivesFromAny(ignoredTypes))
                            {
                                return;
                            }

                            // Collect symbols of private members that could potentially be removed
                            var removableSymbolsCollector = new CSharpRemovableSymbolWalker(c.Compilation.GetSemanticModel);

                            if (!VisitDeclaringReferences(namedType, removableSymbolsCollector, c.Compilation, includeGeneratedFile: false))
                            {
                                return;
                            }

                            // Keep the removable internal types for when the compilation ends
                            foreach (var internalSymbol in removableSymbolsCollector.InternalSymbols.OfType<INamedTypeSymbol>())
                            {
                                removableInternalTypes.Add(internalSymbol);
                            }

                            var usageCollector = new CSharpSymbolUsageCollector(c.Compilation.GetSemanticModel, removableSymbolsCollector.PrivateSymbols);

                            if (!VisitDeclaringReferences(namedType, usageCollector, c.Compilation, includeGeneratedFile: true))
                            {
                                return;
                            }

                            var diagnostics = GetDiagnosticsForUnusedPrivateMembers(usageCollector, removableSymbolsCollector.PrivateSymbols, "private", removableSymbolsCollector.FieldLikeSymbols)
                                                    .Concat(GetDiagnosticsForUsedButUnreadFields(usageCollector, removableSymbolsCollector.PrivateSymbols));
                            foreach (var diagnostic in diagnostics)
                            {
                                cc.ReportDiagnosticIfNonGenerated(diagnostic, cc.Compilation);
                            }
                        },
                        SymbolKind.NamedType);

                    c.RegisterCompilationEndAction(
                        cc =>
                        {
                            var foundInternalsVisibleTo = cc.Compilation.Assembly.HasAttribute(KnownType.System_Runtime_CompilerServices_InternalsVisibleToAttribute);
                            if (foundInternalsVisibleTo
                                || removableInternalTypes.Count == 0)
                            {
                                return;
                            }

                            var usageCollector = new CSharpSymbolUsageCollector(c.Compilation.GetSemanticModel, removableInternalTypes.ToHashSet());

                            foreach (var syntaxTree in c.Compilation.SyntaxTrees
                                                        .Where(tree => !tree.IsGenerated(CSharpGeneratedCodeRecognizer.Instance, c.Compilation)))
                            {
                                usageCollector.SafeVisit(syntaxTree.GetRoot());
                            }

                            var diagnostics = GetDiagnosticsForUnusedPrivateMembers(usageCollector, removableInternalTypes.ToHashSet(), "internal",
                                new BidirectionalDictionary<ISymbol, SyntaxNode>());
                            foreach (var diagnostic in diagnostics)
                            {
                                cc.ReportDiagnosticIfNonGenerated(diagnostic, cc.Compilation);
                            }
                        });
                });
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsForUnusedPrivateMembers(CSharpSymbolUsageCollector usageCollector, ISet<ISymbol> removableSymbols,
                                                                                     string accessibility, BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var unusedSymbols = GetUnusedSymbols(usageCollector, removableSymbols);

            var propertiesWithUnusedAccessor = removableSymbols
                .Intersect(usageCollector.UsedSymbols)
                .OfType<IPropertySymbol>()
                .Where(usageCollector.PropertyAccess.ContainsKey)
                .Where(symbol => !IsMentionedInDebuggerDisplay(symbol, usageCollector));

            return GetDiagnosticsForMembers(unusedSymbols, accessibility, fieldLikeSymbols)
                .Concat(propertiesWithUnusedAccessor.SelectMany(propertySymbol => GetDiagnosticsForProperty(propertySymbol, usageCollector.PropertyAccess)));
        }

        private static bool IsMentionedInDebuggerDisplay(ISymbol symbol, CSharpSymbolUsageCollector usageCollector) =>
                usageCollector.DebuggerDisplayValues.Any(value => value.Contains(symbol.Name));

        private static IEnumerable<Diagnostic> GetDiagnosticsForUsedButUnreadFields(CSharpSymbolUsageCollector usageCollector, IEnumerable<ISymbol> removableSymbols)
        {
            var unusedSymbols = GetUnusedSymbols(usageCollector, removableSymbols);

            var usedButUnreadFields = usageCollector.FieldSymbolUsages.Values
                .Where(usage => usage.Symbol.DeclaredAccessibility == Accessibility.Private
                                || usage.Symbol.ContainingType?.DeclaredAccessibility == Accessibility.Private)
                .Where(usage => usage.Symbol.Kind == SymbolKind.Field || usage.Symbol.Kind == SymbolKind.Event)
                .Where(usage => !unusedSymbols.Contains(usage.Symbol) && !IsMentionedInDebuggerDisplay(usage.Symbol, usageCollector))
                .Where(usage => usage.Declaration != null && !usage.Readings.Any());

            return GetDiagnosticsForUnreadFields(usedButUnreadFields);
        }

        private static HashSet<ISymbol> GetUnusedSymbols(CSharpSymbolUsageCollector usageCollector, IEnumerable<ISymbol> removableSymbols) =>
            removableSymbols
                .Except(usageCollector.UsedSymbols)
                .Where(symbol => !IsMentionedInDebuggerDisplay(symbol, usageCollector))
                .ToHashSet();

        private static IEnumerable<Diagnostic> GetDiagnosticsForUnreadFields(IEnumerable<SymbolUsage> unreadFields) =>
            unreadFields.Select(usage => Diagnostic.Create(ruleS4487, usage.Declaration.GetLocation(), GetFieldAccessibilityForMessage(usage.Symbol), usage.Symbol.Name));

        private static string GetFieldAccessibilityForMessage(ISymbol symbol) =>
            symbol.DeclaredAccessibility == Accessibility.Private ? "private" : "private class";

        private static IEnumerable<Diagnostic> GetDiagnosticsForMembers(ICollection<ISymbol> unusedSymbols, string accessibility,
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
                if (isFieldOrEvent
                    && unused.Syntax.IsKind(SyntaxKind.VariableDeclarator))
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

                diagnostics.Add(CreateS1144Diagnostic(syntaxForLocation, unused.Symbol));
            }

            return diagnostics;

            static IEnumerable<VariableDeclaratorSyntax> GetSiblingDeclarators(SyntaxNode variableDeclarator)
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

            Diagnostic CreateS1144Diagnostic(SyntaxNode syntaxNode, ISymbol symbol) =>
                Diagnostic.Create(ruleS1144, syntaxNode.GetLocation(), accessibility, GetMemberType(symbol), GetMemberName(symbol));
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsForProperty(IPropertySymbol property,
            IReadOnlyDictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
        {
            var access = propertyAccessorAccess[property];
            if (access == AccessorAccess.Get && property.SetMethod != null)
            {
                var accessorSyntax = GetAccessorSyntax(property.SetMethod);
                if (accessorSyntax != null)
                {
                    yield return Diagnostic.Create(ruleS1144, accessorSyntax.GetLocation(), "private",
                        "set accessor in property", property.Name);
                }
            }
            else if (access == AccessorAccess.Set && property.GetMethod != null)
            {
                var accessorSyntax = GetAccessorSyntax(property.GetMethod);
                if (accessorSyntax != null && accessorSyntax.HasBodyOrExpressionBody())
                {
                    yield return Diagnostic.Create(ruleS1144, accessorSyntax.GetLocation(), "private",
                        "get accessor in property", property.Name);
                }
            }
            else
            {
                // do nothing
            }

            static AccessorDeclarationSyntax GetAccessorSyntax(ISymbol symbol) =>
                symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
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

        private static bool VisitDeclaringReferences(ISymbol symbol, CSharpSyntaxWalker visitor, Compilation compilation,
            bool includeGeneratedFile)
        {
            var syntaxReferencesToVisit = includeGeneratedFile
                ? symbol.DeclaringSyntaxReferences
                : symbol.DeclaringSyntaxReferences.Where(r => !IsGenerated(r));

            foreach (var reference in syntaxReferencesToVisit)
            {
                if (!visitor.SafeVisit(reference.GetSyntax()))
                {
                    return false;
                }
            }

            return true;

            bool IsGenerated(SyntaxReference syntaxReference) =>
                syntaxReference.SyntaxTree.IsGenerated(CSharpGeneratedCodeRecognizer.Instance, compilation);
        }

        /// <summary>
        /// Collects private or internal member symbols that could potentially be removed if they are not used.
        /// Members that are overridden, overridable, have specific use, etc. are not removable.
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

            public HashSet<ISymbol> InternalSymbols { get; } = new HashSet<ISymbol>();

            public HashSet<ISymbol> PrivateSymbols { get; } = new HashSet<ISymbol>();

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

            public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
            {
                ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                base.VisitEnumDeclaration(node);
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
                getSemanticModel(syntaxNode).GetDeclaredSymbol(syntaxNode);

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

            private static bool IsEmptyConstructor(BaseMethodDeclarationSyntax constructorDeclaration) =>
                !constructorDeclaration.HasBodyOrExpressionBody()
                || (constructorDeclaration.Body != null && constructorDeclaration.Body.Statements.Count == 0);

            private static bool IsDeclaredInPartialClass(ISymbol methodSymbol)
            {
                return methodSymbol.DeclaringSyntaxReferences
                    .Select(GetContainingTypeDeclaration)
                    .Any(IsPartial);

                static TypeDeclarationSyntax GetContainingTypeDeclaration(SyntaxReference syntaxReference) =>
                    syntaxReference.GetSyntax().FirstAncestorOrSelf<TypeDeclarationSyntax>();

                static bool IsPartial(TypeDeclarationSyntax typeDeclaration) =>
                    typeDeclaration.Modifiers.AnyOfKind(SyntaxKind.PartialKeyword);
            }

            private static bool IsRemovableMethod(IMethodSymbol methodSymbol) =>
                IsRemovableMember(methodSymbol)
                && (methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor)
                && !methodSymbol.IsMainMethod()
                && (!methodSymbol.IsEventHandler() || !IsDeclaredInPartialClass(methodSymbol)) // Event handlers could be added in XAML and no method reference will be generated in the .g.cs file.
                && !methodSymbol.IsSerializationConstructor();

            private static bool IsRemovable(ISymbol symbol) =>
                symbol != null
                && !symbol.IsImplicitlyDeclared
                && !symbol.IsVirtual
                && !symbol.GetAttributes().Any()
                && !symbol.ContainingType.IsInterface()
                && symbol.GetInterfaceMember() == null
                && symbol.GetOverriddenMember() == null;

            private static bool IsRemovableMember(ISymbol symbol) =>
                symbol.GetEffectiveAccessibility() == Accessibility.Private
                && IsRemovable(symbol);

            private static bool IsRemovableType(ISymbol typeSymbol)
            {
                var accessibility = typeSymbol.GetEffectiveAccessibility();
                return typeSymbol.ContainingType != null
                       && (accessibility == Accessibility.Private || accessibility == Accessibility.Internal)
                       && IsRemovable(typeSymbol);
            }
        }
    }
}
