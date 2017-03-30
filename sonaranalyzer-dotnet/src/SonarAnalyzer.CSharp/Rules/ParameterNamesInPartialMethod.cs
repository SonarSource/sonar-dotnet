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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ParameterNamesInPartialMethod : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S927";
        private const string MessageFormat = "Rename parameter '{0}' to '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodSyntax = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodSyntax);

                    if (methodSymbol?.PartialDefinitionPart == null)
                    {
                        return;
                    }

                    var implementationParameters = methodSyntax.ParameterList.Parameters;
                    var definitionParameters = methodSymbol.PartialDefinitionPart.Parameters;

                    for (var i = 0; i < implementationParameters.Count && i < definitionParameters.Length; i++)
                    {
                        var implementationParameter = implementationParameters[i];

                        var definitionParameter = definitionParameters[i];
                        var implementationParameterName = implementationParameter.Identifier.ValueText;
                        if (implementationParameterName != definitionParameter.Name)
                        {
                            c.ReportDiagnostic(Diagnostic.Create(rule,
                                implementationParameter.Identifier.GetLocation(),
                                implementationParameterName, definitionParameter.Name));
                        }
                    }
                },
                SyntaxKind.MethodDeclaration);
        }
    }
}
