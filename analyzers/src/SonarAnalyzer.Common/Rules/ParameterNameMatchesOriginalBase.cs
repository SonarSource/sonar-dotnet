/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ParameterNameMatchesOriginalBase<TSyntaxKind, TMethodDeclarationSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TMethodDeclarationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S927";
        private const string MessageFormat = "Rename parameter '{0}' to '{1}' to match the {2} declaration.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract IEnumerable<SyntaxToken> ParameterIdentifiers(TMethodDeclarationSyntax method);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ParameterNameMatchesOriginalBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
                {
                    var methodSyntax = (TMethodDeclarationSyntax)c.Node;
                    if (c.SemanticModel.GetDeclaredSymbol(methodSyntax) is IMethodSymbol methodSymbol && methodSymbol.Parameters.Any())
                    {
                        if (methodSymbol.PartialImplementationPart != null)
                        {
                            if (methodSymbol.PartialImplementationPart.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is TMethodDeclarationSyntax methodImplementationSyntax)
                            {
                                VerifyParameters(c, methodImplementationSyntax, methodSymbol.Parameters, "partial class");
                            }
                        }
                        else if (methodSymbol.OverriddenMethod != null)
                        {
                            VerifyGenericParameters(c, methodSyntax, methodSymbol.Parameters, methodSymbol.OverriddenMethod.OriginalDefinition.Parameters, "base class");
                        }
                        else if (methodSymbol.GetInterfaceMember() is { } interfaceMember)
                        {
                            VerifyGenericParameters(c, methodSyntax, methodSymbol.Parameters, interfaceMember.OriginalDefinition.Parameters, "interface");
                        }
                    }
                },
                SyntaxKinds);

        private void VerifyParameters(SyntaxNodeAnalysisContext context, TMethodDeclarationSyntax methodSyntax, IList<IParameterSymbol> expectedParameters, string expectedLocation)
        {
            foreach (var item in ParameterIdentifiers(methodSyntax)
                                    .Zip(expectedParameters, (actual, expected) => new { actual, expected })
                                    .Where(x => !x.actual.ValueText.Equals(x.expected.Name, Language.NameComparison)))
            {
                context.ReportIssue(Diagnostic.Create(rule, item.actual.GetLocation(), item.actual.ValueText, item.expected.Name, expectedLocation));
            }
        }

        private void VerifyGenericParameters(SyntaxNodeAnalysisContext context, TMethodDeclarationSyntax methodSyntax, IList<IParameterSymbol> actualParameters,
            IList<IParameterSymbol> expectedParameters, string expectedLocation)
        {
            var parameters = ParameterIdentifiers(methodSyntax).ToList();
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var expectedParameter = expectedParameters[i];
                if (!parameter.ValueText.Equals(expectedParameter.Name, Language.NameComparison)
                    && (expectedParameter.Type.Kind != SymbolKind.TypeParameter || actualParameters[i].Type.Kind == SymbolKind.TypeParameter))
                {
                    context.ReportIssue(Diagnostic.Create(rule, parameter.GetLocation(), parameter.ValueText, expectedParameter.Name, expectedLocation));
                }
            }
        }
    }
}
