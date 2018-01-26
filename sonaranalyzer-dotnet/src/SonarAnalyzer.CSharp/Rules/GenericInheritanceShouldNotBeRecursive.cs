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
    public sealed class GenericInheritanceShouldNotBeRecursive : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3464";
        private const string MessageFormat = "Refactor this class so that the generic inheritance chain is not recursive.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    if (!IsGenericType(classSymbol) ||
                        !IsGenericType(classSymbol.BaseType))
                    {
                        return;
                    }

                    if (HasGenericArgumentsOfType(classSymbol.BaseType, classSymbol))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.GetLocation()));
                    }
                },
                SyntaxKind.ClassDeclaration);
        }

        private static bool HasGenericArgumentsOfType(INamedTypeSymbol typeSymbol, INamedTypeSymbol argumentType)
        {
            bool ProcessGenericArguments(IEnumerable<ITypeSymbol> types)
            {
                foreach (var type in types.OfType<INamedTypeSymbol>())
                {
                    if (type.OriginalDefinition.Equals(argumentType) &&
                        HasSubstitutedTypeArguments(type))
                    {
                        return true;
                    }
                    if (ProcessGenericArguments(type.TypeArguments))
                    {
                        return true;
                    }
                }
                return false;
            }

            return ProcessGenericArguments(typeSymbol.TypeArguments);
        }

        private static bool IsGenericType(INamedTypeSymbol type) => type != null && type.IsGenericType;

        private static bool HasSubstitutedTypeArguments(INamedTypeSymbol type) =>
            type.TypeArguments.OfType<INamedTypeSymbol>().Any();
    }
}
