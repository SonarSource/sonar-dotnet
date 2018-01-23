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

using System;
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
    public sealed class ShouldImplementExportedInterfaces : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4159";
        private const string MessageFormat = "Implement '{0}' on '{1}' or remove this Export attribute.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classSymbol = c.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)c.Node);
                if (classSymbol == null)
                {
                    return;
                }

                ReportNotExportedTypes(classSymbol, classSymbol.GetAttributes(),
                    d => c.ReportDiagnosticWhenActive(d));
            },
            SyntaxKind.ClassDeclaration);
        }

        private void ReportNotExportedTypes(ITypeSymbol type, IEnumerable<AttributeData> attributes, Action<Diagnostic> report) =>
            attributes
                .Where(IsExportAttribute)
                .Select(a => new { Attribute = a, ExportedType = GetExportedType(a) })
                .Where(x => x.ExportedType != null && !type.DerivesOrImplements(x.ExportedType))
                .Select(x => Diagnostic.Create(rule, GetLocation(x.Attribute), x.ExportedType.Name, type.Name))
                .ToList()
                .ForEach(report);

        private static INamedTypeSymbol GetExportedType(AttributeData attribute)
        {
            var exportedType = attribute.ConstructorArguments
                .ElementAtOrDefault(IndexOfTypeArgument(attribute)).Value as INamedTypeSymbol;

            return exportedType?.TypeKind != TypeKind.Error
                ? exportedType
                : null;
        }

        private static int IndexOfTypeArgument(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 1)
            {
                return 0;
            }
            else if (attribute.ConstructorArguments.Length == 2)
            {
                return 1;
            }
            else
            {
                return -1; // No type argument in this ctor overload or invalid code
            }
        }

        private static bool IsExportAttribute(AttributeData attribute) =>
            attribute.AttributeClass.Is(KnownType.System_ComponentModel_Composition_ExportAttribute);

        private Location GetLocation(AttributeData attributeData) =>
            attributeData.ApplicationSyntaxReference.GetSyntax().GetLocation();
    }
}
