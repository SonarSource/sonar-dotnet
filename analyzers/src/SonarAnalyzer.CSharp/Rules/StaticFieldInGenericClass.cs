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
    public sealed class StaticFieldInGenericClass : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2743";
        private const string MessageFormat = "A static field in a generic type is not shared among instances of different close constructed types.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (c.ContainingSymbol.Kind != SymbolKind.NamedType
                        || typeDeclaration.TypeParameterList == null
                        || typeDeclaration.TypeParameterList.Parameters.Count < 1)
                    {
                        return;
                    }
                    var typeParameterNames = typeDeclaration.TypeParameterList.Parameters.Select(x => x.Identifier.ToString()).ToArray();
                    var fields = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().Where(f => f.Modifiers.Any(SyntaxKind.StaticKeyword));
                    foreach (var field in fields.Where(field => !HasGenericType(field.Declaration.Type, typeParameterNames, c)))
                    {
                        field.Declaration.Variables.ToList().ForEach(variable => CheckMember(variable, variable.Identifier.GetLocation(), typeParameterNames, c));
                    }
                    foreach (var property in typeDeclaration.Members.OfType<PropertyDeclarationSyntax>().Where(x => x.Modifiers.Any(SyntaxKind.StaticKeyword)))
                    {
                        CheckMember(property, property.Identifier.GetLocation(), typeParameterNames, c);
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordClassDeclaration);

        private static void CheckMember(SyntaxNode root, Location location, string[] typeParameterNames, SyntaxNodeAnalysisContext context)
        {
            if (!HasGenericType(root, typeParameterNames, context))
            {
                context.ReportIssue(Diagnostic.Create(Rule, location));
            }
        }

        private static bool HasGenericType(SyntaxNode root, string[] typeParameterNames, SyntaxNodeAnalysisContext context) =>
            root.DescendantNodesAndSelf()
                .OfType<IdentifierNameSyntax>()
                .Any(x => typeParameterNames.Contains(x.Identifier.Value) && context.SemanticModel.GetSymbolInfo(x).Symbol is { Kind: SymbolKind.TypeParameter });
    }
}
