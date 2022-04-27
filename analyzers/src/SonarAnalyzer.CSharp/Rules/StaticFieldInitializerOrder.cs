/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StaticFieldInitializerOrder : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3263";
        private const string MessageFormat = "Move this field's initializer into a static constructor.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        return;
                    }
                    var variables = fieldDeclaration.Declaration.Variables.Where(v => v.Initializer != null).ToArray();
                    if (variables.Length == 0)
                    {
                        return;
                    }
                    var containingType = c.SemanticModel.GetDeclaredSymbol(variables[0]).ContainingType;
                    var typeDeclaration = fieldDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>(
                        x => x.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordClassDeclaration));

                    foreach (var variable in variables)
                    {
                        var identifierFieldMappings = IdentifierFieldMappings(variable, containingType, c.SemanticModel);
                        var identifierTypeMappings = IdentifierTypeMappings(identifierFieldMappings);
                        var usedTypeDeclarations = identifierTypeMappings.Select(x => x.TypeDeclaration);
                        var sameTypeIdentifiersAfterThis = identifierTypeMappings
                            .Where(x => x.TypeDeclaration == typeDeclaration)
                            .Where(x => !x.Field.IsConst)
                            .Where(x => x.Field.DeclaringSyntaxReferences.First().Span.Start > variable.SpanStart);

                        if (usedTypeDeclarations.Any(x => x != typeDeclaration) || sameTypeIdentifiersAfterThis.Any())
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, variable.Initializer.GetLocation()));
                        }
                    }
                },
                SyntaxKind.FieldDeclaration);

        private static IdentifierTypeDeclarationMapping[] IdentifierTypeMappings(IEnumerable<IFieldSymbol> identifierFields) =>
            identifierFields
                .Select(x => new IdentifierTypeDeclarationMapping(x, GetTypeDeclaration(x)))
                .Where(x => x.TypeDeclaration != null)
                .ToArray();

        private static IEnumerable<IFieldSymbol> IdentifierFieldMappings(VariableDeclaratorSyntax variable, INamedTypeSymbol containingType, SemanticModel semanticModel)
        {
            foreach (var identifier in variable.Initializer.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (containingType.MemberNames.Contains(identifier.Identifier.ValueText)
                    && semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol { IsConst: false, IsStatic: true } field
                    && containingType.Equals(field.ContainingType)
                    && semanticModel.GetEnclosingSymbol(identifier.SpanStart) is IFieldSymbol enclosingSymbol
                    && enclosingSymbol.ContainingType.Equals(field.ContainingType))
                {
                    yield return field;
                }
            }
        }

        private static TypeDeclarationSyntax GetTypeDeclaration(IFieldSymbol field) =>
            field.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().FirstAncestorOrSelf<TypeDeclarationSyntax>();

        private sealed record IdentifierTypeDeclarationMapping(IFieldSymbol Field, TypeDeclarationSyntax TypeDeclaration);
    }
}
