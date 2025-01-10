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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ExitStatementUsage : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3385";
        private const string MessageFormat = "Remove this 'Exit' statement.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => c.ReportIssue(rule, c.Node),
                SyntaxKind.ExitForStatement,
                SyntaxKind.ExitFunctionStatement,
                SyntaxKind.ExitPropertyStatement,
                SyntaxKind.ExitSubStatement,
                SyntaxKind.ExitTryStatement,
                SyntaxKind.ExitWhileStatement);

            context.RegisterNodeAction(
                c =>
                {
                    var parent = c.Node.Parent;
                    while(parent != null &&
                        !(parent is DoLoopBlockSyntax))
                    {
                        parent = parent.Parent;
                    }

                    if (parent == null ||
                        parent.IsKind(SyntaxKind.SimpleDoLoopBlock))
                    {
                        return;
                    }

                    c.ReportIssue(rule, c.Node);
                },
                SyntaxKind.ExitDoStatement);
        }
    }
}
