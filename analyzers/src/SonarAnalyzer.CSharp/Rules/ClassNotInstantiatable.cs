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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ClassNotInstantiatable : ClassNotInstantiatableBase
    {
        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(CheckClassWithOnlyUnusedPrivateConstructors, SymbolKind.NamedType);

        protected override bool IsTypeDeclaration(SyntaxNode node) =>
            node.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordDeclaration);

        protected override bool IsObjectCreation(SyntaxNode node) =>
            node.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

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

            var typeDeclarations = new CSharpRemovableDeclarationCollector(namedType, context.Compilation).TypeDeclarations;

            if (!IsAnyConstructorCalled(namedType, typeDeclarations))
            {
                var message = constructors.Count > 1
                    ? "at least one of its constructors"
                    : "its constructor";

                foreach (var typeDeclaration in typeDeclarations)
                {
                    var declarationKind = typeDeclaration.SyntaxNode.IsKind(SyntaxKind.ClassDeclaration)
                        ? "class"
                        : "record";
                    context.ReportDiagnosticIfNonGenerated(
                        Diagnostic.Create(Rule, typeDeclaration.SyntaxNode.Identifier.GetLocation(), declarationKind, message));
                }
            }
        }
    }
}
