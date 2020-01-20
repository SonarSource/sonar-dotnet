/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class StaticFieldInitializerOrder : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3263";
        private const string MessageFormat = "Move this field's initializer into a static constructor.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    var variables = fieldDeclaration.Declaration.Variables;
                    var typeDeclaration = fieldDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>(
                        sn => sn.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration));

                    if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                        !variables.Any())
                    {
                        return;
                    }

                    var containingType = c.SemanticModel.GetDeclaredSymbol(variables.First()).ContainingType;

                    foreach (var variable in variables.Where(v => v.Initializer != null))
                    {
                        var identifierFieldMappings = GetIdentifierFieldMappings(variable, containingType, c.SemanticModel);
                        var identifierTypeMappings = GetIdentifierTypeMappings(identifierFieldMappings);

                        var usedTypeDeclarations = identifierTypeMappings
                            .Select(mapping => mapping.TypeDeclaration);
                        var isAnyInDifferentType = usedTypeDeclarations.Any(cl => cl != typeDeclaration);

                        var sameTypeIdentifiersAfterThis = identifierTypeMappings
                            .Where(mapping => mapping.TypeDeclaration == typeDeclaration)
                            .Where(mapping => !mapping.Identifier.Field.IsConst)
                            .Where(mapping => mapping.Identifier.Field.DeclaringSyntaxReferences.First().Span.Start > variable.SpanStart);
                        var isAnyAfterInSameType = sameTypeIdentifiersAfterThis.Any();

                        if (isAnyInDifferentType ||
                            isAnyAfterInSameType)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, variable.Initializer.GetLocation()));
                        }
                    }
                },
                SyntaxKind.FieldDeclaration);
        }

        private static List<IdentifierTypeDeclarationMapping> GetIdentifierTypeMappings(IEnumerable<IdentifierFieldMapping> identifierFieldMappings)
        {
            return identifierFieldMappings
                .Select(i => new IdentifierTypeDeclarationMapping
                {
                    Identifier = i,
                    TypeDeclaration = GetTypeDeclaration(i.Field)
                })
                .Where(mapping => mapping.TypeDeclaration != null)
                .ToList();
        }

        private static IEnumerable<IdentifierFieldMapping> GetIdentifierFieldMappings(VariableDeclaratorSyntax variable,
            INamedTypeSymbol containingType, SemanticModel semanticModel)
        {
            return variable.Initializer.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(identifier =>
                {
                    var field = semanticModel.GetSymbolInfo(identifier).Symbol as IFieldSymbol;
                    var enclosingSymbol = semanticModel.GetEnclosingSymbol(identifier.SpanStart);
                    return new IdentifierFieldMapping
                    {
                        Identifier = identifier,
                        Field = field,
                        IsRelevant = field != null &&
                            !field.IsConst &&
                            field.IsStatic &&
                            containingType.Equals(field.ContainingType) &&
                            enclosingSymbol is IFieldSymbol &&
                            enclosingSymbol.ContainingType.Equals(field.ContainingType)
                    };
                })
                .Where(identifier => identifier.IsRelevant);
        }

        private static TypeDeclarationSyntax GetTypeDeclaration(IFieldSymbol field)
        {
            var reference = field.DeclaringSyntaxReferences.FirstOrDefault();
            return reference?.SyntaxTree == null
                ? null
                : reference.GetSyntax().FirstAncestorOrSelf<TypeDeclarationSyntax>();
        }

        private class IdentifierFieldMapping
        {
            public IdentifierNameSyntax Identifier { get; set; }
            public IFieldSymbol Field { get; set; }
            public bool IsRelevant { get; set; }
        }

        private class IdentifierTypeDeclarationMapping
        {
            public IdentifierFieldMapping Identifier { get; set; }
            public TypeDeclarationSyntax TypeDeclaration { get; set; }
        }
    }
}
