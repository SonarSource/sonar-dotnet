/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ClassNotInstantiatableBase<TBaseTypeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TBaseTypeSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S3453";
        private const string MessageFormat = "This {0} can't be instantiated; make {1} 'public'.";

        protected readonly DiagnosticDescriptor rule;
        private readonly HashSet<TSyntaxKind> objectCreationSyntaxKinds;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract IEnumerable<ConstructorContext> CollectRemovableDeclarations(INamedTypeSymbol namedType, Compilation compilation, int messageArg);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ClassNotInstantiatableBase()
        {
            objectCreationSyntaxKinds = Language.SyntaxKind.ObjectCreationExpression.ToHashSet();
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(CheckClassWithOnlyUnusedPrivateConstructors, SymbolKind.NamedType);

        private bool IsTypeDeclaration(SyntaxNode node) =>
            Language.IsAnyKind(node, Language.SyntaxKind.ClassDeclaration);

        private bool IsAnyConstructorCalled(INamedTypeSymbol namedType, IEnumerable<ConstructorContext> typeDeclarations) =>
            typeDeclarations
                .Select(typeDeclaration => new
                {
                    typeDeclaration.NodeAndSemanticModel.SemanticModel,
                    DescendantNodes = typeDeclaration.NodeAndSemanticModel.SyntaxNode.DescendantNodes().ToList()
                })
                .Any(descendants =>
                    IsAnyConstructorToCurrentType(descendants.DescendantNodes, namedType, descendants.SemanticModel)
                    || IsAnyNestedTypeExtendingCurrentType(descendants.DescendantNodes, namedType, descendants.SemanticModel));

        private void CheckClassWithOnlyUnusedPrivateConstructors(SymbolAnalysisContext context)
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

            var removableDeclarationsAndErrors = CollectRemovableDeclarations(namedType, context.Compilation, constructors.Count).ToList();

            if (!IsAnyConstructorCalled(namedType, removableDeclarationsAndErrors))
            {
                foreach (var typeDeclaration in removableDeclarationsAndErrors)
                {
                    context.ReportDiagnosticIfNonGenerated(Language.GeneratedCodeRecognizer, typeDeclaration.Diagnostic);
                }
            }
        }

        private bool IsAnyNestedTypeExtendingCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel) =>
            descendantNodes
                .Where(IsTypeDeclaration)
                .Select(x => (semanticModel.GetDeclaredSymbol(x) as ITypeSymbol)?.BaseType)
                .WhereNotNull()
                .Any(baseType => baseType.OriginalDefinition.DerivesFrom(namedType));

        private bool IsAnyConstructorToCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel) =>
            descendantNodes
                .Where(x => Language.Syntax.IsAnyKind(x, objectCreationSyntaxKinds))
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
            public readonly SyntaxNodeAndSemanticModel<TBaseTypeSyntax> NodeAndSemanticModel;
            public readonly Diagnostic Diagnostic;

            public ConstructorContext(SyntaxNodeAndSemanticModel<TBaseTypeSyntax> nodeAndSemanticModel, Diagnostic diagnostic)
            {
                NodeAndSemanticModel = nodeAndSemanticModel;
                Diagnostic = diagnostic;
            }
        }
    }
}
