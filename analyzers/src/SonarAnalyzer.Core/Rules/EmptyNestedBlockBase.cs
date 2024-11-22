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

namespace SonarAnalyzer.Rules
{
    public abstract class EmptyNestedBlockBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S108";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract IEnumerable<SyntaxNode> EmptyBlocks(SyntaxNode node);

        protected override string MessageFormat => "Either remove or fill this block of code.";

        protected EmptyNestedBlockBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    foreach (var node in EmptyBlocks(c.Node))
                    {
                        c.ReportIssue(Rule, node);
                    }
                },
                SyntaxKinds);
    }
}
