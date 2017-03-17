/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public class ClassNotInstantiatable : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3453";
        private const string MessageFormat = "This class can't be instantiated; make {0} 'public'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(CheckClassWithOnlyUnusedPrivateConstructors, SymbolKind.NamedType);
        }

        private static void CheckClassWithOnlyUnusedPrivateConstructors(SymbolAnalysisContext context)
        {
            var namedType = context.Symbol as INamedTypeSymbol;
            if (!IsNonStaticClassWithNoAttributes(namedType))
            {
                return;
            }

            var members = namedType.GetMembers();
            var constructors = GetConstructors(members).ToList();

            if (!HasOnlyCandidateConstructors(constructors) ||
                HasOnlyStaticMembers(members.Except(constructors).ToList()))
            {
                return;
            }

            var typeDeclarations = new RemovableDeclarationCollector(namedType, context.Compilation).TypeDeclarations;

            if (!IsAnyConstructorCalled(namedType, typeDeclarations))
            {
                var message = constructors.Count > 1
                    ? "at least one of its constructors"
                    : "its constructor";

                foreach (var classDeclaration in typeDeclarations)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, classDeclaration.SyntaxNode.Identifier.GetLocation(),
                        message));
                }
            }
        }

        private static bool HasOnlyCandidateConstructors(ICollection<IMethodSymbol> constructors)
        {
            return constructors.Any() &&
                !HasNonPrivateConstructor(constructors) &&
                constructors.All(c => !c.GetAttributes().Any());
        }

        private static bool IsNonStaticClassWithNoAttributes(INamedTypeSymbol namedType)
        {
            return namedType.IsClass() &&
                !namedType.IsStatic &&
                !namedType.GetAttributes().Any();
        }

        private static bool IsAnyConstructorCalled(INamedTypeSymbol namedType,
            IEnumerable<SyntaxNodeSemanticModelTuple<BaseTypeDeclarationSyntax>> typeDeclarations)
        {
            return typeDeclarations
                .Select(classDeclaration => new
                {
                    SemanticModel = classDeclaration.SemanticModel,
                    DescendantNodes = classDeclaration.SyntaxNode.DescendantNodes().ToList()
                })
                .Any(descendants =>
                    IsAnyConstructorToCurrentType(descendants.DescendantNodes, namedType, descendants.SemanticModel) ||
                    IsAnyNestedTypeExtendingCurrentType(descendants.DescendantNodes, namedType, descendants.SemanticModel));
        }

        private static bool HasNonPrivateConstructor(IEnumerable<IMethodSymbol> constructors)
        {
            return constructors.Any(method => method.DeclaredAccessibility != Accessibility.Private);
        }

        private static bool IsAnyNestedTypeExtendingCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType,
            SemanticModel semanticModel)
        {
            return descendantNodes
                .OfType<ClassDeclarationSyntax>()
                .Select(c => semanticModel.GetDeclaredSymbol(c)?.BaseType)
                .Any(baseType => baseType != null && baseType.OriginalDefinition.DerivesFrom(namedType));
        }

        private static bool IsAnyConstructorToCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType,
            SemanticModel semanticModel)
        {
            return descendantNodes
                .OfType<ObjectCreationExpressionSyntax>()
                .Select(ctor => semanticModel.GetSymbolInfo(ctor).Symbol as IMethodSymbol)
                .Where(m => m != null)
                .Any(ctor => Equals(ctor.ContainingType?.OriginalDefinition, namedType));
        }

        private static IEnumerable<IMethodSymbol> GetConstructors(IEnumerable<ISymbol> members)
        {
            return members
                .OfType<IMethodSymbol>()
                .Where(method => method.MethodKind == MethodKind.Constructor);
        }

        private static bool HasOnlyStaticMembers(ICollection<ISymbol> members)
        {
            return members.Any() &&
                members.All(member => member.IsStatic);
        }
    }
}
