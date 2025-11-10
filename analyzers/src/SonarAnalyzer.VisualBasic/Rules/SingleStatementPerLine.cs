/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class SingleStatementPerLine : SingleStatementPerLineBase<SyntaxKind, StatementSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;

        protected override bool StatementShouldBeExcluded(StatementSyntax statement) =>
            StatementIsBlock(statement) || StatementIsSingleInLambda(statement);

        private static bool StatementIsSingleInLambda(StatementSyntax st) =>
            !st.DescendantNodes().OfType<StatementSyntax>().Any()
            && st.Parent is MultiLineLambdaExpressionSyntax multiline
            && multiline.Statements.Count == 1;

        private static bool StatementIsBlock(StatementSyntax st) =>
            st is NamespaceBlockSyntax
            or TypeBlockSyntax
            or EnumBlockSyntax
            or MethodBlockBaseSyntax
            or PropertyBlockSyntax
            or EventBlockSyntax
            or DoLoopBlockSyntax
            or WhileBlockSyntax
            or ForOrForEachBlockSyntax
            or MultiLineIfBlockSyntax
            or ElseStatementSyntax
            or SyncLockBlockSyntax
            or TryBlockSyntax
            or UsingBlockSyntax
            or WithBlockSyntax
            or MethodBaseSyntax
            or InheritsOrImplementsStatementSyntax
            or SelectBlockSyntax;
    }
}
