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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CopyPasteTokenAnalyzer : CopyPasteTokenAnalyzerBase<SyntaxKind>
    {
        private static readonly HashSet<SyntaxKind> StringKinds =
            [
                SyntaxKind.StringLiteralToken,
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKindEx.SingleLineRawStringLiteralToken,
                SyntaxKindEx.MultiLineRawStringLiteralToken,
                SyntaxKindEx.Utf8StringLiteralToken,
                SyntaxKindEx.Utf8SingleLineRawStringLiteralToken,
                SyntaxKindEx.Utf8MultiLineRawStringLiteralToken
            ];

        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

        protected override bool IsUsingDirective(SyntaxNode node) =>
            node is UsingDirectiveSyntax;

        protected override string GetCpdValue(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return "$num";
            }
            else if (token.IsAnyKind(StringKinds))
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
