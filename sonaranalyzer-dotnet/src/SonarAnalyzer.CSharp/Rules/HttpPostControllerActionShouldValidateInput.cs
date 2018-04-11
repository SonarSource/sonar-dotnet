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
    public sealed class HttpPostControllerActionShouldValidateInput : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4564";
        private const string MessageFormat = "Enable validation on this 'ValidateInput' attribute.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var attributeSyntax = (AttributeSyntax)c.Node;

                    if (attributeSyntax.ArgumentList == null ||
                        attributeSyntax.ArgumentList.Arguments.Count != 1)
                    {
                        return;
                    }

                    var attributeCtorSymbol = c.SemanticModel.GetSymbolInfo(attributeSyntax.Name).Symbol as IMethodSymbol;
                    if (attributeCtorSymbol == null ||
                        !attributeCtorSymbol.ContainingType.Is(KnownType.System_Web_Mvc_ValidateInputAttribute))
                    {
                        return;
                    }

                    var constantValue = c.SemanticModel.GetConstantValue(attributeSyntax.ArgumentList.Arguments[0].Expression);
                    if (!constantValue.HasValue ||
                        (constantValue.Value as bool?) != false)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attributeSyntax.GetLocation()));
                },
                SyntaxKind.Attribute);
        }
    }
}
