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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;
using SymbolWithInitializer = System.Collections.Generic.KeyValuePair<Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax>;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed partial class MemberInitializerRedundant : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3604";
    private const string InstanceMemberMessage = "Remove the member initializer, all constructors set an initial value for the member.";
    private const string StaticMemberMessage = "Remove the static member initializer, a static constructor or module initializer sets an initial value for the member.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

    private readonly bool useSonarCfg;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public MemberInitializerRedundant() : this(AnalyzerConfiguration.AlwaysEnabled) { }

    internal MemberInitializerRedundant(IAnalyzerConfiguration configuration) =>
        useSonarCfg = configuration.UseSonarCfg();

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var declaration = (TypeDeclarationSyntax)c.Node;
                if (c.Model.GetDeclaredSymbol(declaration)?.GetMembers() is { Length: > 0 } members)
                {
                    // structs cannot initialize fields/properties at declaration time
                    // interfaces cannot have instance fields and instance properties cannot have initializers
                    if (declaration is ClassDeclarationSyntax)
                    {
                        CheckInstanceMembers(c, declaration, members);
                    }
                    CheckStaticMembers(c, declaration, members);
                }
            },
            // For record support, see details in https://github.com/SonarSource/sonar-dotnet/pull/4756
            // it is difficult to work with instance record constructors w/o raising FPs
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration);

    private void CheckInstanceMembers(SonarSyntaxNodeReportingContext c, TypeDeclarationSyntax declaration, IEnumerable<ISymbol> typeMembers)
    {
        var constructors = typeMembers.OfType<IMethodSymbol>().Where(x => x is { MethodKind: MethodKind.Constructor, IsStatic: false }).ToList();
        if (constructors.Exists(x => x.IsImplicitlyDeclared || x.IsPrimaryConstructor()))
        {
            // Implicit parameterless constructors and primary constructors can be considered as having an
            // empty body and they do not initialize any members. If any of these is present, the rule does not apply.
            return;
        }

        // only retrieve the member symbols (an expensive call) if there are explicit class initializers
        var initializedMembers = InitializedMembers(c.Model, declaration, IsNotStaticOrConst);
        if (initializedMembers.Count == 0)
        {
            return;
        }

        var constructorDeclarations = ConstructorDeclarations<ConstructorDeclarationSyntax>(c, constructors);
        foreach (var kvp in initializedMembers)
        {
            // the instance member should be initialized in ALL instance constructors
            // otherwise, initializing it inline makes sense and the rule should not report
            if (constructorDeclarations.TrueForAll(x =>
                    // Calls another ctor, which is also checked:
                    x is { Node.Initializer.ThisOrBaseKeyword.RawKind: (int)SyntaxKind.ThisKeyword }
                    || IsSymbolFirstSetInCfg(kvp.Key, x.Node, x.Model, c.Cancel)))
            {
                c.ReportIssue(Rule, kvp.Value, InstanceMemberMessage);
            }
        }
    }

    private void CheckStaticMembers(SonarSyntaxNodeReportingContext c, TypeDeclarationSyntax declaration, IEnumerable<ISymbol> typeMembers)
    {
        var typeInitializers = typeMembers.OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.StaticConstructor || x.IsModuleInitializer()).ToList();
        if (typeInitializers.Count == 0)
        {
            return;
        }

        // only retrieve the member symbols (an expensive call) if there are explicit class initializers
        var initializedMembers = InitializedMembers(c.Model, declaration, IsStatic);
        if (initializedMembers.Count == 0)
        {
            return;
        }

        var initializerDeclarations = ConstructorDeclarations<BaseMethodDeclarationSyntax>(c, typeInitializers);
        foreach (var memberSymbol in initializedMembers.Keys)
        {
            // there can be only one static constructor
            // all module initializers are executed when the type is created, so it is enough if ANY initializes the member
            if (initializerDeclarations.Any(x => IsSymbolFirstSetInCfg(memberSymbol, x.Node, x.Model, c.Cancel)))
            {
                c.ReportIssue(Rule, initializedMembers[memberSymbol], StaticMemberMessage);
            }
        }
    }

    /// <summary>
    /// Returns true if the member is overwritten without being read in the instance constructor.
    /// Returns false if the member is not set in the constructor, or if it is read before being set.
    /// </summary>
    private bool IsSymbolFirstSetInCfg(ISymbol classMember, BaseMethodDeclarationSyntax constructorOrInitializer, SemanticModel model, CancellationToken cancel)
    {
        if (useSonarCfg)
        {
            if (!CSharpControlFlowGraph.TryGet(constructorOrInitializer, model, out var cfg))
            {
                return false;
            }

            var checker = new SonarChecker(cfg, classMember, model);
            return checker.CheckAllPaths();
        }
        else if (ControlFlowGraph.Create(constructorOrInitializer, model, cancel) is { } cfg)
        {
            var checker = new RoslynChecker(cfg, classMember, cancel);
            return checker.CheckAllPaths();
        }
        else
        {
            return false;
        }
    }

    private static bool IsNotStaticOrConst(SyntaxTokenList tokenList) =>
        !tokenList.Any(x => x.Kind() is SyntaxKind.StaticKeyword or SyntaxKind.ConstKeyword);

    private static bool IsStatic(SyntaxTokenList tokenList) =>
        tokenList.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

    // Retrieves the class members which are initialized - instance or static ones, depending on the given modifiers.
    private static Dictionary<ISymbol, EqualsValueClauseSyntax> InitializedMembers(SemanticModel model,
                                                                                   TypeDeclarationSyntax declaration,
                                                                                   Func<SyntaxTokenList, bool> filterModifiers)
    {
        var candidateFields = InitializedFieldLikeDeclarations<FieldDeclarationSyntax, IFieldSymbol>(declaration, filterModifiers, model, x => x.Type);
        var candidateEvents = InitializedFieldLikeDeclarations<EventFieldDeclarationSyntax, IEventSymbol>(declaration, filterModifiers, model, x => x.Type);
        var candidateProperties = InitializedPropertyDeclarations(declaration, filterModifiers, model);
        var allMembers = candidateFields.Select(x => new SymbolWithInitializer(x.Symbol, x.Initializer))
            .Concat(candidateEvents.Select(x => new SymbolWithInitializer(x.Symbol, x.Initializer)))
            .Concat(candidateProperties.Select(x => new SymbolWithInitializer(x.Symbol, x.Initializer)))
            .ToDictionary(x => x.Key, x => x.Value);
        return allMembers;
    }

    private static List<NodeSymbolAndModel<TSyntax, IMethodSymbol>> ConstructorDeclarations<TSyntax>(SonarSyntaxNodeReportingContext context, List<IMethodSymbol> constructorSymbols)
        where TSyntax : SyntaxNode =>
        constructorSymbols.SelectMany(x =>
            x.DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<TSyntax>()
                .Select(declarationNode => new { declarationNode, constructorSymbol = x }))
            .Select(x => new { x.declarationNode, x.constructorSymbol, model = x.declarationNode.EnsureCorrectSemanticModelOrDefault(context.Model) })
            .Where(x => x.model is not null)
            .Select(x => new NodeSymbolAndModel<TSyntax, IMethodSymbol>(x.declarationNode, x.constructorSymbol, x.model))
            .ToList();

    private static IEnumerable<DeclarationTuple<IPropertySymbol>> InitializedPropertyDeclarations(TypeDeclarationSyntax declaration,
                                                                                                  Func<SyntaxTokenList, bool> filterModifiers,
                                                                                                  SemanticModel model) =>
        declaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(x => filterModifiers(x.Modifiers) && x.Initializer is not null && x.IsAutoProperty())
            .Select(x => new DeclarationTuple<IPropertySymbol>(x.Initializer, model.GetDeclaredSymbol(x)))
            .Where(x => x.Symbol is not null && !MemberInitializedToDefault.IsDefaultValueInitializer(x.Initializer, x.Symbol.Type));

    private static IEnumerable<DeclarationTuple<TSymbol>> InitializedFieldLikeDeclarations<TDeclarationType, TSymbol>(TypeDeclarationSyntax declaration,
                                                                                                                      Func<SyntaxTokenList, bool> filterModifiers,
                                                                                                                      SemanticModel model,
                                                                                                                      Func<TSymbol, ITypeSymbol> typeSelector)
        where TDeclarationType : BaseFieldDeclarationSyntax
        where TSymbol : class, ISymbol =>
        declaration.Members
            .OfType<TDeclarationType>()
            .Where(x => filterModifiers(x.Modifiers))
            .SelectMany(x => x.Declaration.Variables
                                .Where(v => v.Initializer is not null)
                                .Select(v => new DeclarationTuple<TSymbol>(v.Initializer, model.GetDeclaredSymbol(v) as TSymbol)))
            .Where(x => x.Symbol is not null && !MemberInitializedToDefault.IsDefaultValueInitializer(x.Initializer, typeSelector(x.Symbol)));

    private sealed class DeclarationTuple<TSymbol>
        where TSymbol : ISymbol
    {
        public EqualsValueClauseSyntax Initializer { get; }
        public TSymbol Symbol { get; }

        public DeclarationTuple(EqualsValueClauseSyntax initializer, TSymbol symbol)
        {
            Initializer = initializer;
            Symbol = symbol;
        }
    }
}
