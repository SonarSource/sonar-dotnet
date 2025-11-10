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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConditionalsShouldStartOnNewLine : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3972";
        private const string MessageFormat = "Move this 'if' to a new line or add the missing 'else'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var ifKeyword = ((IfStatementSyntax)c.Node).IfKeyword;

                if (TryGetPreviousTokenInSameLine(ifKeyword, out var previousTokenInSameLine) &&
                    previousTokenInSameLine.IsKind(SyntaxKind.CloseBraceToken))
                {
                    c.ReportIssue(rule, ifKeyword, [previousTokenInSameLine.ToSecondaryLocation()]);
                }
            },
            SyntaxKind.IfStatement);
        }

        private static bool TryGetPreviousTokenInSameLine(SyntaxToken token, out SyntaxToken previousToken)
        {
            previousToken = token.GetPreviousToken();
            return previousToken.Line() == token.Line();
        }
    }
}
