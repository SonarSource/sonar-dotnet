/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
{
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
            Language.Syntax.IsAnyKind(node, Language.SyntaxKind.ClassAndRecordDeclarations);

        private bool IsAnyConstructorCalled(INamedTypeSymbol namedType, IEnumerable<ConstructorContext> typeDeclarations) =>
            typeDeclarations
                .Select(typeDeclaration => new
                {
                    typeDeclaration.NodeAndModel.Model,
                    DescendantNodes = typeDeclaration.NodeAndModel.Node.DescendantNodes().ToList()
                })
                .Any(descendants =>
                    IsAnyConstructorToCurrentType(descendants.DescendantNodes, namedType, descendants.Model)
                    || IsAnyNestedTypeExtendingCurrentType(descendants.DescendantNodes, namedType, descendants.Model));

        private void CheckClassWithOnlyUnusedPrivateConstructors(SonarSymbolReportingContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            if (!IsNonStaticClassWithNoAttributes(namedType) || DerivesFromSafeHandle(namedType))
            {
                return;
            }

            var members = namedType.GetMembers();
            var constructors = GetConstructors(members).Where(x => !x.IsImplicitlyDeclared).ToList();

            if (!HasOnlyCandidateConstructors(constructors) || HasOnlyStaticMembers(members.Except(constructors).ToList()))
            {
                return;
            }

            var messageArg = constructors.Count > 1 ? "at least one of its constructors" : "its constructor";
            var removableDeclarationsAndErrors = CollectRemovableDeclarations(namedType, context.Compilation, messageArg).ToList();

            if (!IsAnyConstructorCalled(namedType, removableDeclarationsAndErrors))
            {
                foreach (var typeDeclaration in removableDeclarationsAndErrors)
                {
                    context.ReportIssue(Language.GeneratedCodeRecognizer, typeDeclaration.Diagnostic);
                }
            }
        }

        private bool IsAnyNestedTypeExtendingCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel) =>
            descendantNodes
                .Where(IsClassTypeDeclaration)
                .Select(x => (semanticModel.GetDeclaredSymbol(x) as ITypeSymbol)?.BaseType)
                .WhereNotNull()
                .Any(baseType => baseType.OriginalDefinition.DerivesFrom(namedType));

        private bool IsAnyConstructorToCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel) =>
            descendantNodes
                .Where(x => Language.Syntax.IsAnyKind(x, Language.SyntaxKind.ObjectCreationExpressions))
                .Select(ctor => semanticModel.GetSymbolInfo(ctor).Symbol as IMethodSymbol)
                .WhereNotNull()
                .Any(ctor => Equals(ctor.ContainingType.OriginalDefinition, namedType));

        private static bool HasNonPrivateConstructor(IEnumerable<IMethodSymbol> constructors) =>
            constructors.Any(method => method.DeclaredAccessibility != Accessibility.Private);

        private static IEnumerable<IMethodSymbol> GetConstructors(IEnumerable<ISymbol> members) =>
            members
                .OfType<IMethodSymbol>()
                .Where(method => method.MethodKind == MethodKind.Constructor);

        private static bool HasOnlyStaticMembers(ICollection<ISymbol> members) =>
            members.Any()
            && members.All(member => member.IsStatic);

        private static bool IsNonStaticClassWithNoAttributes(INamedTypeSymbol namedType) =>
            namedType.IsClass()
            && !namedType.IsStatic
            && !namedType.GetAttributes().Any();

        private static bool HasOnlyCandidateConstructors(ICollection<IMethodSymbol> constructors) =>
            constructors.Any()
            && !HasNonPrivateConstructor(constructors)
            && constructors.All(c => !c.GetAttributes().Any());

        private static bool DerivesFromSafeHandle(ITypeSymbol typeSymbol) =>
            typeSymbol.DerivesFrom(KnownType.System_Runtime_InteropServices_SafeHandle);

        protected class ConstructorContext
        {
            public NodeAndModel<TBaseTypeSyntax> NodeAndModel { get; }
            public Diagnostic Diagnostic { get; }

            public ConstructorContext(NodeAndModel<TBaseTypeSyntax> nodeAndModel, Diagnostic diagnostic)
            {
                NodeAndModel = nodeAndModel;
                Diagnostic = diagnostic;
            }
        }
    }
}
