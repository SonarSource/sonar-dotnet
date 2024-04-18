/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnusedPrivateMember : SonarDiagnosticAnalyzer
    {
        internal const string S1144DiagnosticId = "S1144";
        private const string S1144MessageFormat = "Remove the unused {0} {1} '{2}'.";
        private const string S1144MessageFormatForPublicCtor = "Remove unused constructor of {0} type '{1}'.";

        private const string S4487DiagnosticId = "S4487";
        private const string S4487MessageFormat = "Remove this unread {0} field '{1}' or refactor the code to use its value.";

        private static readonly DiagnosticDescriptor RuleS1144 = DescriptorFactory.Create(S1144DiagnosticId, S1144MessageFormat);
        private static readonly DiagnosticDescriptor RuleS1144ForPublicCtor = DescriptorFactory.Create(S1144DiagnosticId, S1144MessageFormatForPublicCtor);
        private static readonly DiagnosticDescriptor RuleS4487 = DescriptorFactory.Create(S4487DiagnosticId, S4487MessageFormat);

        private static readonly ImmutableArray<KnownType> IgnoredTypes = ImmutableArray.Create(
            KnownType.UnityEditor_AssetModificationProcessor,
            KnownType.UnityEditor_AssetPostprocessor,
            KnownType.UnityEngine_MonoBehaviour,
            KnownType.UnityEngine_ScriptableObject,
            KnownType.Microsoft_EntityFrameworkCore_Migrations_Migration);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RuleS1144, RuleS4487);
        protected override bool EnableConcurrentExecution => false;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    // Collect potentially removable internal types from the project to evaluate when
                    // the compilation is over, depending on whether InternalsVisibleTo attribute is present
                    // or not.
                    var removableInternalTypes = new HashSet<ISymbol>();

                    c.RegisterSymbolAction(x => NamedSymbolAction(x, removableInternalTypes), SymbolKind.NamedType);
                    c.RegisterCompilationEndAction(
                        cc =>
                        {
                            var foundInternalsVisibleTo = cc.Compilation.Assembly.HasAttribute(KnownType.System_Runtime_CompilerServices_InternalsVisibleToAttribute);
                            if (foundInternalsVisibleTo || removableInternalTypes.Count == 0)
                            {
                                return;
                            }

                            var usageCollector = new CSharpSymbolUsageCollector(c.Compilation, removableInternalTypes);
                            foreach (var syntaxTree in c.Compilation.SyntaxTrees.Where(tree => !tree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, c.IsRazorAnalysisEnabled())))
                            {
                                usageCollector.SafeVisit(syntaxTree.GetRoot());
                            }
                            foreach (var diagnostic in DiagnosticsForUnusedPrivateMembers(usageCollector, removableInternalTypes, SyntaxConstants.Internal, new()))
                            {
                                cc.ReportIssue(diagnostic);
                            }
                        });
                });

        private static void NamedSymbolAction(SonarSymbolReportingContext context, HashSet<ISymbol> removableInternalTypes)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            var privateSymbols = new HashSet<ISymbol>();
            var fieldLikeSymbols = new BidirectionalDictionary<ISymbol, SyntaxNode>();
            if (GatherSymbols(namedType, context.Compilation, privateSymbols, removableInternalTypes, fieldLikeSymbols, context)
                && privateSymbols.Any()
                && new CSharpSymbolUsageCollector(context.Compilation, privateSymbols) is var usageCollector
                && VisitDeclaringReferences(namedType, usageCollector, context, includeGeneratedFile: true))
            {
                foreach (var diagnostic in DiagnosticsForUnusedPrivateMembers(usageCollector, privateSymbols, SyntaxConstants.Private, fieldLikeSymbols))
                {
                    context.ReportIssue(diagnostic);
                }
                foreach (var diagnostic in DiagnosticsForUsedButUnreadFields(usageCollector, privateSymbols))
                {
                    context.ReportIssue(diagnostic);
                }
            }
        }

        private static bool GatherSymbols(INamedTypeSymbol namedType,
                                          Compilation compilation,
                                          HashSet<ISymbol> privateSymbols,
                                          HashSet<ISymbol> internalSymbols,
                                          BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols,
                                          SonarSymbolReportingContext context)
        {
            if (namedType.ContainingType != null
                // We skip top level statements since they cannot have fields. Other declared types are analyzed separately.
                || namedType.IsTopLevelProgram()
                || namedType.DerivesFromAny(IgnoredTypes)
                // Collect symbols of private members that could potentially be removed
                || RetrieveRemovableSymbols(namedType, compilation, context) is not { } removableSymbolsCollector)
            {
                return false;
            }

            CopyRetrievedSymbols(removableSymbolsCollector, privateSymbols, internalSymbols, fieldLikeSymbols);

            // Collect symbols of private members that could potentially be removed for the nested classes
            foreach (var declaration in PrivateNestedMembersFromNonGeneratedCode(namedType, context))
            {
                if (compilation.GetSemanticModel(declaration.SyntaxTree) is { } semanticModel
                    && semanticModel.GetDeclaredSymbol(declaration) is { } declarationSymbol
                    && !declarationSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_StructLayoutAttribute))
                {
                    var symbolsCollector = RetrieveRemovableSymbols(declarationSymbol, compilation, context);
                    CopyRetrievedSymbols(symbolsCollector, privateSymbols, internalSymbols, fieldLikeSymbols);
                }
            }

            return true;

            static IEnumerable<BaseTypeDeclarationSyntax> PrivateNestedMembersFromNonGeneratedCode(INamedTypeSymbol namedType, SonarSymbolReportingContext context) =>
                namedType.DeclaringSyntaxReferences
                    .Where(r => !r.SyntaxTree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, context.IsRazorAnalysisEnabled()))
                    .SelectMany(x => x.GetSyntax().ChildNodes().OfType<BaseTypeDeclarationSyntax>());
        }

        private static IEnumerable<Diagnostic> DiagnosticsForUnusedPrivateMembers(CSharpSymbolUsageCollector usageCollector,
                                                                                     ISet<ISymbol> removableSymbols,
                                                                                     string accessibility,
                                                                                     BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var unusedSymbols = GetUnusedSymbols(usageCollector, removableSymbols);

            var propertiesWithUnusedAccessor = removableSymbols
                .Intersect(usageCollector.UsedSymbols)
                .OfType<IPropertySymbol>()
                .Where(usageCollector.PropertyAccess.ContainsKey)
                .Where(symbol => !IsMentionedInDebuggerDisplay(symbol, usageCollector))
                .Select(symbol => GetDiagnosticsForProperty(symbol, usageCollector.PropertyAccess))
                .WhereNotNull();

            return GetDiagnosticsForMembers(unusedSymbols, accessibility, fieldLikeSymbols).Concat(propertiesWithUnusedAccessor);
        }

        private static bool IsMentionedInDebuggerDisplay(ISymbol symbol, CSharpSymbolUsageCollector usageCollector) =>
                usageCollector.DebuggerDisplayValues.Any(value => value.Contains(symbol.Name));

        private static IEnumerable<Diagnostic> DiagnosticsForUsedButUnreadFields(CSharpSymbolUsageCollector usageCollector, IEnumerable<ISymbol> removableSymbols)
        {
            var unusedSymbols = GetUnusedSymbols(usageCollector, removableSymbols);

            var usedButUnreadFields = usageCollector.FieldSymbolUsages.Values
                .Where(usage => usage.Symbol.DeclaredAccessibility == Accessibility.Private || usage.Symbol.ContainingType?.DeclaredAccessibility == Accessibility.Private)
                .Where(usage => usage.Symbol.Kind == SymbolKind.Field || usage.Symbol.Kind == SymbolKind.Event)
                .Where(usage => !unusedSymbols.Contains(usage.Symbol) && !IsMentionedInDebuggerDisplay(usage.Symbol, usageCollector))
                .Where(usage => usage.Declaration != null && !usage.Readings.Any());

            return GetDiagnosticsForUnreadFields(usedButUnreadFields);
        }

        private static HashSet<ISymbol> GetUnusedSymbols(CSharpSymbolUsageCollector usageCollector, IEnumerable<ISymbol> removableSymbols) =>
            removableSymbols
                .Except(usageCollector.UsedSymbols)
                .Where(symbol => !IsMentionedInDebuggerDisplay(symbol, usageCollector) && !IsAccessorUsed(symbol, usageCollector))
                .ToHashSet();

        private static IEnumerable<Diagnostic> GetDiagnosticsForUnreadFields(IEnumerable<SymbolUsage> unreadFields) =>
            unreadFields.Select(usage => Diagnostic.Create(RuleS4487, usage.Declaration.GetLocation(), GetFieldAccessibilityForMessage(usage.Symbol), usage.Symbol.Name));

        private static bool IsAccessorUsed(ISymbol symbol, CSharpSymbolUsageCollector usageCollector) =>
            symbol is IMethodSymbol { } accessor
                && accessor.AssociatedSymbol is IPropertySymbol { } property
                && usageCollector.PropertyAccess.TryGetValue(property, out var access)
                && ((access == AccessorAccess.Get && accessor.MethodKind == MethodKind.PropertyGet)
                    || (access == AccessorAccess.Set && accessor.MethodKind == MethodKind.PropertySet));

        private static string GetFieldAccessibilityForMessage(ISymbol symbol) =>
            symbol.DeclaredAccessibility == Accessibility.Private ? SyntaxConstants.Private : "private class";

        private static IEnumerable<Diagnostic> GetDiagnosticsForMembers(ICollection<ISymbol> unusedSymbols,
                                                                        string accessibility,
                                                                        BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var diagnostics = new List<Diagnostic>();
            var alreadyReportedFieldLikeSymbols = new HashSet<ISymbol>();
            var unusedSymbolSyntaxPairs = unusedSymbols.SelectMany(symbol => symbol.DeclaringSyntaxReferences.Select(x => new NodeAndSymbol(x.GetSyntax(), symbol)));

            foreach (var unused in unusedSymbolSyntaxPairs)
            {
                var syntaxForLocation = unused.Node;

                var isFieldOrEvent = unused.Symbol.Kind == SymbolKind.Field || unused.Symbol.Kind == SymbolKind.Event;
                if (isFieldOrEvent && unused.Node.IsKind(SyntaxKind.VariableDeclarator))
                {
                    if (alreadyReportedFieldLikeSymbols.Contains(unused.Symbol))
                    {
                        continue;
                    }

                    var declarations = GetSiblingDeclarators(unused.Node).Select(fieldLikeSymbols.GetByB).ToList();
                    if (declarations.All(unusedSymbols.Contains))
                    {
                        syntaxForLocation = unused.Node.Parent.Parent;
                        alreadyReportedFieldLikeSymbols.UnionWith(declarations);
                    }
                }

                diagnostics.Add(CreateS1144Diagnostic(syntaxForLocation, unused.Symbol));
            }

            return diagnostics;

            static IEnumerable<VariableDeclaratorSyntax> GetSiblingDeclarators(SyntaxNode variableDeclarator) =>
                variableDeclarator.Parent.Parent switch
                {
                    FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables,
                    EventFieldDeclarationSyntax eventDeclaration => eventDeclaration.Declaration.Variables,
                    _ => Enumerable.Empty<VariableDeclaratorSyntax>(),
                };

            Diagnostic CreateS1144Diagnostic(SyntaxNode syntaxNode, ISymbol symbol) =>
                symbol.IsConstructor() && !syntaxNode.GetModifiers().Any(SyntaxKind.PrivateKeyword)
                    ? Diagnostic.Create(RuleS1144ForPublicCtor, syntaxNode.GetLocation(), accessibility, symbol.ContainingType.Name)
                    : Diagnostic.Create(RuleS1144, GetIdentifierLocation(syntaxNode), accessibility, symbol.GetClassification(), GetMemberName(symbol));

            static Location GetIdentifierLocation(SyntaxNode syntaxNode) =>
                syntaxNode.GetIdentifier() is { } identifier
                    ? identifier.GetLocation()
                    : syntaxNode.GetLocation();
        }

        private static Diagnostic GetDiagnosticsForProperty(IPropertySymbol property, IReadOnlyDictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
        {
            var access = propertyAccessorAccess[property];
            if (access == AccessorAccess.Get
                && property.SetMethod is { }
                && GetAccessorSyntax(property.SetMethod) is { } setter)
            {
                return Diagnostic.Create(RuleS1144, setter.Keyword.GetLocation(), SyntaxConstants.Private, "set accessor in property", property.Name);
            }
            else if (access == AccessorAccess.Set
                     && property.GetMethod is { }
                     && GetAccessorSyntax(property.GetMethod) is { } getter
                     && getter.HasBodyOrExpressionBody())
            {
                return Diagnostic.Create(RuleS1144, getter.Keyword.GetLocation(), SyntaxConstants.Private, "get accessor in property", property.Name);
            }
            else
            {
                return null;
            }

            static AccessorDeclarationSyntax GetAccessorSyntax(ISymbol symbol) =>
                symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
        }

        private static string GetMemberName(ISymbol symbol) =>
            symbol.IsConstructor() ? symbol.ContainingType.Name : symbol.Name;

        private static bool VisitDeclaringReferences(ISymbol symbol, ISafeSyntaxWalker visitor, SonarSymbolReportingContext context, bool includeGeneratedFile)
        {
            var syntaxReferencesToVisit = includeGeneratedFile
                ? symbol.DeclaringSyntaxReferences
                : symbol.DeclaringSyntaxReferences.Where(r => !IsGenerated(r));

            return syntaxReferencesToVisit.All(x => visitor.SafeVisit(x.GetSyntax()));

            bool IsGenerated(SyntaxReference syntaxReference) =>
                syntaxReference.SyntaxTree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, context.IsRazorAnalysisEnabled());
        }

        private static CSharpRemovableSymbolWalker RetrieveRemovableSymbols(INamedTypeSymbol namedType, Compilation compilation, SonarSymbolReportingContext context)
        {
            var removableSymbolsCollector = new CSharpRemovableSymbolWalker(compilation.GetSemanticModel, namedType.DeclaredAccessibility);
            if (!VisitDeclaringReferences(namedType, removableSymbolsCollector, context, includeGeneratedFile: false))
            {
                return null;
            }

            return removableSymbolsCollector;
        }

        private static void CopyRetrievedSymbols(CSharpRemovableSymbolWalker removableSymbolCollector,
                                                 HashSet<ISymbol> privateSymbols,
                                                 HashSet<ISymbol> internalSymbols,
                                                 BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            privateSymbols.AddRange(removableSymbolCollector.PrivateSymbols);

            // Keep the removable internal types for when the compilation ends
            internalSymbols.AddRange(removableSymbolCollector.InternalSymbols);

            foreach (var keyValuePair in removableSymbolCollector.FieldLikeSymbols)
            {
                if (!fieldLikeSymbols.ContainsKeyByA(keyValuePair.Key))
                {
                    fieldLikeSymbols.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        /// <summary>
        /// Collects private or internal member symbols that could potentially be removed if they are not used.
        /// Members that are overridden, overridable, have specific use, etc. are not removable.
        /// </summary>
        private sealed class CSharpRemovableSymbolWalker : SafeCSharpSyntaxWalker
        {
            private readonly Func<SyntaxNode, SemanticModel> getSemanticModel;
            private readonly Accessibility containingTypeAccessibility;

            public Dictionary<ISymbol, SyntaxNode> FieldLikeSymbols { get; } = [];
            public HashSet<ISymbol> InternalSymbols { get; } = [];
            public HashSet<ISymbol> PrivateSymbols { get; } = [];

            public CSharpRemovableSymbolWalker(Func<SyntaxTree, bool, SemanticModel> getSemanticModel, Accessibility containingTypeAccessibility)
            {
                this.getSemanticModel = node => getSemanticModel(node.SyntaxTree, false);
                this.containingTypeAccessibility = containingTypeAccessibility;
            }

            // This override is needed because VisitRecordDeclaration and LocalFunctionStatementSyntax are not available due to the Roslyn version.
            public override void Visit(SyntaxNode node)
            {
                if (node.IsAnyKind(SyntaxKindEx.RecordClassDeclaration, SyntaxKindEx.RecordStructDeclaration))
                {
                    VisitBaseTypeDeclaration(node);
                }

                if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
                {
                    ConditionalStore((IMethodSymbol)GetDeclaredSymbol(node), IsRemovableMethod);
                }

                base.Visit(node);
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                VisitBaseTypeDeclaration(node);
                base.VisitClassDeclaration(node);
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                if (!IsEmptyConstructor(node)
                    && IsPrivateOrInPrivateType(node.Modifiers))
                {
                    ConditionalStore((IMethodSymbol)GetDeclaredSymbol(node), IsRemovableMethod);
                }

                base.VisitConstructorDeclaration(node);
            }

            public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers, true))
                {
                    ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                }

                base.VisitDelegateDeclaration(node);
            }

            public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
            {
                VisitBaseTypeDeclaration(node);
                base.VisitEnumDeclaration(node);
            }

            public override void VisitEventDeclaration(EventDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers))
                {
                    ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                }

                base.VisitEventDeclaration(node);
            }

            public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers))
                {
                    StoreRemovableVariableDeclarations(node);
                }

                base.VisitEventFieldDeclaration(node);
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers))
                {
                    StoreRemovableVariableDeclarations(node);
                }

                base.VisitFieldDeclaration(node);
            }

            public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers))
                {
                    ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                }

                base.VisitIndexerDeclaration(node);
            }

            public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                VisitBaseTypeDeclaration(node);
                base.VisitInterfaceDeclaration(node);
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers))
                {
                    var symbol = (IMethodSymbol)GetDeclaredSymbol(node);
                    ConditionalStore(symbol.PartialDefinitionPart ?? symbol, IsRemovableMethod);
                }

                base.VisitMethodDeclaration(node);
            }
            public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
            {
                if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                }

                base.VisitAccessorDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (IsPrivateOrInPrivateType(node.Modifiers))
                {
                    ConditionalStore(GetDeclaredSymbol(node), IsRemovableMember);
                }

                base.VisitPropertyDeclaration(node);
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                VisitBaseTypeDeclaration(node);
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
                    else if (symbol is INamedTypeSymbol)
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
                || (constructorDeclaration.Body is { } && constructorDeclaration.Body.Statements.Count == 0);

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
                && (methodSymbol.MethodKind is MethodKind.Ordinary or MethodKind.Constructor or MethodKindEx.LocalFunction)
                && !methodSymbol.IsMainMethod()
                && (!methodSymbol.IsEventHandler() || !IsDeclaredInPartialClass(methodSymbol)) // Event handlers could be added in XAML and no method reference will be generated in the .g.cs file.
                && !methodSymbol.IsSerializationConstructor()
                && !methodSymbol.IsRecordPrintMembers();

            private static bool IsRemovable(ISymbol symbol) =>
                symbol is { IsImplicitlyDeclared: false, IsVirtual: false }
                && !HasAttributes(symbol)
                && !symbol.IsSerializableMember()
                && !symbol.ContainingType.IsInterface()
                && symbol.GetInterfaceMember() == null
                && symbol.GetOverriddenMember() == null;

            private static bool HasAttributes(ISymbol symbol) =>
                symbol.GetAttributes().Any(x => !x.AttributeClass.Is(KnownType.System_NonSerializedAttribute));

            private static bool IsRemovableMember(ISymbol symbol) =>
                symbol.GetEffectiveAccessibility() == Accessibility.Private
                && IsRemovable(symbol);

            private static bool IsRemovableType(ISymbol typeSymbol) =>
                typeSymbol.GetEffectiveAccessibility() is var accessibility
                && typeSymbol.ContainingType is { }
                && (accessibility is Accessibility.Private or Accessibility.Internal)
                && IsRemovable(typeSymbol);

            private void VisitBaseTypeDeclaration(SyntaxNode node)
            {
                if (IsPrivateOrInPrivateType(((BaseTypeDeclarationSyntax)node).Modifiers, true))
                {
                    ConditionalStore(GetDeclaredSymbol(node), IsRemovableType);
                }
            }

            private bool IsPrivateOrInPrivateType(SyntaxTokenList modifiers, bool checkInternal = false) =>
                containingTypeAccessibility == Accessibility.Private
                || (checkInternal && containingTypeAccessibility == Accessibility.Internal)
                || !modifiers.Any(x => x.IsAnyKind(SyntaxKind.PrivateKeyword, SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword))
                || modifiers.Any(SyntaxKind.PrivateKeyword);
        }
    }
}
