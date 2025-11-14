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

using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class UnnecessaryBitwiseOperationCodeFix : UnnecessaryBitwiseOperationCodeFixBase
    {
        protected override Func<SyntaxNode> CreateNewRoot(SyntaxNode root, TextSpan diagnosticSpan, bool isReportingOnLeft) =>
            root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) switch
            {
                StatementSyntax statement => () => root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia),
                AssignmentExpressionSyntax assignment => () => root.ReplaceNode(assignment, assignment.Left.WithAdditionalAnnotations(Formatter.Annotation)),
                BinaryExpressionSyntax binary => () => root.ReplaceNode(binary, (isReportingOnLeft ? binary.Right : binary.Left).WithAdditionalAnnotations(Formatter.Annotation)),
                _ => null
            };
    }
}
