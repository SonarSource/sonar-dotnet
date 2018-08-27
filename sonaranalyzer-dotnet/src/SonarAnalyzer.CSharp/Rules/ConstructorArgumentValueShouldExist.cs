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
    public sealed class ConstructorArgumentValueShouldExist : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4260";
        private const string MessageFormat = "Change this 'ConstructorArgumentAttribute' value to match one of the " +
            "existing constructors arguments.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;

                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
                    if (propertySymbol == null)
                    {
                        return;
                    }

                    var constructorArgumentAttribute = GetConstructorArgumentAttributeOrDefault(propertySymbol);
                    if (constructorArgumentAttribute != null &&
                        constructorArgumentAttribute.ConstructorArguments.Length == 1 &&
                        !IsValidConstructorArgumentName(constructorArgumentAttribute, propertyDeclaration))
                    {
                        var attributeSyntax =
                            (AttributeSyntax)constructorArgumentAttribute.ApplicationSyntaxReference.GetSyntax();
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                            attributeSyntax.ArgumentList.Arguments[0].GetLocation()));
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static AttributeData GetConstructorArgumentAttributeOrDefault(IPropertySymbol propertySymbol)
        {
            var attributes = propertySymbol.GetAttributes(KnownType.System_Windows_Markup_ConstructorArgumentAttribute)
                .ToList();

            return attributes.Count == 1
                ? attributes[0]
                : null;
        }

        private static bool IsValidConstructorArgumentName(AttributeData attribute,
            PropertyDeclarationSyntax propertyDeclaration)
        {
            var allConstructorArguments = propertyDeclaration
                .FirstAncestorOrSelf<ClassDeclarationSyntax>()
                .Members
                .OfType<ConstructorDeclarationSyntax>()
                .SelectMany(x => x.ParameterList.Parameters)
                .Select(x => x.Identifier.ValueText)
                .ToHashSet();

            return allConstructorArguments.Contains(attribute.ConstructorArguments[0].Value);
        }
    }
}
