/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ControlCharacterInString : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2479";
        private const string MessageFormat = "Replace the control character at position {0} by its escape sequence '{1}'.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly IDictionary<char, string> EscapedControlCharacters = new Dictionary<char, string>
        {
            {'\u0000', "\\0"},
            {'\u0001', "\\u0001"},
            {'\u0002', "\\u0002"},
            {'\u0003', "\\u0003"},
            {'\u0004', "\\u0004"},
            {'\u0005', "\\u0005"},
            {'\u0006', "\\u0006"},
            {'\u0007', "\\a"},
            {'\u0008', "\\b"},
            {'\u0009', "\\t"},
            {'\u000A', "\\n"},
            {'\u000B', "\\v"},
            {'\u000C', "\\f"},
            {'\u000D', "\\r"},
            {'\u000E', "\\u000E"},
            {'\u000F', "\\u000F"},
            {'\u0010', "\\u0010"},
            {'\u0011', "\\u0011"},
            {'\u0012', "\\u0012"},
            {'\u0013', "\\u0013"},
            {'\u0014', "\\u0014"},
            {'\u0015', "\\u0015"},
            {'\u0016', "\\u0016"},
            {'\u0017', "\\u0017"},
            {'\u0018', "\\u0018"},
            {'\u0019', "\\u0019"},
            {'\u001A', "\\u001A"},
            {'\u001B', "\\u001B"},
            {'\u001C', "\\u001C"},
            {'\u001D', "\\u001D"},
            {'\u001E', "\\u001E"},
            {'\u001F', "\\u001F"},
            {'\u007F', "\\u007F"},
            {'\u1680', "\\u1680"},
            {'\u2000', "\\u2000"},
            {'\u2001', "\\u2001"},
            {'\u2002', "\\u2002"},
            {'\u2003', "\\u2003"},
            {'\u2004', "\\u2004"},
            {'\u2005', "\\u2005"},
            {'\u2006', "\\u2006"},
            {'\u2007', "\\u2007"},
            {'\u2008', "\\u2008"},
            {'\u2009', "\\u2009"},
            {'\u200A', "\\u200A"},
            {'\u200B', "\\u200B"},
            {'\u200C', "\\u200C"},
            {'\u200D', "\\u200D"},
            {'\u2028', "\\u2028"},
            {'\u2029', "\\u2029"},
            {'\u202F', "\\u202F"},
            {'\u205F', "\\u205F"},
            {'\u2060', "\\u2060"},
            {'\u3000', "\\u3000"},
            {'\uFEFF', "\\uFEFF"},
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckControlCharacter(c, ((LiteralExpressionSyntax)c.Node).Token.Text, 0),
                SyntaxKind.StringLiteralExpression,
                SyntaxKindEx.Utf8StringLiteralExpression);

            context.RegisterNodeAction(
                c => CheckControlCharacter(c, ((InterpolatedStringTextSyntax)c.Node).TextToken.Text, 1),
                SyntaxKind.InterpolatedStringText);
        }

        private static void CheckControlCharacter(SonarSyntaxNodeReportingContext c, string text, int displayPosIncrement)
        {
            if (IsInescapableString(c.Node) || IsInescepableInterpolatedString(c.Node.Parent) || IsInescapableUtf8String(c.Node))
            {
                return;
            }

            for (var charPos = 0; charPos < text.Length; charPos++)
            {
                if (EscapedControlCharacters.TryGetValue(text[charPos], out var escapeSequence))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, c.Node.GetLocation(), displayPosIncrement + charPos, escapeSequence));
                    return;
                }
            }
        }

        private static bool IsInescapableString(SyntaxNode syntaxNode) =>
            syntaxNode.GetFirstToken() is var token
            && (token.IsVerbatimStringLiteral()
                || token.IsAnyKind(SyntaxKindEx.SingleLineRawStringLiteralToken, SyntaxKindEx.MultiLineRawStringLiteralToken));

        private static bool IsInescepableInterpolatedString(SyntaxNode syntaxNode) =>
            syntaxNode.GetFirstToken().IsAnyKind(
                SyntaxKind.InterpolatedVerbatimStringStartToken,
                SyntaxKindEx.InterpolatedSingleLineRawStringStartToken,
                SyntaxKindEx.InterpolatedMultiLineRawStringStartToken);

        private static bool IsInescapableUtf8String(SyntaxNode syntaxNode) =>
            syntaxNode.GetFirstToken().IsAnyKind(SyntaxKindEx.Utf8SingleLineRawStringLiteralToken, SyntaxKindEx.Utf8MultiLineRawStringLiteralToken);
    }
}
