﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class TooManyLabelsInSwitch : TooManyLabelsInSwitchBase<SyntaxKind, SelectStatementSyntax>
{
    private static readonly ISet<SyntaxKind> IgnoredStatementsInSwitch = new HashSet<SyntaxKind> { SyntaxKind.ReturnStatement, SyntaxKind.ThrowStatement };

    protected override DiagnosticDescriptor Rule { get; } =
        DescriptorFactory.Create(DiagnosticId, string.Format(MessageFormat, "Select Case", "Case"),
            isEnabledByDefault: false);

    protected override SyntaxKind[] SyntaxKinds { get; } = [SyntaxKind.SelectStatement];

    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
        VisualBasicGeneratedCodeRecognizer.Instance;

    protected override SyntaxNode GetExpression(SelectStatementSyntax statement) =>
        statement.Expression;

    protected override int GetSectionsCount(SelectStatementSyntax statement) =>
        ((SelectBlockSyntax)statement.Parent).CaseBlocks.Count;

    protected override bool AllSectionsAreOneLiners(SelectStatementSyntax statement) =>
        ((SelectBlockSyntax)statement.Parent).CaseBlocks.All(HasOneLine);

    protected override Location GetKeywordLocation(SelectStatementSyntax statement) =>
        statement.SelectKeyword.GetLocation();

    private static bool HasOneLine(CaseBlockSyntax switchSection)
    {
        var statements = switchSection.Statements.Where(x => !x.IsAnyKind(IgnoredStatementsInSwitch)).ToArray();

        return statements.Length is 0
               // Statements can be multiline, we make sure the statement is on a single line.
               || (statements.Length is 1 && statements[0].GetLocation().GetLineSpan() is var lineSpan && lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line);
    }
}
