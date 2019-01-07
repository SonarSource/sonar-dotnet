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
    [Rule(DiagnosticId)]
    public sealed class GenericTypeParametersRequired : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4018";
        private const string MessageFormat = "Refactor this method to have parameters matching all the type parameters.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var methodDeclaration = c.Node as MethodDeclarationSyntax;

                    var typeParameters = methodDeclaration
                            .TypeParameterList
                            ?.Parameters
                            .Select(p => c.SemanticModel.GetDeclaredSymbol(p));

                    if (typeParameters == null)
                    {
                        return;
                    }

                    var argumentTypes = methodDeclaration
                            .ParameterList
                            .Parameters
                            .Select(p => c.SemanticModel.GetDeclaredSymbol(p)?.Type);

                    var typeParametersInArguments = new HashSet<ITypeParameterSymbol>();
                    foreach (var argumentType in argumentTypes)
                    {
                        AddTypeParameters(argumentType, typeParametersInArguments);
                    }

                    if (typeParameters.Except(typeParametersInArguments).Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
        }

        private void AddTypeParameters(ITypeSymbol argumentSymbol, ISet<ITypeParameterSymbol> set)
        {
            var localArgumentSymbol = argumentSymbol;

            if (localArgumentSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                localArgumentSymbol = arrayTypeSymbol.ElementType;
            }

            if (localArgumentSymbol.Is(TypeKind.TypeParameter))
            {
                set.Add(localArgumentSymbol as ITypeParameterSymbol);
            }

            if (localArgumentSymbol is INamedTypeSymbol namedSymbol)
            {
                foreach (var typeParam in namedSymbol.TypeArguments)
                {
                    AddTypeParameters(typeParam, set);
                }
            }
        }
    }
}
