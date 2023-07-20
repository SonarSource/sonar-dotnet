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
    public sealed class CallerInformationParametersShouldBeLast : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3343";
        private const string MessageFormat = "Move '{0}' to the end of the parameter list.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                ReportOnViolation,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKindEx.LocalFunctionStatement);

        private static void ReportOnViolation(SonarSyntaxNodeReportingContext context)
        {
            var methodDeclaration = context.Node;
            var parameterList = LocalFunctionStatementSyntaxWrapper.IsInstance(methodDeclaration)
                ? ((LocalFunctionStatementSyntaxWrapper)methodDeclaration).ParameterList
                : ((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList;

            if (parameterList is null or { Parameters.Count: 0 })
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
            foreach (var parameter in parameterList.Parameters.Reverse())
            {
                if (parameter.AttributeLists.GetAttributes(KnownType.CallerInfoAttributes, context.SemanticModel).Any())
                {
                    if (noCallerInfoParameter != null && HasIdentifier(parameter))
                    {
                        context.ReportIssue(CreateDiagnostic(Rule, parameter.GetLocation(), parameter.Identifier.Text));
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
