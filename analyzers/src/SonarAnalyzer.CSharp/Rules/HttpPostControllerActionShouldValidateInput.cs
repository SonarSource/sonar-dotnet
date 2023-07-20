/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class HttpPostControllerActionShouldValidateInput : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4564";
        private const string MessageFormat = "Enable input validation for this HttpPost method.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    if (methodDeclaration.ParameterList == null
                        || methodDeclaration.ParameterList.Parameters.Count == 0)
                    {
                        // When HttpPost method doesn't have input there is no need to validate them
                        return;
                    }

                    var attributeSymbols = methodDeclaration.AttributeLists
                                                            .SelectMany(list => list.Attributes)
                                                            .Select(x => new NodeAndSymbol<AttributeSyntax, IMethodSymbol>(x, c.SemanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol))
                                                            .Where(tuple => tuple.Symbol != null)
                                                            .ToList();

                    var httpPostAttribute = attributeSymbols.FirstOrDefault(x => x.Symbol.ContainingType.Is(KnownType.System_Web_Mvc_HttpPostAttribute));
                    if (httpPostAttribute == null)
                    {
                        // There is no HttpPost attribute
                        return;
                    }

                    var validateInputAttribute = attributeSymbols.FirstOrDefault(x => x.Symbol.ContainingType.Is(KnownType.System_Web_Mvc_ValidateInputAttribute));

                    if (validateInputAttribute?.Node.ArgumentList == null
                        || validateInputAttribute.Node.ArgumentList.Arguments.Count != 1)
                    {
                        // ValidateInputAttribute not set or has incorrect number of args
                        c.ReportIssue(CreateDiagnostic(Rule, httpPostAttribute.Node.GetLocation()));
                        return;
                    }

                    var constantValue = c.SemanticModel.GetConstantValue(validateInputAttribute.Node.ArgumentList.Arguments[0].Expression);
                    if (!constantValue.HasValue || (constantValue.Value as bool?) != true)
                    {
                        // ValidateInputAttribute is set but with incorrect value
                        c.ReportIssue(CreateDiagnostic(Rule, httpPostAttribute.Node.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
    }
}
