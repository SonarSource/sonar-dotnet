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
    public abstract class ParameterNamesInPartialMethodBase<TSyntaxKind, TMethodDeclarationSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TMethodDeclarationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S927";
        private const string MessageFormat = "Rename parameter '{0}' to '{1}' to match the {2} declaration.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade LanguageFacade { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract IEnumerable<SyntaxToken> ParameterIdentifiers(TMethodDeclarationSyntax method);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ParameterNamesInPartialMethodBase(System.Resources.ResourceManager rspecResources) =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(LanguageFacade.GeneratedCodeRecognizer, c =>
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
                            VerifyParameters(c, methodSyntax, methodSymbol.OverriddenMethod.Parameters, "base class");
                        }
                        else if (methodSymbol.GetInterfaceMember() is { } interfaceMember)
                        {
                            VerifyParameters(c, methodSyntax, interfaceMember.Parameters, "interface");
                        }
                    }
                },
                SyntaxKinds);

        private void VerifyParameters(SyntaxNodeAnalysisContext context, TMethodDeclarationSyntax methodSyntax, IList<IParameterSymbol> expectedParameters, string expectedLocation)
        {
            foreach (var item in ParameterIdentifiers(methodSyntax)
                                    .Zip(expectedParameters, (actual, expected) => new { actual, expected })
                                    .Where(x => !x.actual.ValueText.Equals(x.expected.Name, LanguageFacade.NameComparison)))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, item.actual.GetLocation(), item.actual.ValueText, item.expected.Name, expectedLocation));
            }
        }
    }
}
