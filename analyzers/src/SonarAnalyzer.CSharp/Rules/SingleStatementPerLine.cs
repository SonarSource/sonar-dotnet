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
    public sealed class SingleStatementPerLine : SingleStatementPerLineBase<SyntaxKind, StatementSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;

        protected override bool StatementShouldBeExcluded(StatementSyntax statement) =>
            StatementIsBlock(statement) || StatementIsSingleInLambda(statement);

        private static bool StatementIsSingleInLambda(StatementSyntax st) =>
            !st.DescendantNodes().OfType<StatementSyntax>().Any()
            && st.Parent is BlockSyntax parentBlock
            && parentBlock.Statements.Count <= 1
            && parentBlock.Parent is AnonymousFunctionExpressionSyntax;

        private static bool StatementIsBlock(StatementSyntax st) =>
            st is BlockSyntax;
    }
}
