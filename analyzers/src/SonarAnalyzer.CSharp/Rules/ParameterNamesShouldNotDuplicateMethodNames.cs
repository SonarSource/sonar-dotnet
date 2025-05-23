﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
