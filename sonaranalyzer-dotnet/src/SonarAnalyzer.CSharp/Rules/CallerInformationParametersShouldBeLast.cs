
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
    public sealed class CallerInformationParametersShouldBeLast : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3343";
        private const string MessageFormat = "Move '{0}' to the end of the parameter list.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                ReportOnViolation,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);
        }

        private static void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (BaseMethodDeclarationSyntax)context.Node;
            if (methodDeclaration.ParameterList == null)
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null ||
                methodSymbol.IsOverride ||
                methodSymbol.GetInterfaceMember() != null)
            {
                return;
            }

            ParameterSyntax noCallerInfoParameter = null;
            foreach (var parameter in methodDeclaration.ParameterList.Parameters.Reverse())
            {
                if (parameter.AttributeLists.GetAttributes(KnownType.CallerInfoAttributes, context.SemanticModel).Any())
                {
                    if (noCallerInfoParameter != null &&
                        HasIdentifier(parameter))
                    {
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, parameter.GetLocation(),
                            parameter.Identifier.Text));
                    }
                }
                else
                {
                    noCallerInfoParameter = parameter;
                }
            }
        }

        private static bool HasIdentifier(ParameterSyntax parameter) =>
            !string.IsNullOrEmpty(parameter.Identifier.Text);
    }
}
