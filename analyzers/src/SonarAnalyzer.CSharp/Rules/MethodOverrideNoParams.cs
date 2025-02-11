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
    public sealed class MethodOverrideNoParams : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3262";
        private const string MessageFormat = "'params' should not be removed from an override.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.Model.GetDeclaredSymbol(method);

                    if (methodSymbol is not null
                        && methodSymbol.IsOverride
                        && methodSymbol.OverriddenMethod is not null
                        && methodSymbol.OverriddenMethod.Parameters.Any(p => p.IsParams)
                        && !method.ParameterList.Parameters.Last().Modifiers.Any(SyntaxKind.ParamsKeyword))
                    {
                        c.ReportIssue(Rule, method.ParameterList.Parameters.Last());
                    }
                },
                SyntaxKind.MethodDeclaration);
    }
}
