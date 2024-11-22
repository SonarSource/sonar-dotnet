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
    public class CopyPasteTokenAnalyzer : CopyPasteTokenAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;

        protected override bool IsUsingDirective(SyntaxNode node) =>
            node is ImportsStatementSyntax;

        protected override string GetCpdValue(SyntaxToken token)
        {
            if (token.IsAnyKind(SyntaxKind.DecimalLiteralToken, SyntaxKind.FloatingLiteralToken, SyntaxKind.IntegerLiteralToken))
            {
                return "$num";
            }
            else if (token.IsAnyKind(SyntaxKind.StringLiteralToken, SyntaxKind.InterpolatedStringTextToken))
            {
                return "$str";
            }
            else if (token.IsKind(SyntaxKind.CharacterLiteralToken))
            {
                return "$char";
            }
            else
            {
                return token.Text;
            }
        }
    }
}
