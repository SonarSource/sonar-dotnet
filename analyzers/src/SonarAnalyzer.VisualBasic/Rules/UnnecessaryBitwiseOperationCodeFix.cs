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

using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.VisualBasic.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class UnnecessaryBitwiseOperationCodeFix : UnnecessaryBitwiseOperationCodeFixBase
    {
        protected override Func<SyntaxNode> CreateNewRoot(SyntaxNode root, TextSpan diagnosticSpan, bool isReportingOnLeft)
        {
            if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is BinaryExpressionSyntax binary)
            {
                var newNode = isReportingOnLeft ? binary.Right : binary.Left;
                return () => root.ReplaceNode(binary, newNode.WithTrailingTrivia(binary.GetTrailingTrivia()).WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                return null;
            }
        }
    }
}
