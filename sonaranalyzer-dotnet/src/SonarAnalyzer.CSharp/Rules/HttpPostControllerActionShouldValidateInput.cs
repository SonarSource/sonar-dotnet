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
    public sealed class HttpPostControllerActionShouldValidateInput : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4564";
        private const string MessageFormat = "Enable input validation for this HttpPost method.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    if (methodDeclaration.ParameterList == null ||
                        methodDeclaration.ParameterList.Parameters.Count == 0)
                    {
                        // When HttpPost method doesn't have input there is no need to validate them
                        return;
                    }

                    var attributeSymbols = methodDeclaration.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Select(a => a.ToSyntaxWithSymbol(c.SemanticModel.GetSymbolInfo(a).Symbol as IMethodSymbol))
                        .Where(tuple => tuple.Symbol != null)
                        .ToList();

                    var httpPostAttribute = attributeSymbols.FirstOrDefault(tuple =>
                        tuple.Symbol.ContainingType.Is(KnownType.System_Web_Mvc_HttpPostAttribute));
                    if (httpPostAttribute == null)
                    {
                        // There is no HttpPost attribute
                        return;
                    }

                    var validateInputAttribute = attributeSymbols.FirstOrDefault(a =>
                        a.Symbol.ContainingType.Is(KnownType.System_Web_Mvc_ValidateInputAttribute));

                    if (validateInputAttribute == null ||
                        validateInputAttribute.Syntax.ArgumentList == null ||
                        validateInputAttribute.Syntax.ArgumentList.Arguments.Count != 1)
                    {
                        // ValidateInputAttribute not set or has incorrect number of args
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, httpPostAttribute.Syntax.GetLocation()));
                        return;
                    }

                    var constantValue = c.SemanticModel.GetConstantValue(
                        validateInputAttribute.Syntax.ArgumentList.Arguments[0].Expression);
                    if (!constantValue.HasValue ||
                        (constantValue.Value as bool?) != true)
                    {
                        // ValidateInputAttribute is set but with incorrect value
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, httpPostAttribute.Syntax.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
        }
    }
}
