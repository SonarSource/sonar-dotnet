/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public sealed class StaticFieldInGenericClass : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2743";
        private const string MessageFormat =
            "A static field in a generic type is not shared among instances of different close constructed types.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;

                    if (classDeclaration.TypeParameterList == null ||
                        classDeclaration.TypeParameterList.Parameters.Count < 1)
                    {
                        return;
                    }

                    var typeParameterNames =
                        classDeclaration.TypeParameterList.Parameters.Select(p => p.Identifier.ToString()).ToList();

                    var fields = classDeclaration.Members
                        .OfType<FieldDeclarationSyntax>()
                        .Where(f => f.Modifiers.Any(SyntaxKind.StaticKeyword));

                    foreach (var field in fields.Where(field => !HasGenericType(field.Declaration.Type, typeParameterNames, c)))
                    {
                        field.Declaration.Variables.ToList().ForEach(variable =>
                        {
                            CheckMember(variable, variable.Identifier.GetLocation(), typeParameterNames, c);
                        });
                    }

                    var properties = classDeclaration.Members
                        .OfType<PropertyDeclarationSyntax>()
                        .Where(p => p.Modifiers.Any(SyntaxKind.StaticKeyword))
                        .ToList();

                    properties.ForEach(property =>
                    {
                        CheckMember(property, property.Identifier.GetLocation(), typeParameterNames, c);
                    });

                },
                SyntaxKind.ClassDeclaration);
        }

        private static void CheckMember(SyntaxNode root, Location location, IEnumerable<string> typeParameterNames,
            SyntaxNodeAnalysisContext context)
        {
            if (HasGenericType(root, typeParameterNames, context))
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
        }

        private static bool HasGenericType(SyntaxNode root, IEnumerable<string> typeParameterNames,
            SyntaxNodeAnalysisContext context)
        {
            var typeParameters = root.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(identifier => context.SemanticModel.GetSymbolInfo(identifier).Symbol)
                .Where(symbol => symbol != null && symbol.Kind == SymbolKind.TypeParameter)
                .Select(symbol => symbol.Name)
                .ToList();

            return typeParameters.Intersect(typeParameterNames).Any();
        }
    }
}
