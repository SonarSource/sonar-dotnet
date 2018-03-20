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
    public sealed class MemberShouldNotHaveConflictingTransparencyAttributes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4211";
        private const string MessageFormat = "Change or remove this attribute to be consistent with its container.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                csac =>
                {
                    if (csac.Compilation.IsTest())
                    {
                        return;
                    }

                    var nodesWithSecuritySafeCritical = new Dictionary<SyntaxNode, AttributeSyntax>();
                    var nodesWithSecurityCritical = new Dictionary<SyntaxNode, AttributeSyntax>();

                    csac.RegisterSyntaxNodeActionInNonGenerated(
                        snac =>
                        {
                            var attribute = (AttributeSyntax)snac.Node;

                            var attributeConstructor = snac.SemanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                            if (attributeConstructor == null)
                            {
                                return;
                            }

                            if (attributeConstructor.ContainingType.Is(KnownType.System_Security_SecuritySafeCriticalAttribute))
                            {
                                nodesWithSecuritySafeCritical.Add(attribute.Parent.Parent, attribute);
                            }
                            else if (attributeConstructor.ContainingType.Is(KnownType.System_Security_SecurityCriticalAttribute))
                            {
                                nodesWithSecurityCritical.Add(attribute.Parent.Parent, attribute);
                            }
                            else
                            {
                                // nothing
                            }
                        },
                        SyntaxKind.Attribute);

                    csac.RegisterCompilationEndAction(
                        cac =>
                        {
                            var assemblySecurityCriticalAttribute = cac.Compilation.Assembly.GetAttributes()
                                .FirstOrDefault(a => a.AttributeClass.Is(KnownType.System_Security_SecurityCriticalAttribute));

                            if (assemblySecurityCriticalAttribute != null)
                            {
                                var assemblySecurityLocation = assemblySecurityCriticalAttribute.ApplicationSyntaxReference
                                    .GetSyntax().GetLocation();

                                // All parts declaring the 'SecuritySafeCriticalAttribute' are incorrect since the assembly
                                // itself is marked as 'SecurityCritical'.
                                foreach (var item in nodesWithSecuritySafeCritical)
                                {
                                    cac.ReportDiagnosticWhenActive(Diagnostic.Create(rule, item.Value.GetLocation(),
                                        additionalLocations: new[] { assemblySecurityLocation }));
                                }
                            }
                            else
                            {
                                foreach (var item in nodesWithSecuritySafeCritical)
                                {
                                    var current = item.Key.Parent;
                                    while (current != null)
                                    {
                                        if (nodesWithSecurityCritical.ContainsKey(current))
                                        {
                                            cac.ReportDiagnosticWhenActive(Diagnostic.Create(rule, item.Value.GetLocation(),
                                                additionalLocations: new[] { nodesWithSecurityCritical[current].GetLocation() }));
                                            break;
                                        }

                                        current = current.Parent;
                                    }
                                }
                            }
                        });
                });
        }
    }
}
