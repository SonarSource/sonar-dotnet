/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

public abstract class ClassNotInstantiatableBase<TBaseTypeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TBaseTypeSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S3453";

    protected abstract IEnumerable<ConstructorContext> CollectRemovableDeclarations(INamedTypeSymbol namedType, Compilation compilation, string messageArg);

    protected override string MessageFormat => "This {0} can't be instantiated; make {1} 'public'.";

    protected ClassNotInstantiatableBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolAction(CheckClassWithOnlyUnusedPrivateConstructors, SymbolKind.NamedType);

    private bool IsClassTypeDeclaration(SyntaxNode node) =>
        Language.SyntaxKind.ClassAndRecordDeclarations.Contains(Language.Syntax.Kind(node));

    private bool IsAnyConstructorCalled(INamedTypeSymbol namedType, IEnumerable<ConstructorContext> typeDeclarations) =>
        typeDeclarations
            .Select(x => new
            {
                x.NodeAndModel.Model,
                DescendantNodes = x.NodeAndModel.Node.DescendantNodes().ToList()
            })
            .Any(x =>
                IsAnyConstructorToCurrentType(x.DescendantNodes, namedType, x.Model)
                || IsAnyNestedTypeExtendingCurrentType(x.DescendantNodes, namedType, x.Model));

    private void CheckClassWithOnlyUnusedPrivateConstructors(SonarSymbolReportingContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (!IsNonStaticClassWithNoAttributes(namedType) || DerivesFromSafeHandle(namedType))
        {
            return;
        }

        var members = namedType.GetMembers();
        var constructors = Constructors(members).Where(x => !x.IsImplicitlyDeclared).ToList();
        // Compiler-generated members are ignored, otherwise records would never be exempted: they always carry synthesized
        // instance members such as EqualityContract, PrintMembers and the copy constructor.
        var declaredMembers = members.Except(constructors).Where(x => !x.IsImplicitlyDeclared).ToList();

        if (!HasOnlyCandidateConstructors(constructors) || IsUsableWithoutInstantiation(declaredMembers))
        {
            return;
        }

        var messageArg = constructors.Count > 1 ? "at least one of its constructors" : "its constructor";
        var removableDeclarations = CollectRemovableDeclarations(namedType, context.Compilation, messageArg).ToList();

        if (!IsAnyConstructorCalled(namedType, removableDeclarations))
        {
            foreach (var typeDeclaration in removableDeclarations)
            {
                context.ReportIssue(Language.GeneratedCodeRecognizer, Rule, typeDeclaration.Location, messageArgs: typeDeclaration.MessageArgs);
            }
        }
    }

    private bool IsAnyNestedTypeExtendingCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel model) =>
        descendantNodes
            .Where(IsClassTypeDeclaration)
            .Select(x => (model.GetDeclaredSymbol(x) as ITypeSymbol)?.BaseType)
            .WhereNotNull()
            .Any(x => x.OriginalDefinition.DerivesFrom(namedType));

    private bool IsAnyConstructorToCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel model) =>
        descendantNodes
            .Where(x => Language.SyntaxKind.ObjectCreationExpressions.Contains(Language.Syntax.Kind(x)))
            .Select(x => model.GetSymbolInfo(x).Symbol as IMethodSymbol)
            .WhereNotNull()
            .Any(x => Equals(x.ContainingType.OriginalDefinition, namedType));

    private static bool HasNonPrivateConstructor(IEnumerable<IMethodSymbol> constructors) =>
        constructors.Any(x => x.DeclaredAccessibility != Accessibility.Private);

    private static IEnumerable<IMethodSymbol> Constructors(IEnumerable<ISymbol> members) =>
        members
            .OfType<IMethodSymbol>()
            .Where(x => x.MethodKind == MethodKind.Constructor);

    // Static members and nested types are usable without an instance, so a private constructor is not a defect. At least one of them must be
    // reachable from the outside though, otherwise the whole type is dead code and reporting it is still valuable. Only private constructors
    // exist at this point, so the type cannot be derived from the outside: 'protected' and 'private protected' are as unreachable as 'private'.
    private static bool IsUsableWithoutInstantiation(ICollection<ISymbol> members) =>
        members.Count > 0
        && members.All(x => x is INamedTypeSymbol || x.IsStatic)
        && members.Any(x => x.IsStatic
                            || x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal);

    private static bool IsNonStaticClassWithNoAttributes(INamedTypeSymbol namedType) =>
        namedType is { IsClass: true, IsStatic: false }
        && !namedType.GetAttributes().Any();

    private static bool HasOnlyCandidateConstructors(ICollection<IMethodSymbol> constructors) =>
        constructors.Any()
        && !HasNonPrivateConstructor(constructors)
        && constructors.All(x => !x.GetAttributes().Any());

    private static bool DerivesFromSafeHandle(ITypeSymbol typeSymbol) =>
        typeSymbol.DerivesFrom(KnownType.System_Runtime_InteropServices_SafeHandle);

    protected class ConstructorContext
    {
        public NodeAndModel<TBaseTypeSyntax> NodeAndModel { get; }
        public Location Location { get; }
        public string[] MessageArgs { get; }

        public ConstructorContext(NodeAndModel<TBaseTypeSyntax> nodeAndModel, Location location, params string[] messageArgs)
        {
            NodeAndModel = nodeAndModel;
            Location = location;
            MessageArgs = messageArgs;
        }
    }
}
