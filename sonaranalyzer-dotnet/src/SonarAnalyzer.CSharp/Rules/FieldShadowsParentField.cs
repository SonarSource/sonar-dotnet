/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    [Rule(S2387DiagnosticId)]
    [Rule(S4025DiagnosticId)]
    public sealed class FieldShadowsParentField : SonarDiagnosticAnalyzer
    {
        internal const string S2387DiagnosticId = "S2387";
        private const string S2387MessageFormat = "'{0}' is the name of a field in '{1}'.";

        internal const string S4025DiagnosticId = "S4025";
        private const string S4025MessageFormat = "Rename this field; it may be confused with '{0}' in '{1}'.";

        private static readonly DiagnosticDescriptor s2387 =
            DiagnosticDescriptorBuilder.GetDescriptor(S2387DiagnosticId, S2387MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor s4025 =
            DiagnosticDescriptorBuilder.GetDescriptor(S4025DiagnosticId, S4025MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s2387, s4025);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;

                    fieldDeclaration.Declaration.Variables
                        .SelectMany(v => CheckFields(c.SemanticModel, v))
                        .ToList()
                        .ForEach(d => c.ReportDiagnosticWhenActive(d));
                },
                SyntaxKind.FieldDeclaration);
        }

        private static IEnumerable<Diagnostic> CheckFields(SemanticModel semanticModel, VariableDeclaratorSyntax variableDeclarator)
        {
            if (!(semanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol fieldSymbol))
            {
                yield break;
            }

            var fieldName = fieldSymbol.Name;
            var fieldNameLower = fieldSymbol.Name.ToUpperInvariant();
            var declaringType = fieldSymbol.ContainingType;
            var baseTypes = declaringType.BaseType.GetSelfAndBaseTypes();

            foreach (var baseType in baseTypes)
            {
                var similarFields = baseType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(field => field.DeclaredAccessibility != Accessibility.Private)
                    .Where(field => !field.IsStatic)
                    .Where(field => field.Name.ToUpperInvariant() == fieldNameLower)
                    .ToList();

                if (similarFields.Any(field => field.Name == fieldName))
                {
                    yield return Diagnostic.Create(s2387, variableDeclarator.Identifier.GetLocation(),
                        fieldName, baseType.Name);
                }
                else if (similarFields.Any())
                {
                    yield return Diagnostic.Create(s4025, variableDeclarator.Identifier.GetLocation(),
                        similarFields.First().Name, baseType.Name);
                }
                else
                {
                    // nothing to do
                }
            }
        }
    }
}
