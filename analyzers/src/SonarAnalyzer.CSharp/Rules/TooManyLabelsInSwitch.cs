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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TooManyLabelsInSwitch : TooManyLabelsInSwitchBase<SyntaxKind, SwitchStatementSyntax>
{
    private static readonly ISet<SyntaxKind> IgnoredStatementsInSwitch = new HashSet<SyntaxKind> { SyntaxKind.BreakStatement, SyntaxKind.ReturnStatement, SyntaxKind.ThrowStatement };

    private static readonly ISet<SyntaxKind> TransparentSyntax = new HashSet<SyntaxKind>
    {
        SyntaxKind.Block,
        SyntaxKind.CatchClause,
        SyntaxKind.CheckedStatement,
        SyntaxKind.DoStatement,
        SyntaxKind.FinallyClause,
        SyntaxKind.FixedStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKindEx.ForEachVariableStatement,
        SyntaxKind.ForStatement,
        SyntaxKind.IfStatement,
        SyntaxKind.LockStatement,
        SyntaxKind.SwitchStatement,
        SyntaxKind.TryStatement,
        SyntaxKind.UncheckedStatement,
        SyntaxKind.UnsafeStatement,
        SyntaxKind.UsingStatement,
        SyntaxKind.WhileStatement
    };

    protected override DiagnosticDescriptor Rule { get; } =
        DescriptorFactory.Create(DiagnosticId, string.Format(MessageFormat, "switch", "case"),
            isEnabledByDefault: false);

    protected override SyntaxKind[] SyntaxKinds { get; } = [SyntaxKind.SwitchStatement];

    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
        CSharpGeneratedCodeRecognizer.Instance;

    protected override SyntaxNode GetExpression(SwitchStatementSyntax statement) =>
        statement.Expression;

    protected override int GetSectionsCount(SwitchStatementSyntax statement) =>
        statement.Sections.Count;

    protected override bool AllSectionsAreOneLiners(SwitchStatementSyntax statement) =>
        statement.Sections.All(HasOneLine);

    protected override Location GetKeywordLocation(SwitchStatementSyntax statement) =>
        statement.SwitchKeyword.GetLocation();

    private static bool HasOneLine(SwitchSectionSyntax switchSection) =>
        switchSection.Statements
            .SelectMany(x => x.DescendantNodesAndSelf(descendIntoChildren: c => c.IsAnyKind(TransparentSyntax)))
            .Count(x => !x.IsAnyKind(IgnoredStatementsInSwitch)) is 0 or 1;
}
