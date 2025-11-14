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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class NoExceptionsInFinally : NoExceptionsInFinallyBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var walker = new ThrowInFinallyWalker(c, Rule);
                    foreach (var statement in ((FinallyBlockSyntax)c.Node).Statements)
                    {
                        walker.SafeVisit(statement);
                    }
                }, SyntaxKind.FinallyBlock);

        private class ThrowInFinallyWalker : SafeVisualBasicSyntaxWalker
        {
            private readonly SonarSyntaxNodeReportingContext context;
            private readonly DiagnosticDescriptor rule;

            public ThrowInFinallyWalker(SonarSyntaxNodeReportingContext context, DiagnosticDescriptor rule)
            {
                this.context = context;
                this.rule = rule;
            }

            public override void VisitThrowStatement(ThrowStatementSyntax node) =>
                context.ReportIssue(rule, node);

            public override void VisitFinallyBlock(FinallyBlockSyntax node)
            {
                // Do not call base to force the walker to stop. Another walker will take care of this finally clause.
            }
        }
    }
}
