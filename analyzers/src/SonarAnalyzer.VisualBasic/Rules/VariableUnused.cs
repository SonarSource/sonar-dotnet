/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class VariableUnused : VariableUnusedBase
    {
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartAction(cbc =>
            {
                var collector = new UnusedLocalsCollector();

                cbc.RegisterNodeAction(collector.CollectDeclarations, SyntaxKind.LocalDeclarationStatement);
                cbc.RegisterNodeAction(collector.CollectUsages, SyntaxKind.IdentifierName);
                cbc.RegisterCodeBlockEndAction(c => collector.ReportUnusedVariables(c, Rule));
            });

        private class UnusedLocalsCollector : UnusedLocalsCollectorBase<LocalDeclarationStatementSyntax>
        {
            protected override IEnumerable<SyntaxNode> GetDeclaredVariables(LocalDeclarationStatementSyntax localDeclaration) =>
                localDeclaration.Declarators.SelectMany(d => d.Names);
        }
    }
}
