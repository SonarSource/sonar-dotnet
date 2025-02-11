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

namespace SonarAnalyzer.CSharp.Rules
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
                    var methodSymbol = c.Model.GetDeclaredSymbol(method);

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
                        && IsNotSemanticallyParams(lastParameter, c.Model))
                    {
                        c.ReportIssue(Rule, paramsKeyword);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool IsNotSemanticallyParams(ParameterSyntax parameter, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(parameter) is { IsParams: false };
    }
}
