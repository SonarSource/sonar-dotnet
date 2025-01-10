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
    public sealed class FinalizerShouldNotBeEmpty : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3880";
        private const string MessageFormat = "Remove this empty finalizer.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var destructorDeclaration = (DestructorDeclarationSyntax)c.Node;
                    if (destructorDeclaration.Body?.Statements.Count == 0
                        && destructorDeclaration.ExpressionBody() == null)
                    {
                        c.ReportIssue(Rule, destructorDeclaration);
                    }
                },
                SyntaxKind.DestructorDeclaration);
    }
}
