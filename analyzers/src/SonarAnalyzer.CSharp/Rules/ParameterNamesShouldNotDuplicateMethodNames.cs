/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParameterNamesShouldNotDuplicateMethodNames : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3872";
        private const string MessageFormat = "Rename the parameter '{0}' so that it does not duplicate the method name.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;
                CheckMethodParameters(c, method.Identifier, method.ParameterList);
            },
            SyntaxKind.MethodDeclaration);

            context.RegisterNodeAction(c =>
            {
                var localFunction = (LocalFunctionStatementSyntaxWrapper)c.Node;
                CheckMethodParameters(c, localFunction.Identifier, localFunction.ParameterList);
            },
            SyntaxKindEx.LocalFunctionStatement);
        }

        private static void CheckMethodParameters(SonarSyntaxNodeReportingContext context, SyntaxToken identifier, ParameterListSyntax parameterList)
        {
            var methodName = identifier.ToString();
            foreach (var parameter in parameterList.Parameters.Select(p => p.Identifier))
            {
                var parameterName = parameter.ToString();
                if (string.Equals(parameterName, methodName, StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportIssue(rule, parameter, [identifier.ToSecondaryLocation()], parameterName);
                }
            }
        }
    }
}
