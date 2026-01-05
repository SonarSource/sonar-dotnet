/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

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

                        var usageCollector = new SymbolUsageCollector(cc.Compilation, removableInternalTypes);
                        foreach (var syntaxTree in cc.Compilation.SyntaxTrees.Where(tree => !tree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, cc.IsRazorAnalysisEnabled())))
                        {
                            usageCollector.SafeVisit(syntaxTree.GetRoot());
                        }
                        ReportUnusedPrivateMembers(cc, usageCollector, removableInternalTypes, SyntaxConstants.Internal, new());
                    });
            });

    private static void NamedSymbolAction(SonarSymbolReportingContext context, HashSet<ISymbol> removableInternalTypes)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        var privateSymbols = new HashSet<ISymbol>();
        var fieldLikeSymbols = new BidirectionalDictionary<ISymbol, SyntaxNode>();
        if (GatherSymbols(namedType, context.Compilation, privateSymbols, removableInternalTypes, fieldLikeSymbols, context)
            && privateSymbols.Any()
            && new SymbolUsageCollector(context.Compilation, AssociatedSymbols(privateSymbols)) is var usageCollector
            && VisitDeclaringReferences(namedType, usageCollector, context, includeGeneratedFile: true))
        {
            ReportUnusedPrivateMembers(context, usageCollector, privateSymbols, SyntaxConstants.Private, fieldLikeSymbols);
            ReportUsedButUnreadFields(context, usageCollector, privateSymbols);
        }
    }

    private static IEnumerable<ISymbol> AssociatedSymbols(IEnumerable<ISymbol> privateSymbols) =>
        privateSymbols.Select(x => x is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } ? property : x);

    private static bool GatherSymbols(INamedTypeSymbol namedType,
                                      Compilation compilation,
                                      HashSet<ISymbol> privateSymbols,
                                      HashSet<ISymbol> internalSymbols,
                                      BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols,
                                      SonarSymbolReportingContext context)
    {
        if (namedType.ContainingType is not null
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
                && semanticModel.GetDeclaredSymbol(declaration) is { } declarationSymbol)
            {
                var symbolsCollector = RetrieveRemovableSymbols(declarationSymbol, compilation, context);
                CopyRetrievedSymbols(symbolsCollector, privateSymbols, internalSymbols, fieldLikeSymbols);
            }
        }

        return true;

        static IEnumerable<BaseTypeDeclarationSyntax> PrivateNestedMembersFromNonGeneratedCode(INamedTypeSymbol namedType, SonarSymbolReportingContext context) =>
            namedType.DeclaringSyntaxReferences
                .Where(x => !x.SyntaxTree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, context.IsRazorAnalysisEnabled()))
                .SelectMany(x => x.GetSyntax().ChildNodes().OfType<BaseTypeDeclarationSyntax>());
    }

    private static void ReportUnusedPrivateMembers<TContext>(TContext context,
                                                             SymbolUsageCollector usageCollector,
                                                             ISet<ISymbol> removableSymbols,
                                                             string accessibility,
                                                             BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols) where TContext : ICompilationReport
    {
        var unusedSymbols = UnusedSymbols(usageCollector, removableSymbols);

        var propertiesWithUnusedAccessor = removableSymbols
            .Intersect(usageCollector.UsedSymbols)
            .OfType<IPropertySymbol>()
            .Where(usageCollector.PropertyAccess.ContainsKey)
            .Where(x => !IsMentionedInDebuggerDisplay(x, usageCollector));
        foreach (var property in propertiesWithUnusedAccessor)
        {
            ReportProperty(context, property, usageCollector.PropertyAccess);
        }
        ReportDiagnosticsForMembers(context, unusedSymbols, accessibility, fieldLikeSymbols);
    }

    private static bool IsUsedWithReflection(ISymbol symbol, HashSet<ISymbol> symbolsUsedWithReflection)
    {
        var currentSymbol = symbol;
        while (currentSymbol is not null)
        {
            if (symbolsUsedWithReflection.Contains(currentSymbol))
            {
                return true;
            }
            currentSymbol = currentSymbol.ContainingSymbol;
        }
        return false;
    }

    private static bool IsMentionedInDebuggerDisplay(ISymbol symbol, SymbolUsageCollector usageCollector) =>
            usageCollector.DebuggerDisplayValues.Any(x => x.Contains(symbol.Name) || (symbol is IPropertySymbol { IsIndexer: true } && x.Contains("this[")));

    private static void ReportUsedButUnreadFields(SonarSymbolReportingContext context, SymbolUsageCollector usageCollector, IEnumerable<ISymbol> removableSymbols)
    {
        var unusedSymbols = UnusedSymbols(usageCollector, removableSymbols);

        var usedButUnreadFields = usageCollector.FieldSymbolUsages.Values
            .Where(x => x.Symbol.DeclaredAccessibility == Accessibility.Private || x.Symbol.ContainingType?.DeclaredAccessibility == Accessibility.Private)
            .Where(x => x.Symbol.Kind is SymbolKind.Field or SymbolKind.Event)
            .Where(x => !unusedSymbols.Contains(x.Symbol)
                        && !IsMentionedInDebuggerDisplay(x.Symbol, usageCollector)
                        && !IsUsedWithReflection(x.Symbol, usageCollector.TypesUsedWithReflection))
            .Where(x => x.Declaration is not null && !x.Readings.Any());

        foreach (var usage in usedButUnreadFields)
        {
            context.ReportIssue(RuleS4487, usage.Declaration.GetLocation(), FieldAccessibilityForMessage(usage.Symbol), usage.Symbol.Name);
        }
    }

    private static HashSet<ISymbol> UnusedSymbols(SymbolUsageCollector usageCollector, IEnumerable<ISymbol> removableSymbols) =>
        removableSymbols
            .Except(usageCollector.UsedSymbols)
            .Where(x => !IsMentionedInDebuggerDisplay(x, usageCollector)
                        && !IsAccessorUsed(x, usageCollector)
                        && !IsDeconstructMethod(x)
                        && !usageCollector.PrivateAttributes.Contains(x)
                        && !IsUsedWithReflection(x, usageCollector.TypesUsedWithReflection))
            .ToHashSet();

    private static bool IsDeconstructMethod(ISymbol symbol) =>
        symbol is IMethodSymbol { Name: "Deconstruct", Parameters.Length: > 0 } method
        && method.ReturnType.Is(KnownType.Void)
        && method.Parameters.All(x => x.RefKind == RefKind.Out);

    private static bool IsAccessorUsed(ISymbol symbol, SymbolUsageCollector usageCollector) =>
        symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } accessor
        && usageCollector.PropertyAccess.TryGetValue(property, out var access)
        && ((access.HasFlag(AccessorAccess.Get) && accessor.MethodKind == MethodKind.PropertyGet)
            || (access.HasFlag(AccessorAccess.Set) && accessor.MethodKind == MethodKind.PropertySet));

    private static string FieldAccessibilityForMessage(ISymbol symbol) =>
        symbol.DeclaredAccessibility == Accessibility.Private ? SyntaxConstants.Private : "private class";

    private static void ReportDiagnosticsForMembers<TContext>(TContext context,
                                                              ICollection<ISymbol> unusedSymbols,
                                                              string accessibility,
                                                              BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols) where TContext : ICompilationReport
    {
        var alreadyReportedFieldLikeSymbols = new HashSet<ISymbol>();
        var unusedSymbolSyntaxPairs = unusedSymbols.SelectMany(x => x.DeclaringSyntaxReferences.Select(syntax => new NodeAndSymbol(syntax.GetSyntax(), x)));

        foreach (var unused in unusedSymbolSyntaxPairs)
        {
            var syntaxForLocation = unused.Node;

            var isFieldOrEvent = unused.Symbol.Kind is SymbolKind.Field or SymbolKind.Event;
            if (isFieldOrEvent && unused.Node.IsKind(SyntaxKind.VariableDeclarator))
            {
                if (alreadyReportedFieldLikeSymbols.Contains(unused.Symbol))
                {
                    continue;
                }

                var declarations = GetSiblingDeclarators(unused.Node).Select(fieldLikeSymbols.GetByB).ToList();
                if (declarations.TrueForAll(unusedSymbols.Contains))
                {
                    syntaxForLocation = unused.Node.Parent.Parent;
                    alreadyReportedFieldLikeSymbols.UnionWith(declarations);
                }
            }

            if (unused.Symbol.IsConstructor() && !syntaxForLocation.GetModifiers().Any(SyntaxKind.PrivateKeyword))
            {
                context.ReportIssue(RuleS1144ForPublicCtor, syntaxForLocation, accessibility, unused.Symbol.ContainingType.Name);
            }
            else
            {
                context.ReportIssue(RuleS1144, IdentifierLocation(syntaxForLocation), accessibility, unused.Symbol.GetClassification(), MemberName(unused.Symbol));
            }
        }

        static IEnumerable<VariableDeclaratorSyntax> GetSiblingDeclarators(SyntaxNode variableDeclarator) =>
            variableDeclarator.Parent.Parent switch
            {
                FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables,
                EventFieldDeclarationSyntax eventDeclaration => eventDeclaration.Declaration.Variables,
                _ => [],
            };

        static Location IdentifierLocation(SyntaxNode node) =>
            node.GetIdentifier() is { } identifier
                ? identifier.GetLocation()
                : node.GetLocation();

        static string MemberName(ISymbol symbol) =>
            symbol.IsConstructor() ? symbol.ContainingType.Name : symbol.Name;
    }

    private static void ReportProperty<TContext>(TContext context,
                                                 IPropertySymbol property,
                                                 IReadOnlyDictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess) where TContext : ICompilationReport
    {
        var access = propertyAccessorAccess[property];
        if (access == AccessorAccess.Get
            && property.SetMethod is not null
            && GetAccessorSyntax(property.SetMethod) is { } setter)
        {
            context.ReportIssue(RuleS1144, setter.Keyword.GetLocation(), SyntaxConstants.Private, "set accessor in property", property.Name);
        }
        else if (access == AccessorAccess.Set
            && property.GetMethod is not null
            && GetAccessorSyntax(property.GetMethod) is { } getter
            && getter.HasBodyOrExpressionBody())
        {
            context.ReportIssue(RuleS1144, getter.Keyword.GetLocation(), SyntaxConstants.Private, "get accessor in property", property.Name);
        }

        static AccessorDeclarationSyntax GetAccessorSyntax(ISymbol symbol) =>
            symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
    }

    private static bool VisitDeclaringReferences(ISymbol symbol, ISafeSyntaxWalker visitor, SonarSymbolReportingContext context, bool includeGeneratedFile)
    {
        var syntaxReferencesToVisit = includeGeneratedFile
            ? symbol.DeclaringSyntaxReferences
            : symbol.DeclaringSyntaxReferences.Where(x => !IsGenerated(x));

        return syntaxReferencesToVisit.All(x => visitor.SafeVisit(x.GetSyntax()));

        bool IsGenerated(SyntaxReference syntaxReference) =>
            syntaxReference.SyntaxTree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, context.IsRazorAnalysisEnabled());
    }

    private static CSharpRemovableSymbolWalker RetrieveRemovableSymbols(INamedTypeSymbol namedType, Compilation compilation, SonarSymbolReportingContext context)
    {
        var removableSymbolsCollector = new CSharpRemovableSymbolWalker(compilation.GetSemanticModel, namedType.DeclaredAccessibility);
        return VisitDeclaringReferences(namedType, removableSymbolsCollector, context, includeGeneratedFile: false)
            ? removableSymbolsCollector
            : null;
    }

    private static void CopyRetrievedSymbols(CSharpRemovableSymbolWalker removableSymbolCollector,
                                             HashSet<ISymbol> privateSymbols,
                                             HashSet<ISymbol> internalSymbols,
                                             BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
    {
        privateSymbols.AddRange(removableSymbolCollector.PrivateSymbols);

        // Keep the removable internal types for when the compilation ends
        internalSymbols.AddRange(removableSymbolCollector.InternalSymbols);

        foreach (var pair in removableSymbolCollector.FieldLikeSymbols.Where(x => !fieldLikeSymbols.ContainsKeyByA(x.Key)))
        {
            fieldLikeSymbols.Add(pair.Key, pair.Value);
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
            this.getSemanticModel = x => getSemanticModel(x.SyntaxTree, false);
            this.containingTypeAccessibility = containingTypeAccessibility;
        }

        // This override is needed because VisitRecordDeclaration and LocalFunctionStatementSyntax are not available due to the Roslyn version.
        public override void Visit(SyntaxNode node)
        {
            if (node.Kind() is SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration)
            {
                VisitBaseTypeDeclaration(node);
            }

            if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
            {
                ConditionalStore((IMethodSymbol)DeclaredSymbol(node), IsRemovableMethod);
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
                ConditionalStore((IMethodSymbol)DeclaredSymbol(node), IsRemovableMethod);
            }

            base.VisitConstructorDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            if (IsPrivateOrInPrivateType(node.Modifiers, true))
            {
                ConditionalStore(DeclaredSymbol(node), IsRemovableType);
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
                var symbol = (IEventSymbol)DeclaredSymbol(node);
                ConditionalStore(symbol.PartialDefinitionPart ?? symbol, IsRemovableMember);
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
                ConditionalStore(DeclaredSymbol(node), IsRemovableMember);
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
                var symbol = (IMethodSymbol)DeclaredSymbol(node);
                ConditionalStore(symbol.AssociatedExtensionImplementation ?? symbol.PartialDefinitionPart ?? symbol, IsRemovableMethod);
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                ConditionalStore(DeclaredSymbol(node), IsRemovable);
            }

            base.VisitAccessorDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (IsPrivateOrInPrivateType(node.Modifiers))
            {
                ConditionalStore(DeclaredSymbol(node), IsRemovableMember);
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

        private ISymbol DeclaredSymbol(SyntaxNode node) =>
            getSemanticModel(node).GetDeclaredSymbol(node);

        private void StoreRemovableVariableDeclarations(BaseFieldDeclarationSyntax node)
        {
            foreach (var variable in node.Declaration.Variables)
            {
                var symbol = DeclaredSymbol(variable);
                if (IsRemovableMember(symbol))
                {
                    PrivateSymbols.Add(symbol);
                    FieldLikeSymbols.Add(symbol, variable);
                }
            }
        }

        private static bool IsEmptyConstructor(BaseMethodDeclarationSyntax constructorDeclaration) =>
            !constructorDeclaration.HasBodyOrExpressionBody()
            || constructorDeclaration.Body is { Statements.Count: 0 };

        private static bool IsRemovableMethod(IMethodSymbol methodSymbol) =>
            IsRemovableMember(methodSymbol)
            && (methodSymbol.MethodKind is MethodKind.Ordinary or MethodKind.Constructor or MethodKindEx.LocalFunction)
            && !methodSymbol.IsMainMethod()
            && !methodSymbol.IsEventHandler() // Event handlers could be added in XAML and no method reference will be generated in the .g.cs file.
            && !methodSymbol.IsSerializationConstructor()
            && !methodSymbol.IsRecordPrintMembers();

        private static bool IsRemovable(ISymbol symbol) =>
            symbol is { IsImplicitlyDeclared: false, IsVirtual: false }
            && !HasAttributes(symbol)
            && !symbol.IsSerializableMember()
            && !symbol.ContainingType.IsInterface()
            && !(symbol.Kind is SymbolKind.Field && symbol.ContainingType.HasAttribute(KnownType.System_Runtime_InteropServices_StructLayoutAttribute))
            && symbol.InterfaceMembers().IsEmpty()
            && symbol.GetOverriddenMember() is null;

        private static bool HasAttributes(ISymbol symbol)
        {
            var attributes = symbol.GetAttributes().AsEnumerable();
            if (symbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet, AssociatedSymbol: { } property })
            {
                attributes = attributes.Union(property.GetAttributes());
            }
            return attributes.Any(x => !x.AttributeClass.Is(KnownType.System_NonSerializedAttribute));
        }

        private static bool IsRemovableMember(ISymbol symbol) =>
            symbol.GetEffectiveAccessibility() == Accessibility.Private
            && IsRemovable(symbol);

        private static bool IsRemovableType(ISymbol typeSymbol) =>
            typeSymbol.GetEffectiveAccessibility() is var accessibility
            && typeSymbol.ContainingType is not null
            && (accessibility is Accessibility.Private or Accessibility.Internal)
            && IsRemovable(typeSymbol);

        private void VisitBaseTypeDeclaration(SyntaxNode node)
        {
            if (IsPrivateOrInPrivateType(((BaseTypeDeclarationSyntax)node).Modifiers, true))
            {
                ConditionalStore(DeclaredSymbol(node), IsRemovableType);
            }
        }

        private bool IsPrivateOrInPrivateType(SyntaxTokenList modifiers, bool checkInternal = false) =>
            containingTypeAccessibility == Accessibility.Private
            || (checkInternal && containingTypeAccessibility == Accessibility.Internal)
            || !modifiers.Any(x => x.Kind() is SyntaxKind.PrivateKeyword or SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword or SyntaxKind.ProtectedKeyword)
            || modifiers.Any(SyntaxKind.PrivateKeyword);
    }
}
