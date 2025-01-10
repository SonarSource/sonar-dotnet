/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
                SyntaxKind.DelegateDeclaration,
                SyntaxKindEx.LocalFunctionStatement,
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

        private static void ReportOnViolation(SonarSyntaxNodeReportingContext context)
        {
            var methodDeclaration = context.Node;
            var parameterList = methodDeclaration.ParameterList();
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
                        context.ReportIssue(Rule, parameter, parameter.Identifier.Text);
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
