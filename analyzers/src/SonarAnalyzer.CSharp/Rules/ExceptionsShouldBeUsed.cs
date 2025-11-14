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
    public sealed class ExceptionsShouldBeUsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3984";
        private const string MessageFormat = "Throw this exception or remove this useless statement.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                var parent = objectCreation.GetFirstNonParenthesizedParent();
                if (parent.IsKind(SyntaxKind.ExpressionStatement)
                    && c.Model.GetSymbolInfo(objectCreation.Type).Symbol is INamedTypeSymbol createdObjectType
                    && createdObjectType.DerivesFrom(KnownType.System_Exception))
                {
                    c.ReportIssue(Rule, objectCreation);
                }
            },
            SyntaxKind.ObjectCreationExpression);
    }
}
