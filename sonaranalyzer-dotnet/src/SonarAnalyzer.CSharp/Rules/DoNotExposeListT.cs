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
    public sealed class DoNotExposeListT : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3956";
        private const string MessageFormat = "Refactor this {0} to use a generic collection designed for inheritance.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var baseMethodDeclaration = (BaseMethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(baseMethodDeclaration);

                    if (methodSymbol == null ||
                        !methodSymbol.IsPubliclyAccessible() ||
                        methodSymbol.IsOverride ||
                        !IsOrdinaryMethodOrConstructor(methodSymbol))
                    {
                        return;
                    }

                    var methodType = methodSymbol.IsConstructor() ? "constructor" : "method";

                    if (baseMethodDeclaration is MethodDeclarationSyntax methodDeclaration)
                    {
                        ReportIfListT(methodDeclaration.ReturnType, c, methodType);
                    }

                    baseMethodDeclaration
                        .ParameterList?
                        .Parameters
                        .ToList()
                        .ForEach(p => ReportIfListT(p.Type, c, methodType));
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

                    if (propertySymbol != null &&
                        propertySymbol.IsPubliclyAccessible() &&
                        !propertySymbol.IsOverride)
                    {
                        ReportIfListT(propertyDeclaration.Type, c, "property");
                    }
                },
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;

                    var variableDeclaration = fieldDeclaration.Declaration?.Variables.FirstOrDefault();
                    if (variableDeclaration == null)
                    {
                        return;
                    }

                    var fieldSymbol = c.SemanticModel.GetDeclaredSymbol(variableDeclaration);

                    if (fieldSymbol != null &&
                        fieldSymbol.IsPubliclyAccessible() &&
                        !fieldSymbol.IsOverride)
                    {
                        ReportIfListT(fieldDeclaration.Declaration.Type, c, "field");
                    }
                },
                SyntaxKind.FieldDeclaration);
        }

        private static void ReportIfListT(TypeSyntax typeSyntax, SyntaxNodeAnalysisContext context, string memberType)
        {
            if (typeSyntax != null
                && typeSyntax.IsKnownType(KnownType.System_Collections_Generic_List_T, context.SemanticModel))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, typeSyntax.GetLocation(),
                    messageArgs: memberType));
            }
        }

        private static bool IsOrdinaryMethodOrConstructor(IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Ordinary ||
                   method.MethodKind == MethodKind.Constructor;
        }
    }
}
