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
    public sealed class ParameterNamesInPartialMethod : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S927";
        private const string MessageFormat = "Rename parameter '{0}' to '{1}' to match the {2} declaration.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodSyntax = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodSyntax);

                    if (methodSymbol?.PartialImplementationPart != null)
                    {
                        if (methodSymbol.PartialImplementationPart.DeclaringSyntaxReferences
                             .FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax methodImplementationSyntax)
                        {
                            VerifyParameters(c, methodImplementationSyntax, methodSymbol.Parameters, "partial class");
                        }
                    }
                    else if (methodSymbol?.OverriddenMethod != null)
                    {
                        VerifyParameters(c, methodSyntax, methodSymbol.OverriddenMethod.Parameters, "base class");
                    }
                    else
                    {
                        var interfaceMember = methodSymbol.GetInterfaceMember();
                        if (interfaceMember != null)
                        {
                            VerifyParameters(c, methodSyntax, interfaceMember.Parameters, "interface");
                        }
                    }
                },
                SyntaxKind.MethodDeclaration);
        }

        private static void VerifyParameters(SyntaxNodeAnalysisContext context,
            MethodDeclarationSyntax methodSyntax, IList<IParameterSymbol> expectedParameters, string expectedLocation)
        {
            methodSyntax.ParameterList.Parameters
                .Zip(expectedParameters, (actual, expected) => new { actual, expected })
                .Where(x => x.actual.Identifier.ValueText != x.expected.Name)
                .ToList()
                .ForEach(x =>
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                        x.actual.Identifier.GetLocation(),
                        x.actual.Identifier.ValueText, x.expected.Name, expectedLocation));
                });
        }
    }
}
