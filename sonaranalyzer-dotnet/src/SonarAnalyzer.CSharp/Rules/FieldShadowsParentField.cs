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
    public class FieldShadowsParentField : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2387";
        private const string MessageFormat = "{0}";
        internal const string MessageMatch = "'{0}' is the name of a field in '{1}'.";
        internal const string MessageSimilar = "'{0}' differs only by case from '{2}' in '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax) c.Node;
                    foreach (var variableDeclarator in fieldDeclaration.Declaration.Variables)
                    {
                        CheckField(c, variableDeclarator);
                    }
                },
                SyntaxKind.FieldDeclaration);
        }

        private static void CheckField(SyntaxNodeAnalysisContext context, VariableDeclaratorSyntax variableDeclarator)
        {
            var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variableDeclarator) as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return;
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
                    .Where(field => field.Name.ToUpperInvariant() == fieldNameLower)
                    .ToList();

                if (similarFields.Any(field => field.Name == fieldName))
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, variableDeclarator.Identifier.GetLocation(),
                        string.Format(MessageMatch, fieldName, baseType.Name)));
                    return;
                }

                if (similarFields.Any())
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, variableDeclarator.Identifier.GetLocation(),
                        string.Format(MessageSimilar, fieldName, baseType.Name, similarFields.First().Name)));
                    return;
                }
            }
        }
    }
}
