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
    public sealed class MethodOverrideAddsParams : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3600";
        private const string MessageFormat = "'params' should be removed from this override.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(method);

                    if (methodSymbol is not { IsOverride: true }
                        || methodSymbol.OverriddenMethod == null)
                    {
                        return;
                    }

                    var lastParameter = method.ParameterList.Parameters.LastOrDefault();
                    if (lastParameter == null)
                    {
                        return;
                    }

                    var paramsKeyword = lastParameter.Modifiers.FirstOrDefault(modifier => modifier.IsKind(SyntaxKind.ParamsKeyword));
                    if (paramsKeyword != default
                        && IsNotSemanticallyParams(lastParameter, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, paramsKeyword.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool IsNotSemanticallyParams(ParameterSyntax parameter, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(parameter) is { IsParams: false };
    }
}
