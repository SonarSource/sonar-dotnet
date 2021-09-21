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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TypeMemberVisibility : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3059";
        private const string MessageFormat = "Types should not have members with visibility set higher than the type's visibility";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly SyntaxKind[] TypeKinds = { SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.EnumDeclaration, SyntaxKindEx.RecordDeclaration };
        private static readonly SyntaxKind[] MemberDeclarationKinds =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKind.StructDeclaration
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var typeDeclaration = (BaseTypeDeclarationSyntax)c.Node;
                    var secondaryLocations = GetInvalidMemberLocations(typeDeclaration, c.SemanticModel);
                    if (secondaryLocations.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, typeDeclaration.Identifier.GetLocation(), secondaryLocations));
                    }
                },
                TypeKinds);

        private static List<Location> GetInvalidMemberLocations(BaseTypeDeclarationSyntax type, SemanticModel semanticModel)
        {
            var parentType = GetParentType(type);
            if (parentType == null && type.Modifiers.AnyOfKind(SyntaxKind.InternalKeyword))
            {
                return type.DescendantNodes()
                           .Where(node => node.IsAnyKind(MemberDeclarationKinds))
                           .OfType<MemberDeclarationSyntax>()
                           // We skip overridden methods since they need to keep the public visibility (if present)
                           .Where(declaration => declaration.Modifiers().AnyOfKind(SyntaxKind.PublicKeyword)
                                                 && !declaration.Modifiers().AnyOfKind(SyntaxKind.OverrideKeyword)
                                                 && !IsInterfaceImplementation(declaration, semanticModel))
                           .Select(declaration => declaration.Modifiers().Single(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).GetLocation())
                           .ToList();
            }

            return new List<Location>();
        }

        private static bool IsInterfaceImplementation(MemberDeclarationSyntax declaration, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(declaration)?.GetInterfaceMember() != null;

        private static BaseTypeDeclarationSyntax GetParentType(SyntaxNode node) =>
            (BaseTypeDeclarationSyntax)node.Ancestors().FirstOrDefault(x => x.IsAnyKind(TypeKinds));
    }
}
