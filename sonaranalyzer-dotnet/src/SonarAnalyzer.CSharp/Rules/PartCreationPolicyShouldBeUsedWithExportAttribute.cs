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
    public sealed class PartCreationPolicyShouldBeUsedWithExportAttribute : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4428";
        private const string MessageFormat = "Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute'" +
            " to/from this class definition.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var attribute = (AttributeSyntax)c.Node;

                    if (IsPartCreationPolicyAttribute(attribute) &&
                        attribute.FirstAncestorOrSelf<MemberDeclarationSyntax>() is ClassDeclarationSyntax classDeclaration)
                    {
                        var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                        if (classSymbol == null ||
                            classSymbol.GetAttributes(KnownType.System_ComponentModel_Composition_ExportAttribute).Any() ||
                            classSymbol.GetSelfAndBaseTypes().Any(s => s.GetAttributes(KnownType.System_ComponentModel_Composition_InheritedExportAttribute).Any()))
                        {
                            return;
                        }

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attribute.GetLocation()));
                    }

                    bool IsPartCreationPolicyAttribute(AttributeSyntax attributeSyntax) =>
                        c.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol attributeSymbol &&
                        attributeSymbol.ContainingType.Is(KnownType.System_ComponentModel_Composition_PartCreationPolicyAttribute);
                },
                SyntaxKind.Attribute);
        }
    }
}
