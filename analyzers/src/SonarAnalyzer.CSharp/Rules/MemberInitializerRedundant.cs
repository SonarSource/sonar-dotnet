/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;
using SymbolWithInitializer = System.Collections.Generic.KeyValuePair<Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax>;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed partial class MemberInitializerRedundant : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3604";
        private const string InstanceMemberMessage = "Remove the member initializer, all constructors set an initial value for the member.";
        private const string StaticMemberMessage = "Remove the static member initializer, a static constructor or module initializer sets an initial value for the member.";

        private readonly bool useSonarCfg;

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public MemberInitializerRedundant() : this(AnalyzerConfiguration.AlwaysEnabled) { }

        internal /* for testing */ MemberInitializerRedundant(IAnalyzerConfiguration configuration) =>
            useSonarCfg = configuration.UseSonarCfg();

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var declaration = (TypeDeclarationSyntax)c.Node;
                    var members = c.SemanticModel.GetDeclaredSymbol(declaration)?.GetMembers();
                    if (members == null || members.Value.Length == 0)
                    {
                        return;
                    }

                    // structs cannot initialize fields/properties at declaration time
                    // interfaces cannot have instance fields and instance properties cannot have initializers
                    if (declaration is ClassDeclarationSyntax)
                    {
                        CheckInstanceMembers(c, declaration, members);
                    }
                    CheckStaticMembers(c, declaration, members);
                },
                // For record support, see details in https://github.com/SonarSource/sonar-dotnet/pull/4756
                // it is difficult to work with instance record constructors w/o raising FPs
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration);

        private void CheckInstanceMembers(SonarSyntaxNodeReportingContext c, TypeDeclarationSyntax declaration, IEnumerable<ISymbol> typeMembers)
        {
            var typeInitializers = typeMembers.OfType<IMethodSymbol>().Where(x => x is { MethodKind: MethodKind.Constructor, IsStatic: false, IsImplicitlyDeclared: false }).ToList();
            if (typeInitializers.Count == 0)
            {
                return;
            }

            // only retrieve the member symbols (an expensive call) if there are explicit class initializers
            var initializedMembers = GetInitializedMembers(c.SemanticModel, declaration, IsNotStaticOrConst);
            if (initializedMembers.Count == 0)
            {
                return;
            }

            var initializerDeclarations = GetInitializerDeclarations<ConstructorDeclarationSyntax>(c, typeInitializers);
            foreach (var memberSymbol in initializedMembers.Keys)
            {
                // the instance member should be initialized in ALL instance constructors
                // otherwise, initializing it inline makes sense and the rule should not report
                if (initializerDeclarations.All(constructor =>
                    {
                        if (constructor.Node.Initializer != null
                            && constructor.Node.Initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword))
                        {
                            // Calls another ctor, which is also checked.
                            return true;
                        }

                        return IsSymbolFirstSetInCfg(memberSymbol, constructor.Node, constructor.Model, c.Cancel);
                    }))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, initializedMembers[memberSymbol].GetLocation(), InstanceMemberMessage));
                }
            }
        }

        private void CheckStaticMembers(SonarSyntaxNodeReportingContext c, TypeDeclarationSyntax declaration, IEnumerable<ISymbol> typeMembers)
        {
            var typeInitializers = typeMembers.OfType<IMethodSymbol>().Where(method => method.MethodKind == MethodKind.StaticConstructor || method.IsModuleInitializer()).ToList();
            if (typeInitializers.Count == 0)
            {
                return;
            }

            // only retrieve the member symbols (an expensive call) if there are explicit class initializers
            var initializedMembers = GetInitializedMembers(c.SemanticModel, declaration, IsStatic);
            if (initializedMembers.Count == 0)
            {
                return;
            }

            var initializerDeclarations = GetInitializerDeclarations<BaseMethodDeclarationSyntax>(c, typeInitializers);
            foreach (var memberSymbol in initializedMembers.Keys)
            {
                // there can be only one static constructor
                // all module initializers are executed when the type is created, so it is enough if ANY initializes the member
                if (initializerDeclarations.Any(x => IsSymbolFirstSetInCfg(memberSymbol, x.Node, x.Model, c.Cancel)))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, initializedMembers[memberSymbol].GetLocation(), StaticMemberMessage));
                }
            }
        }

        /// <summary>
        /// Returns true if the member is overwritten without being read in the instance constructor.
        /// Returns false if the member is not set in the constructor, or if it is read before being set.
        /// </summary>
        private bool IsSymbolFirstSetInCfg(ISymbol classMember, BaseMethodDeclarationSyntax constructorOrInitializer, SemanticModel semanticModel, CancellationToken cancel)
        {
            var body = (CSharpSyntaxNode)constructorOrInitializer.Body ?? constructorOrInitializer.ExpressionBody();
            if (useSonarCfg)
            {
                if (!CSharpControlFlowGraph.TryGet(body, semanticModel, out var cfg))
                {
                    return false;
                }

                var checker = new SonarChecker(cfg, classMember, semanticModel);
                return checker.CheckAllPaths();
            }
            else
            {
                var cfg = ControlFlowGraph.Create(body.Parent, semanticModel, cancel);
                var checker = new RoslynChecker(cfg, classMember, cancel);
                return checker.CheckAllPaths();
            }
        }

        private static bool IsNotStaticOrConst(SyntaxTokenList tokenList) =>
            !tokenList.Any(x => x.IsAnyKind(SyntaxKind.StaticKeyword, SyntaxKind.ConstKeyword));

        private static bool IsStatic(SyntaxTokenList tokenList) =>
            tokenList.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

        // Retrieves the class members which are initialized - instance or static ones, depending on the given modifiers.
        private static Dictionary<ISymbol, EqualsValueClauseSyntax> GetInitializedMembers(SemanticModel semanticModel,
                                                                                          TypeDeclarationSyntax declaration,
                                                                                          Func<SyntaxTokenList, bool> filterModifiers)
        {
            var candidateFields = GetInitializedFieldLikeDeclarations<FieldDeclarationSyntax, IFieldSymbol>(declaration, filterModifiers, semanticModel, f => f.Type);
            var candidateEvents = GetInitializedFieldLikeDeclarations<EventFieldDeclarationSyntax, IEventSymbol>(declaration, filterModifiers, semanticModel, f => f.Type);
            var candidateProperties = GetInitializedPropertyDeclarations(declaration, filterModifiers, semanticModel);
            var allMembers = candidateFields.Select(t => new SymbolWithInitializer(t.Symbol, t.Initializer))
                .Concat(candidateEvents.Select(t => new SymbolWithInitializer(t.Symbol, t.Initializer)))
                .Concat(candidateProperties.Select(t => new SymbolWithInitializer(t.Symbol, t.Initializer)))
                .ToDictionary(t => t.Key, t => t.Value);
            return allMembers;
        }

        private static List<NodeSymbolAndModel<TSyntax, IMethodSymbol>> GetInitializerDeclarations<TSyntax>(SonarSyntaxNodeReportingContext context, List<IMethodSymbol> constructorSymbols)
            where TSyntax : SyntaxNode =>
            constructorSymbols
                .Select(x => new NodeSymbolAndModel<TSyntax, IMethodSymbol>(null, x.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TSyntax, x))
                .Where(x => x.Node != null)
                .Select(x => new NodeSymbolAndModel<TSyntax, IMethodSymbol>(x.Node.EnsureCorrectSemanticModelOrDefault(context.SemanticModel), x.Node, x.Symbol))
                .Where(x => x.Model != null)
                .ToList();

        private static IEnumerable<DeclarationTuple<IPropertySymbol>> GetInitializedPropertyDeclarations(TypeDeclarationSyntax declaration,
                                                                                                         Func<SyntaxTokenList, bool> filterModifiers,
                                                                                                         SemanticModel semanticModel) =>
            declaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => filterModifiers(p.Modifiers) && p.Initializer != null && p.IsAutoProperty())
                .Select(p => new DeclarationTuple<IPropertySymbol>(p.Initializer, semanticModel.GetDeclaredSymbol(p)))
                .Where(t => t.Symbol != null && !MemberInitializedToDefault.IsDefaultValueInitializer(t.Initializer, t.Symbol.Type));

        private static IEnumerable<DeclarationTuple<TSymbol>> GetInitializedFieldLikeDeclarations<TDeclarationType, TSymbol>(TypeDeclarationSyntax declaration,
                                                                                                                             Func<SyntaxTokenList, bool> filterModifiers,
                                                                                                                             SemanticModel semanticModel,
                                                                                                                             Func<TSymbol, ITypeSymbol> typeSelector)
            where TDeclarationType : BaseFieldDeclarationSyntax
            where TSymbol : class, ISymbol =>
            declaration.Members
                .OfType<TDeclarationType>()
                .Where(x => filterModifiers(x.Modifiers))
                .SelectMany(fd => fd.Declaration.Variables
                    .Where(v => v.Initializer != null)
                    .Select(v => new DeclarationTuple<TSymbol>(v.Initializer, semanticModel.GetDeclaredSymbol(v) as TSymbol)))
                .Where(t =>
                    t.Symbol != null
                    && !MemberInitializedToDefault.IsDefaultValueInitializer(t.Initializer, typeSelector(t.Symbol)));

        private class DeclarationTuple<TSymbol>
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
}
